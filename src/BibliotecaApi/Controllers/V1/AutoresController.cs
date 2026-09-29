using Asp.Versioning;
using BibliotecaApi.Dtos;
using BibliotecaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers.V1;

/// <summary>Gerenciamento de autores.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AutoresController(IAutorService autorService) : ControllerBase
{
    /// <summary>Lista todos os autores, ordenados por nome.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AutorResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AutorResponse>>> Listar(CancellationToken ct) =>
        Ok(await autorService.ListarAsync(ct));

    /// <summary>Obtém um autor pelo id.</summary>
    /// <response code="404">Autor não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<AutorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutorResponse>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await autorService.ObterAsync(id, ct));

    /// <summary>Cadastra um novo autor.</summary>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType<AutorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AutorResponse>> Criar(AutorRequest request, CancellationToken ct)
    {
        var autor = await autorService.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = autor.Id }, autor);
    }

    /// <summary>Atualiza todos os dados de um autor.</summary>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Autor não encontrado.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, AutorRequest request, CancellationToken ct)
    {
        await autorService.AtualizarAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Exclui um autor que não possua livros cadastrados.</summary>
    /// <response code="404">Autor não encontrado.</response>
    /// <response code="409">O autor possui livros cadastrados.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await autorService.RemoverAsync(id, ct);
        return NoContent();
    }
}
