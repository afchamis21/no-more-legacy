using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.HTTP;

public record TestValidationRequest(List<FileContent> Files, FileGroupContext Context);

public record TestValidationResponse(List<FileContent> TestFiles);