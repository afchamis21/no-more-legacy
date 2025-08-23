using System.IO.Compression;
using NoMoreLegacy.Domain;
using NoMoreLegacy.Services.AI.Models;
using NoMoreLegacy.Util;

namespace NoMoreLegacy.Services;

public class FileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<List<FileContent>>> ParseZipFolder(IFormFile? file)
    {
    // Suas validações iniciais estão perfeitas
        if (file == null || file.Length == 0)
            return Result<List<FileContent>>.Fail(new BussinessError("Missing required file!"));

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return Result<List<FileContent>>.Fail(new BussinessError("File must be a .zip"));

        var files = new List<FileContent>();
        
        try
        {
            await using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            foreach (var entry in archive.Entries)
            {
                // Ignora entradas que são diretórios
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                byte[] contentBytes;
                await using (var entryStream = entry.Open())
                await using (var ms = new MemoryStream())
                {
                    await entryStream.CopyToAsync(ms);
                    contentBytes = ms.ToArray();
                }

                // 2. Validação de Binário: Verifica se o conteúdo é textual
                if (contentBytes.Contains((byte)0))
                {
                    return Result<List<FileContent>>.Fail(new BussinessError($"File '{entry.FullName}' appears to be binary and cannot be processed."));
                }

                // 3. Codificação Explícita: Converte para string usando UTF-8
                var body = System.Text.Encoding.UTF8.GetString(contentBytes);
                    
                files.Add(new FileContent(entry.FullName, body));
            }
        }
        catch (InvalidDataException)
        {
            // Ocorre se o arquivo não for um zip válido
            return Result<List<FileContent>>.Fail(new BussinessError("The provided file is not a valid .zip archive."));
        }

        _logger.LogInformation("Found and parsed [{Files}] files inside the zip binder", files.Count);
        return Result<List<FileContent>>.Ok(files);
    }
    
    // Dentro da classe FileService

    public async Task<byte[]> CreateZipArchiveAsync(List<FileContentDto> files)
    {
        _logger.LogInformation("Creating a new zip archive with [{FileCount}] files.", files.Count);

        using var memoryStream = new MemoryStream();
        
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);

                await using (var entryStream = entry.Open())
                {
                    await entryStream.WriteAsync(file.FileContent.AsMemory(0, file.FileContent.Length));
                }
            }
        }

        return memoryStream.ToArray();
    }
}
