using Asp.Versioning;
using BibliotecaApi.Dtos;
using BibliotecaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers.V1;

/// <summary>Empréstimos de livros para leitores.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class EmprestimosController(IEmprestimoService emprestimoService) : ControllerBase
{
    /// <summary>Lista os empréstimos, do mais recente para o mais antigo.</summary>
    /// <param name="devolvido">true = somente devolvidos; false = somente em andamento.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmprestimoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmprestimoResponse>>> Listar(
        [FromQuery] bool? devolvido, CancellationToken ct) =>
        Ok(await emprestimoService.ListarAsync(devolvido, ct));

    /// <summary>Obtém um empréstimo pelo id.</summary>
    /// <response code="404">Empréstimo não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<EmprestimoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmprestimoResponse>> ObterPorId(int id, CancellationToken ct) =>
        Ok(await emprestimoService.ObterAsync(id, ct));

    /// <summary>Registra um empréstimo e marca o livro como indisponível.</summary>
    /// <response code="400">Dados inválidos, livro inexistente ou prazo anterior a hoje.</response>
    /// <response code="409">O livro já está emprestado.</response>
    [HttpPost]
    [ProducesResponseType<EmprestimoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmprestimoResponse>> Criar(EmprestimoRequest request, CancellationToken ct)
    {
        var emprestimo = await emprestimoService.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = emprestimo.Id }, emprestimo);
    }

    /// <summary>Atualiza leitor e prazo de um empréstimo ainda em andamento.</summary>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Empréstimo não encontrado.</response>
    /// <response code="409">O empréstimo já foi devolvido.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, EmprestimoUpdateRequest request, CancellationToken ct)
    {
        await emprestimoService.AtualizarAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Registra a devolução do livro, tornando-o disponível novamente.</summary>
    /// <response code="404">Empréstimo não encontrado.</response>
    /// <response code="409">O empréstimo já foi devolvido.</response>
    [HttpPost("{id:int}/devolucao")]
    [ProducesResponseType<EmprestimoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmprestimoResponse>> RegistrarDevolucao(int id, CancellationToken ct) =>
        Ok(await emprestimoService.RegistrarDevolucaoAsync(id, ct));

    /// <summary>Exclui um empréstimo (se ainda ativo, o livro volta a ficar disponível).</summary>
    /// <response code="404">Empréstimo não encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await emprestimoService.RemoverAsync(id, ct);
        return NoContent();
    }
}
