using NoMoreLegacy.Domain;

namespace NoMoreLegacy.Services.AI.Clients.Converter;

public interface IFileConversor
{
    SupportedFramework Framework { get; }
}