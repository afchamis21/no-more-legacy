namespace NoMoreLegacy.Services.AI.Models;


public record FileGroupContext(
    List<string> Functionalities,
    List<Endpoint> Endpoints,
    List<string> DataModels,
    List<string> Dependencies,
    List<string> Integrations,
    List<LibraryMigration> Libraries // New property
);

public record Endpoint(
    string Url,
    string Method,
    List<string> Parameters,
    string Return
);

public record LibraryMigration(string Old, string New);
