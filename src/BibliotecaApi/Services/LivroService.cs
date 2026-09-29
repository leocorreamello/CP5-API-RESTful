using BibliotecaApi.Data;
using BibliotecaApi.Dtos;
using BibliotecaApi.Exceptions;
using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Services;

public interface ILivroService
{
    Task<IReadOnlyList<LivroResponse>> ListarAsync(int? autorId, bool? disponivel, CancellationToken ct);
    Task<PagedResponse<LivroResponse>> ListarPaginadoAsync(LivroConsultaV2 consulta, CancellationToken ct);
    Task<LivroResponse> ObterAsync(int id, CancellationToken ct);
    Task<LivroResponse> CriarAsync(LivroRequest request, CancellationToken ct);
    Task AtualizarAsync(int id, LivroRequest request, CancellationToken ct);
    Task RemoverAsync(int id, CancellationToken ct);
}

public class LivroService(AppDbContext db) : ILivroService
{
    public async Task<IReadOnlyList<LivroResponse>> ListarAsync(int? autorId, bool? disponivel, CancellationToken ct)
    {
        var livros = await Filtrar(autorId, disponivel, busca: null)
            .OrderBy(l => l.Titulo)
            .ToListAsync(ct);

        return livros.Select(LivroResponse.From).ToList();
    }

    public async Task<PagedResponse<LivroResponse>> ListarPaginadoAsync(LivroConsultaV2 consulta, CancellationToken ct)
    {
        var query = Filtrar(consulta.AutorId, consulta.Disponivel, consulta.Busca);

        var total = await query.CountAsync(ct);
        var livros = await query
            .OrderBy(l => l.Titulo).ThenBy(l => l.Id)
            .Skip((consulta.Pagina - 1) * consulta.TamanhoPagina)
            .Take(consulta.TamanhoPagina)
            .ToListAsync(ct);

        var totalPaginas = (int)Math.Ceiling(total / (double)consulta.TamanhoPagina);
        return new PagedResponse<LivroResponse>(
            consulta.Pagina, consulta.TamanhoPagina, total, totalPaginas,
            livros.Select(LivroResponse.From).ToList());
    }

    public async Task<LivroResponse> ObterAsync(int id, CancellationToken ct)
    {
        var livro = await db.Livros.AsNoTracking().Include(l => l.Autor).FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new NotFoundException($"Livro {id} não encontrado.");
        return LivroResponse.From(livro);
    }

    public async Task<LivroResponse> CriarAsync(LivroRequest request, CancellationToken ct)
    {
        var autor = await ObterAutorAsync(request.AutorId, ct);
        await GarantirIsbnUnicoAsync(request.Isbn, idAtual: null, ct);

        var livro = new Livro
        {
            Titulo = request.Titulo.Trim(),
            Isbn = request.Isbn,
            AnoPublicacao = request.AnoPublicacao,
            Autor = autor
        };

        db.Livros.Add(livro);
        await db.SaveChangesAsync(ct);
        return LivroResponse.From(livro);
    }

    public async Task AtualizarAsync(int id, LivroRequest request, CancellationToken ct)
    {
        var livro = await db.Livros.FindAsync([id], ct)
            ?? throw new NotFoundException($"Livro {id} não encontrado.");

        await ObterAutorAsync(request.AutorId, ct);
        await GarantirIsbnUnicoAsync(request.Isbn, idAtual: id, ct);

        livro.Titulo = request.Titulo.Trim();
        livro.Isbn = request.Isbn;
        livro.AnoPublicacao = request.AnoPublicacao;
        livro.AutorId = request.AutorId;

        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(int id, CancellationToken ct)
    {
        var livro = await db.Livros.FindAsync([id], ct)
            ?? throw new NotFoundException($"Livro {id} não encontrado.");

        if (await db.Emprestimos.AnyAsync(e => e.LivroId == id, ct))
            throw new ConflictException("Não é possível excluir um livro que possui empréstimos registrados.");

        db.Livros.Remove(livro);
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<Livro> Filtrar(int? autorId, bool? disponivel, string? busca)
    {
        var query = db.Livros.AsNoTracking().Include(l => l.Autor).AsQueryable();

        if (autorId.HasValue)
            query = query.Where(l => l.AutorId == autorId.Value);
        if (disponivel.HasValue)
            query = query.Where(l => l.Disponivel == disponivel.Value);
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(l => l.Titulo.ToLower().Contains(termo));
        }

        return query;
    }

    private async Task<Autor> ObterAutorAsync(int autorId, CancellationToken ct) =>
        await db.Autores.FindAsync([autorId], ct)
            ?? throw new BusinessRuleException($"O autor {autorId} informado não existe.");

    private async Task GarantirIsbnUnicoAsync(string isbn, int? idAtual, CancellationToken ct)
    {
        if (await db.Livros.AnyAsync(l => l.Isbn == isbn && l.Id != idAtual, ct))
            throw new ConflictException($"Já existe um livro cadastrado com o ISBN {isbn}.");
    }
}
