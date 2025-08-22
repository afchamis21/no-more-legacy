using System.IO.Compression;
using System.Text;
using NoMoreLegacy.Domain;
using NoMoreLegacy.Services.AI;
using NoMoreLegacy.Services.AI.Clients;
using NoMoreLegacy.Services.AI.Models;
using NoMoreLegacy.Util;

namespace NoMoreLegacy.Services;

public class ConversorService
{
    private readonly ILogger<ConversorService> _logger;
    private readonly FileService _fileService;
    private readonly ConversorOrchestrator _conversorOrchestrator;
    
    public ConversorService(ILogger<ConversorService> logger, FileService fileService, ConversorOrchestrator conversorOrchestrator)
    {
        _logger = logger;
        _fileService = fileService;
        _conversorOrchestrator = conversorOrchestrator;
    }

    public async Task<Result<byte[]>> DoConversion(IFormFile file, SupportedFramework framework)
    {
        _logger.LogInformation("Got a request to convert file [{FileName}] with size [{FileSize} Bytes]. Selected Framework [{}]", file.FileName, file.Length, framework);
        var parseFilesResult = await _fileService.ParseZipFolder(file);
        if (!parseFilesResult.Success)
        {
            _logger.LogError("Error parsing Zip Binder! [Err: {ErrorMessage}]. Stopping execution early", parseFilesResult.Error?.Message);
            return Result<byte[]>.Fail(parseFilesResult.Error!);
        }

        var parsedFiles = parseFilesResult.GetOrThrow();

        var conversionResult = await _conversorOrchestrator.DoPipeline(parsedFiles, framework);
        if (!conversionResult.Success)
        {
            _logger.LogError("Error converting project! [Err: {ErrorMessage}]. Stopping execution early", conversionResult.Error?.Message);
            return Result<byte[]>.Fail(conversionResult.Error!);
        }

        var convertedFiles = conversionResult.GetOrThrow();
        
        _logger.LogInformation("Converted [{FileCount}] file to modern frameworks!", convertedFiles.Count);

        var fileDtos = convertedFiles.ConvertAll(f => new FileContentDto(f.Name, Encoding.UTF8.GetBytes(f.Content)));

        // 2. Chama o serviço para criar o arquivo ZIP a partir dos DTOs
        var zipBytes = await _fileService.CreateZipArchiveAsync(fileDtos);

        // 3. Retorna os bytes do ZIP gerado
        return Result<byte[]>.Ok(zipBytes);
    }
}