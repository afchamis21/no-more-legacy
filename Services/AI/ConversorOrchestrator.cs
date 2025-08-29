using System.Collections.Concurrent;
using NoMoreLegacy.Domain;
using NoMoreLegacy.Services.AI.Clients;
using NoMoreLegacy.Services.AI.Clients.Context;
using NoMoreLegacy.Services.AI.Clients.Converter;
using NoMoreLegacy.Services.AI.Clients.Grouper;
using NoMoreLegacy.Services.AI.Clients.Merger;
using NoMoreLegacy.Services.AI.Clients.Scaffold;
using NoMoreLegacy.Services.AI.Clients.Validation;
using NoMoreLegacy.Services.AI.HTTP;
using NoMoreLegacy.Services.AI.Models;
using NoMoreLegacy.Util;

namespace NoMoreLegacy.Services.AI;


public class ConversorOrchestrator
{
    private readonly ILogger<ConversorOrchestrator> _logger;

    #region Grouper
    private readonly AngularJsFileGrouperClient _angularJsFileGrouperClient;
    private readonly JaxRsFileGrouperClient _jaxRsFileGrouperClient;
    private readonly JsfFileGrouperClient _jsfFileGrouperClient;
    private readonly StrutFileGrouperClient _strutFileGrouperClient;
    #endregion
    
    #region Context
    private readonly AngularJsContextExtractorClient _angularJsContextExtractorClient;
    private readonly JaxRsContextExtractorClient _jaxRsContextExtractorClient;
    private readonly JsfContextExtractorClient _jsfContextExtractorClient;
    private readonly StrutContextExtractorClient _strutContextExtractorClient;
    #endregion
    
    #region Converters
    private readonly AngularJsFileConverterClient _angularJsFileConverterClient;
    private readonly JaxRsFileConverterClient _jaxRsFileConverterClient;
    private readonly JsfFileConverterClient _jsfFileConverterClient;
    private readonly StrutFileConverterClient _strutFileConverterClient; 
    #endregion

    #region Validation
    private readonly AngularJsTestValidationClient _angularJsTestValidationClient;
    private readonly JaxRsTestValidationClient _jaxRsTestValidationClient;
    private readonly JsfTestValidationClient _jsfTestValidationClient;
    private readonly StrutTestValidationClient _strutTestValidationClient;
    #endregion
    
    #region Scaffold
    private readonly AngularJsScaffoldClient _angularJsScaffoldClient;
    private readonly JaxRsScaffoldClient _jaxRsScaffoldClient;
    private readonly JsfScaffoldClient _jsfScaffoldClient;
    private readonly StrutScaffoldClient _strutScaffoldClient;
    #endregion
    
    #region Merger
    private readonly AngularJsCodeMergerClient _angularJsCodeMergerClient;
    private readonly JaxRsCodeMergerClient _jaxRsCodeMergerClient;
    private readonly JsfCodeMergerClient _jsfCodeMergerClient;
    private readonly StrutCodeMergerClient _strutCodeMergerClient;
    #endregion
    
    public ConversorOrchestrator(AngularJsFileConverterClient angularJsFileConverterClient, JaxRsFileConverterClient jaxRsFileConverterClient, JsfFileConverterClient jsfFileConverterClient, StrutFileConverterClient strutFileConverterClient, ILogger<ConversorOrchestrator> logger, AngularJsScaffoldClient angularJsScaffoldClient, JaxRsScaffoldClient jaxRsScaffoldClient, JsfScaffoldClient jsfScaffoldClient, StrutScaffoldClient strutScaffoldClient, AngularJsFileGrouperClient angularJsFileGrouperClient, JaxRsFileGrouperClient jaxRsFileGrouperClient, JsfFileGrouperClient jsfFileGrouperClient, StrutFileGrouperClient strutFileGrouperClient, AngularJsContextExtractorClient angularJsContextExtractorClient, JaxRsContextExtractorClient jaxRsContextExtractorClient, JsfContextExtractorClient jsfContextExtractorClient, StrutContextExtractorClient strutContextExtractorClient, AngularJsTestValidationClient angularJsTestValidationClient, JaxRsTestValidationClient jaxRsTestValidationClient, JsfTestValidationClient jsfTestValidationClient, StrutTestValidationClient strutTestValidationClient, AngularJsCodeMergerClient angularJsCodeMergerClient, JaxRsCodeMergerClient jaxRsCodeMergerClient, JsfCodeMergerClient jsfCodeMergerClient, StrutCodeMergerClient strutCodeMergerClient)
    {

        _angularJsFileConverterClient = angularJsFileConverterClient;
        _jaxRsFileConverterClient = jaxRsFileConverterClient;
        _jsfFileConverterClient = jsfFileConverterClient;
        _strutFileConverterClient = strutFileConverterClient;
        _angularJsScaffoldClient = angularJsScaffoldClient;
        _jaxRsScaffoldClient = jaxRsScaffoldClient;
        _jsfScaffoldClient = jsfScaffoldClient;
        _strutScaffoldClient = strutScaffoldClient;
        _angularJsFileGrouperClient = angularJsFileGrouperClient;
        _jaxRsFileGrouperClient = jaxRsFileGrouperClient;
        _jsfFileGrouperClient = jsfFileGrouperClient;
        _strutFileGrouperClient = strutFileGrouperClient;
        _angularJsContextExtractorClient = angularJsContextExtractorClient;
        _jaxRsContextExtractorClient = jaxRsContextExtractorClient;
        _jsfContextExtractorClient = jsfContextExtractorClient;
        _strutContextExtractorClient = strutContextExtractorClient;
        _angularJsTestValidationClient = angularJsTestValidationClient;
        _jaxRsTestValidationClient = jaxRsTestValidationClient;
        _jsfTestValidationClient = jsfTestValidationClient;
        _strutTestValidationClient = strutTestValidationClient;
        _angularJsCodeMergerClient = angularJsCodeMergerClient;
        _jaxRsCodeMergerClient = jaxRsCodeMergerClient;
        _jsfCodeMergerClient = jsfCodeMergerClient;
        _strutCodeMergerClient = strutCodeMergerClient;
        
        _logger = logger;
    }
    
