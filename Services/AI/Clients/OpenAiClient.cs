using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using NoMoreLegacy.Util;
using OpenAI.Chat;

namespace NoMoreLegacy.Services.AI.Clients;

public abstract class OpenAiClient<TInput, TReturn>
{
    private readonly AzureOpenAIClient _client;
    private readonly AiClientDeployment _deployment;
    private readonly ILogger<OpenAiClient<TInput, TReturn>> _logger;

    private const int MaxAttempts = 4;
    private readonly TimeSpan _initialBackoff = TimeSpan.FromSeconds(1);
    private readonly Random _jitterer = new Random();
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    protected OpenAiClient(IConfiguration configuration, ILogger<OpenAiClient<TInput, TReturn>> logger, AiClientDeployment deployment)
    {
        _logger = logger;
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new Exception("Missing AzureOpenAI:Endpoint config!");
        var apiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new Exception("Missing AzureOpenAI:ApiKey config!");

        _deployment = deployment;
        
        _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            
    }

    private ChatClient GetChatClient()
    {
        return _client.GetChatClient(_deployment.DeploymentName);
    }

    protected abstract string SystemPrompt();

    public async Task<Result<TReturn>> Call(TInput input)
    {
        var attempts = 0;
        Result<TReturn> lastResult = null!;

        while (attempts < MaxAttempts)
        {
            lastResult = await Execute(input);
            if (lastResult.Success)
            {
                return lastResult;
            }

            attempts++;

            if (attempts >= MaxAttempts) continue;

            var backoff = TimeSpan.FromSeconds(Math.Pow(2, attempts - 1)) * _initialBackoff.TotalSeconds;

            var jitter = TimeSpan.FromMilliseconds(_jitterer.Next(0, 1000));
            
            await Task.Delay(backoff + jitter);
        }

        return Result<TReturn>.Fail(lastResult.Error!);
    }

    private async Task<Result<TReturn>> Execute(TInput input)
    {   
        var agentName = GetType().Name;
        _logger.LogInformation("[{AgentName}] AI call started for input type {InputType}.", agentName, typeof(TInput).Name);

        try
        {
            var chatClient = GetChatClient();

            var options = new ChatCompletionOptions()
            {
                Temperature = 1,
                // Temperature = 0.1f,
            };

            var requestPayload = JsonSerializer.Serialize(input, _jsonSerializerOptions);
            var messages = new List<ChatMessage>()
            {
                new SystemChatMessage(this.SystemPrompt()),
                new UserChatMessage(requestPayload),
            };

            _logger.LogInformation("[{AgentName}] Sending request to model. Input length: {InputLength} characters.", agentName, requestPayload.Length);

            var chatResponse = await chatClient.CompleteChatAsync(messages, options);
            
            var usage = chatResponse.Value.Usage;
            _logger.LogInformation(
                "[{AgentName}] Response received. Tokens Used: Prompt={PromptTokens}, Completion={CompletionTokens}, Total={TotalTokens}.",
                agentName, usage.InputTokenCount, usage.OutputTokenCount, usage.TotalTokenCount);

            var rawResponse = chatResponse.Value.Content[0].Text;
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                _logger.LogWarning("[{AgentName}] AI model returned an empty or null response.", agentName);
                return Result<TReturn>.Fail(new BussinessError("The AI model returned an empty response."));
            }

            try
            {
                var resultObject = JsonSerializer.Deserialize<TReturn>(rawResponse, _jsonSerializerOptions);
                if (resultObject is null)
                {
                    _logger.LogError("[{AgentName}] Deserialization resulted in a null object. Raw response: {RawResponse}", agentName, rawResponse);
                    return Result<TReturn>.Fail(new BussinessError("Failed to convert the JSON from the model to an object."));
                }
                
                _logger.LogInformation("[{AgentName}] Successfully deserialized response to {ReturnType}.", agentName, typeof(TReturn).Name);
                return Result<TReturn>.Ok(resultObject);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "[{AgentName}] JSON Deserialization failed. Raw response: {RawResponse}", agentName, rawResponse);
                return Result<TReturn>.Fail(new BussinessError($"The AI model returned a malformed JSON. Details: {jsonEx.Message}"));
            }
        }
        catch (RequestFailedException apiEx)
        {
            _logger.LogError(apiEx, "[{AgentName}] AI API call failed with status code {StatusCode}.", agentName, apiEx.Status);
            return Result<TReturn>.Fail(new BussinessError($"Error communicating with the AI service: {apiEx.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{AgentName}] An unexpected error occurred during the AI call.", agentName);
            return Result<TReturn>.Fail(new BussinessError($"An unexpected error occurred: {ex.Message}"));
        }
    } 
}