using NoMoreLegacy.Services;
using NoMoreLegacy.Services.AI;
using NoMoreLegacy.Services.AI.Clients;
using NoMoreLegacy.Services.AI.Clients.Converter;
using NoMoreLegacy.Services.AI.Clients.Scaffold;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<FileGrouperClient>();

builder.Services.AddSingleton<ContextExtractorClient>();

builder.Services.AddSingleton<AngularJsFileConverterClient>();
builder.Services.AddSingleton<JaxRsFileConverterClient>();
builder.Services.AddSingleton<JsfFileConverterClient>();
builder.Services.AddSingleton<StrutFileConverterClient>();

builder.Services.AddSingleton<AngularJsScaffoldClient>();
builder.Services.AddSingleton<JaxRsScaffoldClient>();
builder.Services.AddSingleton<JsfScaffoldClient>();
builder.Services.AddSingleton<StrutScaffoldClient>();

builder.Services.AddSingleton<CodeMergerClient>();
builder.Services.AddSingleton<TestValidationClient>();

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