    public async Task<Result<List<FileContent>>> DoPipeline(List<FileContent> files, SupportedFramework framework)
    {
        _logger.LogInformation("Starting conversion pipeline for {Framework} with {FileCount} files.", framework, files.Count);

        // Step 1: Group the initial set of files.
        var pipelineClients = GetPipelineClients(framework);
        
        _logger.LogInformation("Step 1: Grouping files...");
        var fileGroupsResult = await pipelineClients.FileGrouperClient.Call(new GroupFilesRequest(files));
        if (!fileGroupsResult.Success)
        {
            _logger.LogError("Group File Call failed with err: [{Err}]", fileGroupsResult.Error!.Message);
            return Result<List<FileContent>>.Fail(fileGroupsResult.Error!);
        }
        var fileGroups = fileGroupsResult.GetOrThrow();
        _logger.LogInformation("Grouping successful. Created {GroupCount} file groups.", fileGroups.Groups.Count);

        var fileMap = files.ToDictionary(f => f.Name);
        var libraries = new ConcurrentBag<LibraryMigration>();

        // Step 2: Process each file group in parallel.
        _logger.LogInformation("Step 2: Starting parallel processing for {GroupCount} file groups.", fileGroups.Groups.Count);
        var processingTasks = fileGroups.Groups.Select(group => 
            ProcessFileGroupAsync(group, fileMap, pipelineClients, libraries)
        );
        var processingResults = await Task.WhenAll(processingTasks);
        _logger.LogInformation("Parallel processing completed.");
        
        // Step 3: Aggregate results and find duplicates.
        _logger.LogInformation("Step 3: Aggregating results and checking for duplicates.");
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
        _logger.LogInformation("Aggregation complete. Found {UniqueCount} unique files and {DuplicateCount} files with duplicates.", uniqueFiles.Count, duplicates.Count);
        
        var allFileNames = uniqueFiles.Values.Select(f => f.Name).ToList();

        _logger.LogInformation("Step 4: Scaffolding necessary library and project files.");
        var scaffoldResult = await pipelineClients.CodeScaffoldClient.Call(new CodeScaffoldRequest(libraries.ToList(), allFileNames));
        if (!scaffoldResult.Success)
        {
            _logger.LogError("Scaffold File Call failed with err: [{Err}]", scaffoldResult.Error!.Message);
            return Result<List<FileContent>>.Fail(scaffoldResult.Error!);
        }
        var scaffoldFiles = scaffoldResult.GetOrThrow().Files;
        _logger.LogInformation("Scaffolding successful. Generated {ScaffoldFileCount} new files.", scaffoldFiles.Count);
        
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
        if (duplicates.Count <= 0)
        {
            _logger.LogInformation("No duplicate files found to merge. Pipeline finished.");
            return Result<List<FileContent>>.Ok([..uniqueFiles.Values]);
        }
        
        _logger.LogInformation("Step 5: Merging {DuplicateCount} sets of duplicate files.", duplicates.Count);
        var mergeResult = await MergeDuplicateFilesAsync(duplicates, uniqueFiles, pipelineClients);
        
        if (!mergeResult.Success)
        {
            // Error is logged inside MergeDuplicateFilesAsync
            return Result<List<FileContent>>.Fail(mergeResult.Error!);
        }
        
        _logger.LogInformation("Pipeline completed successfully. Returning {TotalFileCount} files.", uniqueFiles.Values.Count);
        return Result<List<FileContent>>.Ok([..uniqueFiles.Values]);
    }
    
