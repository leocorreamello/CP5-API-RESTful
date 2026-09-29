using Asp.Versioning;
using BibliotecaApi.Dtos;
using BibliotecaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers.V2;

/// <summary>Acervo de livros — versão 2 (listagem paginada e com busca por título).</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class LivrosController(ILivroService livroService) : ControllerBase
{
    /// <summary>Lista os livros de forma paginada. Diferente da v1, retorna um envelope com metadados de paginação.</summary>
    /// <response code="400">Parâmetros de paginação inválidos.</response>
    [HttpGet]
    [ProducesResponseType<PagedResponse<LivroResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<LivroResponse>>> Listar(
        [FromQuery] LivroConsultaV2 consulta, CancellationToken ct) =>
        Ok(await livroService.ListarPaginadoAsync(consulta, ct));

    /// <summary>Obtém um livro pelo id.</summary>
    /// <response code="404">Livro não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<LivroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LivroResponse>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await livroService.ObterAsync(id, ct));
}
