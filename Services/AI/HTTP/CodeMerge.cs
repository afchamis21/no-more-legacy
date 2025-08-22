using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.HTTP;

public record CodeMergeRequest(List<FileContent> Duplicates);

public record CodeMergeResponse(FileContent MergedFile);