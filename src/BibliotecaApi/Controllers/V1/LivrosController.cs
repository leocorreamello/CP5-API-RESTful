using Asp.Versioning;
using BibliotecaApi.Dtos;
using BibliotecaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers.V1;

/// <summary>Gerenciamento do acervo de livros.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class LivrosController(ILivroService livroService) : ControllerBase
{
    /// <summary>Lista os livros, com filtros opcionais.</summary>
    /// <param name="autorId">Retorna apenas livros deste autor.</param>
    /// <param name="disponivel">Retorna apenas livros disponíveis (true) ou emprestados (false).</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LivroResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LivroResponse>>> Listar(
        [FromQuery] int? autorId, [FromQuery] bool? disponivel, CancellationToken ct) =>
        Ok(await livroService.ListarAsync(autorId, disponivel, ct));

    /// <summary>Obtém um livro pelo id.</summary>
    /// <response code="404">Livro não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<LivroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LivroResponse>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await livroService.ObterAsync(id, ct));

    /// <summary>Cadastra um novo livro (inicia como disponível).</summary>
    /// <response code="400">Dados inválidos ou autor inexistente.</response>
    /// <response code="409">Já existe um livro com o mesmo ISBN.</response>
    [HttpPost]
    [ProducesResponseType<LivroResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LivroResponse>> Criar(LivroRequest request, CancellationToken ct)
    {
        var livro = await livroService.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }

    /// <summary>Atualiza todos os dados de um livro.</summary>
    /// <response code="400">Dados inválidos ou autor inexistente.</response>
    /// <response code="404">Livro não encontrado.</response>
    /// <response code="409">Já existe outro livro com o mesmo ISBN.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, LivroRequest request, CancellationToken ct)
    {
        await livroService.AtualizarAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Exclui um livro que não possua empréstimos registrados.</summary>
    /// <response code="404">Livro não encontrado.</response>
    /// <response code="409">O livro possui empréstimos registrados.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await livroService.RemoverAsync(id, ct);
        return NoContent();
    }
}
