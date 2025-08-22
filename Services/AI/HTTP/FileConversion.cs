using NoMoreLegacy.Domain;
using NoMoreLegacy.Services.AI.Models;

namespace NoMoreLegacy.Services.AI.HTTP;

public record ConversionRequest(List<FileContent> Files, FileGroupContext Context);

public record ConversionResponse(List<FileContent> Files);