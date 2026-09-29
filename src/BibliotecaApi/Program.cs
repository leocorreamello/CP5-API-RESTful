using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using BibliotecaApi.Configuration;
using BibliotecaApi.Data;
using BibliotecaApi.Middleware;
using BibliotecaApi.Services;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// --- Banco de dados (Oracle) ---
var connectionString = builder.Configuration.GetConnectionString("OracleFiap")
    ?? throw new InvalidOperationException("Connection string 'OracleFiap' não configurada. Veja o README.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString, oracle =>
        // Compatibilidade com Oracle 19c+: bool vira NUMBER(1) (o tipo BOOLEAN só existe no 23ai).
        oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

// --- Serviços da aplicação ---
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<ILivroService, LivroService>();
builder.Services.AddScoped<IEmprestimoService, EmprestimoService>();

// --- Tratamento global de erros (ProblemDetails) ---
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// --- Controllers e versionamento por URL: /api/v1/..., /api/v2/... ---
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty; // Swagger UI na raiz: http://localhost:5080
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions.Reverse())
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Biblioteca API {description.GroupName.ToUpperInvariant()}");
});

app.MapControllers();

app.Run();

// Necessário para os testes de integração (WebApplicationFactory).
public partial class Program;
