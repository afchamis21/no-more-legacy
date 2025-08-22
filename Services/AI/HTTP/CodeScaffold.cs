using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.HTTP;

public record CodeScaffoldRequest(List<LibraryMigration> Libraries, List<string> FileNames);
public record CodeScaffoldResponse(List<FileContent> Files);
