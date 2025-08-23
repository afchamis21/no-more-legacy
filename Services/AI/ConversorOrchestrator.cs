using System.Collections.Concurrent;
using NoMoreLegacy.Domain;
using NoMoreLegacy.Services.AI.Clients;
using NoMoreLegacy.Services.AI.Clients.Converter;
using NoMoreLegacy.Services.AI.Clients.Scaffold;
using NoMoreLegacy.Services.AI.HTTP;
using NoMoreLegacy.Services.AI.Models;
using NoMoreLegacy.Util;

namespace NoMoreLegacy.Services.AI;

public class ConversorOrchestrator
{
    private readonly ILogger<ConversorOrchestrator> _logger;
    
    #region Clients
    private readonly FileGrouperClient _fileGrouperClient;
    private readonly ContextExtractorClient _contextExtractorClient;
    private readonly TestValidationClient _testValidationClient;
    private readonly CodeMergerClient _codeMergerClient;
    #endregion

    #region Converters
    private readonly AngularJsFileConverterClient _angularJsFileConverterClient;
    private readonly JaxRsFileConverterClient _jaxRsFileConverterClient;
    private readonly JsfFileConverterClient _jsfFileConverterClient;
    private readonly StrutFileConverterClient _strutFileConverterClient; 
    #endregion

    #region Scaffold
    private readonly AngularJsScaffoldClient _angularJsScaffoldClient;
    private readonly JaxRsScaffoldClient _jaxRsScaffoldClient;
    private readonly JsfScaffoldClient _jsfScaffoldClient;
    private readonly StrutScaffoldClient _strutScaffoldClient;
    
    #endregion

    public ConversorOrchestrator(FileGrouperClient fileGrouperClient, ContextExtractorClient contextExtractorClient, TestValidationClient testValidationClient, CodeMergerClient codeMergerClient, AngularJsFileConverterClient angularJsFileConverterClient, JaxRsFileConverterClient jaxRsFileConverterClient, JsfFileConverterClient jsfFileConverterClient, StrutFileConverterClient strutFileConverterClient, ILogger<ConversorOrchestrator> logger, AngularJsScaffoldClient angularJsScaffoldClient, JaxRsScaffoldClient jaxRsScaffoldClient, JsfScaffoldClient jsfScaffoldClient, StrutScaffoldClient strutScaffoldClient)
    {
        _contextExtractorClient = contextExtractorClient;
        _testValidationClient = testValidationClient;
        _codeMergerClient = codeMergerClient;
        _angularJsFileConverterClient = angularJsFileConverterClient;
        _jaxRsFileConverterClient = jaxRsFileConverterClient;
        _jsfFileConverterClient = jsfFileConverterClient;
        _strutFileConverterClient = strutFileConverterClient;
        _logger = logger;
        _angularJsScaffoldClient = angularJsScaffoldClient;
        _jaxRsScaffoldClient = jaxRsScaffoldClient;
        _jsfScaffoldClient = jsfScaffoldClient;
        _strutScaffoldClient = strutScaffoldClient;
        _fileGrouperClient = fileGrouperClient;
    }

    // TODO think about retries later...
    //  Each call should have a separate retry count, etc.
    public async Task<Result<List<FileContent>>> DoPipeline(List<FileContent> files, SupportedFramework framework)
    {
        // Step 1: Group the initial set of files.
        var fileGroupsResult = await _fileGrouperClient.Call(new GroupFilesRequest(files));
        if (!fileGroupsResult.Success)
        {
            _logger.LogError("Group File Call failed with err: [{Err}]", fileGroupsResult.Error!.Message);
            return Result<List<FileContent>>.Fail(fileGroupsResult.Error!);
        }
        var fileGroups = fileGroupsResult.GetOrThrow();
        var fileMap = files.ToDictionary(f => f.Name);
        var libraries = new ConcurrentBag<LibraryMigration>();

        // Step 2: Process each file group in parallel.
        var processingTasks = fileGroups.Groups.Select(group => 
            ProcessFileGroupAsync(group, fileMap, framework, libraries)
        );
        var processingResults = await Task.WhenAll(processingTasks);
        
        // Step 3: Aggregate results and find duplicates.
        var uniqueFiles = new Dictionary<string, FileContent>();
        var duplicates = new Dictionary<string, List<FileContent>>();

        foreach (var result in processingResults)
        {
            if (!result.Success)
            {
                return Result<List<FileContent>>.Fail(result.Error!);
            }

            foreach (var file in result.GetOrThrow())
            {
                if (!uniqueFiles.TryGetValue(file.Name, out var existingFile))
                {
                    uniqueFiles.Add(file.Name, file);
                }
                else
                {
                    if (!duplicates.TryGetValue(file.Name, out var duplicateList))
                    {
                        duplicateList = [existingFile];
                    }
                    duplicateList.Add(file); 
                    duplicates[file.Name] = duplicateList;
                }
            }
        }

        var allFileNames = uniqueFiles.Values.Select(f => f.Name).ToList();
        var scaffoldAgent = GetScaffoldClient(framework);
        var scaffoldResult = await scaffoldAgent.Call(new CodeScaffoldRequest(libraries.ToList(), allFileNames));
        if (!scaffoldResult.Success)
        {
            _logger.LogError("Scaffold File Call failed with err: [{Err}]", scaffoldResult.Error!.Message);
            return Result<List<FileContent>>.Fail(scaffoldResult.Error!);
        }
        var scaffoldFiles = scaffoldResult.GetOrThrow().Files;
        
        foreach (var scaffoldFile in scaffoldFiles)
        {
            if (!uniqueFiles.TryGetValue(scaffoldFile.Name, out var existingFile))
            {
                uniqueFiles.Add(scaffoldFile.Name, scaffoldFile);
            }
            else
            {
                if (!duplicates.TryGetValue(scaffoldFile.Name, out var duplicateList))
                {
                    duplicateList = [existingFile];
                }
                duplicateList.Add(scaffoldFile); 
                duplicates[scaffoldFile.Name] = duplicateList;
            }
        }
        
        // Step 4: If there are duplicates, merge them.
        if (duplicates.Count <= 0) return Result<List<FileContent>>.Ok([..uniqueFiles.Values]);
        
        var mergeResult = await MergeDuplicateFilesAsync(duplicates, uniqueFiles);
        
        return !mergeResult.Success ? Result<List<FileContent>>.Fail(mergeResult.Error!) : // Propagate failure
            Result<List<FileContent>>.Ok([..uniqueFiles.Values]);
    }
    
