using NoMoreLegacy.Domain;
using NoMoreLegacy.Services.AI.HTTP;
using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.Clients.Base;

public class DefineFrameworkClient(IConfiguration configuration, ILogger<OpenAiClient<DefineFrameworkRequest, DefineFrameworkResponse>> logger, AiClientDeployment deployment): 
    OpenAiClient<DefineFrameworkRequest, DefineFrameworkResponse>(configuration, logger, deployment)
{
    protected override string SystemPrompt()
    {
        throw new NotImplementedException(); // TODO I'll do this at a later point
    }

    private static string _getSupportedFrameworks()
    {
        return string.Join(", ", Enum.GetNames<SupportedFramework>());
    }
}