    /// <summary>
    /// Processes a single group of files by extracting context, converting, and validating tests.
    /// This method contains the logic that runs in parallel for each group.
    /// </summary>
    private async Task<Result<List<FileContent>>> ProcessFileGroupAsync(
        FileGroup group,
        Dictionary<string, FileContent> fileMap,
        ConversionPipeline pipelineClients,
        ConcurrentBag<LibraryMigration> libraries)
    {
        var filesInGroup = BuildFileListForGroup(group, fileMap);
        _logger.LogInformation("Processing file group '{GroupName}' with {FileCount} files. Description {GroupDescription}", group.Group, filesInGroup.Count, group.Description);

        _logger.LogInformation("[{GroupName}] ==> Extracting context...", group.Group);
        var contextResult = await pipelineClients.ContextExtractorClient.Call(new ContextExtractionRequest(group.Group, group.Description, filesInGroup));
        if (!contextResult.Success)
        {
            _logger.LogError("Context Extractor Call failed for group [{Group}] with error: {Error}", group.Group, contextResult.Error!.Message);
            return Result<List<FileContent>>.Fail(contextResult.Error!);
        }
        var context = contextResult.GetOrThrow().Context;
        context.Libraries.ForEach(libraries.Add);
        _logger.LogInformation("[{GroupName}] ==> Context extraction successful.", group.Group);

        _logger.LogInformation("[{GroupName}] ==> Converting code...", group.Group);
        var conversionResult = await pipelineClients.CodeConverterClient.Call(new ConversionRequest(filesInGroup, context));
        if (!conversionResult.Success)
        {
            _logger.LogError("Conversion Call failed for group [{Group}] with error: {Error}", group.Group, conversionResult.Error!.Message);
            return Result<List<FileContent>>.Fail(conversionResult.Error!);
        }
        var convertedFiles = conversionResult.GetOrThrow().Files;
        _logger.LogInformation("[{GroupName}] ==> Code conversion successful. Generated {FileCount} files.", group.Group, convertedFiles.Count);

        _logger.LogInformation("[{GroupName}] ==> Validating and generating tests...", group.Group);
        var validationResult = await pipelineClients.TestValidationClient.Call(new TestValidationRequest(convertedFiles, context));
        if (!validationResult.Success)
        {
            _logger.LogError("Test Validation Call failed for group [{Group}] with error: {Error}", group.Group, validationResult.Error!.Message);
            return Result<List<FileContent>>.Fail(validationResult.Error!);
        }
        var testFiles = validationResult.GetOrThrow().TestFiles;
        _logger.LogInformation("[{GroupName}] ==> Test validation successful. Generated {FileCount} test files.", group.Group, testFiles.Count);

        _logger.LogInformation("Successfully processed group '{GroupName}'.", group.Group);
        return Result<List<FileContent>>.Ok([..convertedFiles, ..testFiles]);
    }

    /// <summary>
    /// Takes a dictionary of duplicate files, calls the merge client for each list,
    /// and updates the unique files dictionary with the merged results.
    /// </summary>
    private async Task<Result> MergeDuplicateFilesAsync(
        Dictionary<string, List<FileContent>> duplicates,
        Dictionary<string, FileContent> uniqueFiles,
        ConversionPipeline pipelineClients
    )
    {
        _logger.LogInformation("Starting merge process for {DuplicateCount} sets of duplicate files.", duplicates.Count);
        
        var mergeTasks = duplicates.Select(kvp => 
            pipelineClients.CodeMerger.Call(new CodeMergeRequest(kvp.Value))
                .ContinueWith(task => new { FileName = kvp.Key, Result = task.Result })
        );

        var mergeResults = await Task.WhenAll(mergeTasks);

        foreach (var mergeResult in mergeResults)
        {
            if (!mergeResult.Result.Success)
            {
                _logger.LogError("Merge Call failed for file [{FileName}] with err: [{Err}]", mergeResult.FileName, mergeResult.Result.Error!.Message);
                return Result.Fail(mergeResult.Result.Error!); // Fail the entire merge operation
            }

            var mergedFile = mergeResult.Result.GetOrThrow().MergedFile;
            uniqueFiles[mergedFile.Name] = mergedFile; // Add the final merged file to the unique dictionary
            _logger.LogInformation("Successfully merged file: {FileName}", mergedFile.Name);
        }

        _logger.LogInformation("All duplicate files merged successfully.");
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

    private ConversionPipeline GetPipelineClients(SupportedFramework framework) => framework switch
    {
        SupportedFramework.AngularJs => new ConversionPipeline(_angularJsFileGrouperClient, _angularJsContextExtractorClient, _angularJsFileConverterClient, _angularJsTestValidationClient, _angularJsScaffoldClient, _angularJsCodeMergerClient),
        SupportedFramework.JaxRs => new ConversionPipeline(_jaxRsFileGrouperClient, _jaxRsContextExtractorClient, _jaxRsFileConverterClient, _jaxRsTestValidationClient, _jaxRsScaffoldClient, _jaxRsCodeMergerClient),
        SupportedFramework.Jsf => new ConversionPipeline(_jsfFileGrouperClient, _jsfContextExtractorClient, _jsfFileConverterClient, _jsfTestValidationClient, _jsfScaffoldClient, _jsfCodeMergerClient),
        SupportedFramework.Struts => new ConversionPipeline(_strutFileGrouperClient, _strutContextExtractorClient, _strutFileConverterClient, _strutTestValidationClient, _strutScaffoldClient, _strutCodeMergerClient),
        _ => throw new ArgumentOutOfRangeException(nameof(framework), framework, null)
    };
}