    /// <summary>
    /// Processes a single group of files by extracting context, converting, and validating tests.
    /// This method contains the logic that runs in parallel for each group.
    /// </summary>
    private async Task<Result<List<FileContent>>> ProcessFileGroupAsync(
        FileGroup group,
        Dictionary<string, FileContent> fileMap,
        SupportedFramework framework,
        ConcurrentBag<LibraryMigration> libraries)
    {
        // TODO We might want to make an agent to first separate back-end from UI code and then process a different pipeline for each... (This would be something.......)
        var filesInGroup = BuildFileListForGroup(group, fileMap);

        var contextResult = await _contextExtractorClient.Call(new ContextExtractionRequest(group.Group, group.Description, filesInGroup));
        if (!contextResult.Success)
        {
            _logger.LogError("Context Extractor Call failed for group [{Group}]", group.Group);
            return Result<List<FileContent>>.Fail(contextResult.Error!);
        }
        var context = contextResult.GetOrThrow().Context;
        context.Libraries.ForEach(libraries.Add);

        var conversor = GetConversor(framework);

        var conversionResult = await conversor.Call(new ConversionRequest(filesInGroup, context));
        if (!conversionResult.Success)
        {
            _logger.LogError("Conversion Call failed for group [{Group}]", group.Group);
            return Result<List<FileContent>>.Fail(conversionResult.Error!);
        }
        var convertedFiles = conversionResult.GetOrThrow().Files;

        var validationResult = await _testValidationClient.Call(new TestValidationRequest(convertedFiles, context));
        if (!validationResult.Success)
        {
            _logger.LogError("Test Validation Call failed for group [{Group}]", group.Group);
            return Result<List<FileContent>>.Fail(validationResult.Error!);
        }
        var testFiles = validationResult.GetOrThrow().TestFiles;

        return Result<List<FileContent>>.Ok([..convertedFiles, ..testFiles]);
    }

    /// <summary>
    /// Takes a dictionary of duplicate files, calls the merge client for each list,
    /// and updates the unique files dictionary with the merged results.
    /// </summary>
    private async Task<Result> MergeDuplicateFilesAsync(
        Dictionary<string, List<FileContent>> duplicates,
        Dictionary<string, FileContent> uniqueFiles)
    {
        var mergeTasks = duplicates.Values.Select(filesToMerge => 
            _codeMergerClient.Call(new CodeMergeRequest(filesToMerge))
        );

        var mergeResults = await Task.WhenAll(mergeTasks);

        foreach (var mergeResult in mergeResults)
        {
            if (!mergeResult.Success)
            {
                _logger.LogError("Merge Call failed with err: [{Err}]", mergeResult.Error!.Message);
                return Result.Fail(mergeResult.Error!); // Fail the entire merge operation
            }

            var mergedFile = mergeResult.GetOrThrow().MergedFile;
            uniqueFiles[mergedFile.Name] = mergedFile; // Add the final merged file to the unique dictionary
        }

        return Result.Ok();
    }

    private List<FileContent> BuildFileListForGroup(FileGroup group, Dictionary<string, FileContent> files)
    {
        var fileContents = group.Files
            .Select(f => {
                if (files.TryGetValue(f, out var fileContent))
                {
                    return fileContent;
                }
            
                _logger.LogWarning("File [{FileName}] not found in the file dictionary! This may indicate an issue with the grouping agent.", f);
                return null;
            })
            .Where(f => f is not null)
            .ToList();

        return fileContents!;
    }
    
    private OpenAiClient<ConversionRequest, ConversionResponse> GetConversor(SupportedFramework framework) => framework switch
    {
        SupportedFramework.AngularJs => _angularJsFileConverterClient,
        SupportedFramework.JaxRs => _jaxRsFileConverterClient,
        SupportedFramework.Jsf => _jsfFileConverterClient,
        SupportedFramework.Struts => _strutFileConverterClient,
        _ => throw new ArgumentOutOfRangeException(nameof(framework), framework, null)
    };
    
    private OpenAiClient<CodeScaffoldRequest, CodeScaffoldResponse> GetScaffoldClient(SupportedFramework framework) => framework switch
    {
        SupportedFramework.AngularJs => _angularJsScaffoldClient,
        SupportedFramework.JaxRs => _jaxRsScaffoldClient,
        SupportedFramework.Jsf => _jsfScaffoldClient,
        SupportedFramework.Struts => _strutScaffoldClient,
        _ => throw new ArgumentOutOfRangeException(nameof(framework), framework, null)
    };
}