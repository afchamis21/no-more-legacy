using NoMoreLegacy.Domain;
using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.HTTP;

public record DefineFrameworkRequest(List<FileContent> Files);

public record DefineFrameworkResponse(SupportedFramework Framework);