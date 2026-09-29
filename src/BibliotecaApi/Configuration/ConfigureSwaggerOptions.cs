using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BibliotecaApi.Configuration;

/// <summary>Cria um documento Swagger para cada versão da API descoberta.</summary>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Biblioteca Comunitária API",
                Version = description.ApiVersion.ToString(),
                Description = description.ApiVersion.MajorVersion == 1
                    ? "API RESTful para gerenciar autores, livros e empréstimos de uma biblioteca comunitária."
                    : "Versão 2: listagem de livros paginada, com busca por título e filtros."
            });
        }
    }
}
