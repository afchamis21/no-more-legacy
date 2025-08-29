using NoMoreLegacy.Services.AI.Clients;
using NoMoreLegacy.Services.AI.HTTP;

namespace NoMoreLegacy.Services.AI.Models;

public record ConversionPipeline(
    OpenAiClient<GroupFilesRequest, GroupFilesResponse> FileGrouperClient,
    OpenAiClient<ContextExtractionRequest, ContextExtractionResponse> ContextExtractorClient,
    OpenAiClient<ConversionRequest, ConversionResponse> CodeConverterClient,
    OpenAiClient<TestValidationRequest, TestValidationResponse> TestValidationClient,
    OpenAiClient<CodeScaffoldRequest, CodeScaffoldResponse> CodeScaffoldClient,
    OpenAiClient<CodeMergeRequest, CodeMergeResponse> CodeMerger
);