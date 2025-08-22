using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.HTTP;

public record GroupFilesRequest(List<FileContent> Files);

public record GroupFilesResponse(List<FileGroup> Groups);