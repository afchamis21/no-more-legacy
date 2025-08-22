using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.HTTP;

public record ContextExtractionRequest(int Group, string description ,List<FileContent> Files);

public record ContextExtractionResponse(FileGroupContext Context);