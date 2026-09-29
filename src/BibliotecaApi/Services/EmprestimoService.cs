using BibliotecaApi.Data;
using BibliotecaApi.Dtos;
using BibliotecaApi.Exceptions;
using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Services;

public interface IEmprestimoService
{
    Task<IReadOnlyList<EmprestimoResponse>> ListarAsync(bool? devolvido, CancellationToken ct);
    Task<EmprestimoResponse> ObterAsync(int id, CancellationToken ct);
    Task<EmprestimoResponse> CriarAsync(EmprestimoRequest request, CancellationToken ct);
    Task AtualizarAsync(int id, EmprestimoUpdateRequest request, CancellationToken ct);
    Task<EmprestimoResponse> RegistrarDevolucaoAsync(int id, CancellationToken ct);
    Task RemoverAsync(int id, CancellationToken ct);
}

public class EmprestimoService(AppDbContext db, TimeProvider clock) : IEmprestimoService
{
    public async Task<IReadOnlyList<EmprestimoResponse>> ListarAsync(bool? devolvido, CancellationToken ct)
    {
        var query = db.Emprestimos.AsNoTracking().Include(e => e.Livro).AsQueryable();

        if (devolvido.HasValue)
            query = devolvido.Value
                ? query.Where(e => e.DataDevolucao != null)
                : query.Where(e => e.DataDevolucao == null);

        var emprestimos = await query.OrderByDescending(e => e.DataEmprestimo).ToListAsync(ct);
        return emprestimos.Select(EmprestimoResponse.From).ToList();
    }

    public async Task<EmprestimoResponse> ObterAsync(int id, CancellationToken ct)
    {
        var emprestimo = await db.Emprestimos.AsNoTracking().Include(e => e.Livro)
                .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException($"Empréstimo {id} não encontrado.");
        return EmprestimoResponse.From(emprestimo);
    }

    public async Task<EmprestimoResponse> CriarAsync(EmprestimoRequest request, CancellationToken ct)
    {
        var agora = clock.GetUtcNow().UtcDateTime;
        GarantirPrazoValido(request.DataPrevistaDevolucao!.Value, agora);

        var livro = await db.Livros.FindAsync([request.LivroId], ct)
            ?? throw new BusinessRuleException($"O livro {request.LivroId} informado não existe.");

        if (!livro.Disponivel)
            throw new ConflictException($"O livro \"{livro.Titulo}\" não está disponível para empréstimo.");

        var emprestimo = new Emprestimo
        {
            Livro = livro,
            NomeLeitor = request.NomeLeitor.Trim(),
            EmailLeitor = request.EmailLeitor.Trim(),
            DataEmprestimo = agora,
            DataPrevistaDevolucao = request.DataPrevistaDevolucao.Value
        };
        livro.Disponivel = false;

        db.Emprestimos.Add(emprestimo);
        await db.SaveChangesAsync(ct); // um único SaveChanges: empréstimo e livro são gravados na mesma transação
        return EmprestimoResponse.From(emprestimo);
    }

    public async Task AtualizarAsync(int id, EmprestimoUpdateRequest request, CancellationToken ct)
    {
        var emprestimo = await db.Emprestimos.FindAsync([id], ct)
            ?? throw new NotFoundException($"Empréstimo {id} não encontrado.");

        if (emprestimo.Devolvido)
            throw new ConflictException("Não é possível alterar um empréstimo já devolvido.");

        GarantirPrazoValido(request.DataPrevistaDevolucao!.Value, emprestimo.DataEmprestimo);

        emprestimo.NomeLeitor = request.NomeLeitor.Trim();
        emprestimo.EmailLeitor = request.EmailLeitor.Trim();
        emprestimo.DataPrevistaDevolucao = request.DataPrevistaDevolucao.Value;

        await db.SaveChangesAsync(ct);
    }

    public async Task<EmprestimoResponse> RegistrarDevolucaoAsync(int id, CancellationToken ct)
    {
        var emprestimo = await db.Emprestimos.Include(e => e.Livro).FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException($"Empréstimo {id} não encontrado.");

        if (emprestimo.Devolvido)
            throw new ConflictException("Este empréstimo já foi devolvido.");

        emprestimo.DataDevolucao = clock.GetUtcNow().UtcDateTime;
        emprestimo.Livro!.Disponivel = true;

        await db.SaveChangesAsync(ct);
        return EmprestimoResponse.From(emprestimo);
    }

    public async Task RemoverAsync(int id, CancellationToken ct)
    {
        var emprestimo = await db.Emprestimos.Include(e => e.Livro).FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException($"Empréstimo {id} não encontrado.");

        // Excluir um empréstimo ainda ativo libera o exemplar novamente.
        if (!emprestimo.Devolvido)
            emprestimo.Livro!.Disponivel = true;

        db.Emprestimos.Remove(emprestimo);
        await db.SaveChangesAsync(ct);
    }

    private static void GarantirPrazoValido(DateTime dataPrevista, DateTime referencia)
    {
        if (dataPrevista.Date < referencia.Date)
            throw new BusinessRuleException("A data prevista de devolução não pode ser anterior à data do empréstimo.");
    }
}
