using NoMoreLegacy.Services;
using NoMoreLegacy.Services.AI;
using NoMoreLegacy.Services.AI.Clients;
using NoMoreLegacy.Services.AI.Clients.Base;
using NoMoreLegacy.Services.AI.Clients.Context;
using NoMoreLegacy.Services.AI.Clients.Converter;
using NoMoreLegacy.Services.AI.Clients.Grouper;
using NoMoreLegacy.Services.AI.Clients.Merger;
using NoMoreLegacy.Services.AI.Clients.Scaffold;
using NoMoreLegacy.Services.AI.Clients.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

const string allowLocalUi = "AllowLocalUi";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowLocalUi,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

#region AI Clients
builder.Services.AddSingleton<AngularJsFileGrouperClient>();
builder.Services.AddSingleton<JaxRsFileGrouperClient>();
builder.Services.AddSingleton<JsfFileGrouperClient>();
builder.Services.AddSingleton<StrutFileGrouperClient>();

builder.Services.AddSingleton<AngularJsContextExtractorClient>();
builder.Services.AddSingleton<JaxRsContextExtractorClient>();
builder.Services.AddSingleton<JsfContextExtractorClient>();
builder.Services.AddSingleton<StrutContextExtractorClient>();

builder.Services.AddSingleton<AngularJsFileConverterClient>();
builder.Services.AddSingleton<JaxRsFileConverterClient>();
builder.Services.AddSingleton<JsfFileConverterClient>();
builder.Services.AddSingleton<StrutFileConverterClient>();

builder.Services.AddSingleton<AngularJsScaffoldClient>();
builder.Services.AddSingleton<JaxRsScaffoldClient>();
builder.Services.AddSingleton<JsfScaffoldClient>();
builder.Services.AddSingleton<StrutScaffoldClient>();

builder.Services.AddSingleton<AngularJsCodeMergerClient>();
builder.Services.AddSingleton<JaxRsCodeMergerClient>();
builder.Services.AddSingleton<JsfCodeMergerClient>();
builder.Services.AddSingleton<StrutCodeMergerClient>();

builder.Services.AddSingleton<AngularJsTestValidationClient>();
builder.Services.AddSingleton<JaxRsTestValidationClient>();
builder.Services.AddSingleton<JsfTestValidationClient>();
builder.Services.AddSingleton<StrutTestValidationClient>();

builder.Services.AddSingleton<ConversorOrchestrator>();
#endregion

#region Services
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<ConversorService>();
#endregion


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(allowLocalUi);

app.UseAuthorization();

app.MapControllers();

app.Run();
