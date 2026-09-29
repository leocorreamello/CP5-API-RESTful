using BibliotecaApi.Data;
using BibliotecaApi.Dtos;
using BibliotecaApi.Exceptions;
using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Services;

public interface IAutorService
{
    Task<IReadOnlyList<AutorResponse>> ListarAsync(CancellationToken ct);
    Task<AutorResponse> ObterAsync(int id, CancellationToken ct);
    Task<AutorResponse> CriarAsync(AutorRequest request, CancellationToken ct);
    Task AtualizarAsync(int id, AutorRequest request, CancellationToken ct);
    Task RemoverAsync(int id, CancellationToken ct);
}

public class AutorService(AppDbContext db) : IAutorService
{
    public async Task<IReadOnlyList<AutorResponse>> ListarAsync(CancellationToken ct) =>
        (await db.Autores.AsNoTracking().OrderBy(a => a.Nome).ToListAsync(ct))
            .Select(AutorResponse.From)
            .ToList();

    public async Task<AutorResponse> ObterAsync(int id, CancellationToken ct)
    {
        var autor = await db.Autores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException($"Autor {id} não encontrado.");
        return AutorResponse.From(autor);
    }

    public async Task<AutorResponse> CriarAsync(AutorRequest request, CancellationToken ct)
    {
        var autor = new Autor
        {
            Nome = request.Nome.Trim(),
            Nacionalidade = request.Nacionalidade?.Trim(),
            DataNascimento = ParaDateTime(request.DataNascimento)
        };

        db.Autores.Add(autor);
        await db.SaveChangesAsync(ct);
        return AutorResponse.From(autor);
    }

    public async Task AtualizarAsync(int id, AutorRequest request, CancellationToken ct)
    {
        var autor = await db.Autores.FindAsync([id], ct)
            ?? throw new NotFoundException($"Autor {id} não encontrado.");

        autor.Nome = request.Nome.Trim();
        autor.Nacionalidade = request.Nacionalidade?.Trim();
        autor.DataNascimento = ParaDateTime(request.DataNascimento);

        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(int id, CancellationToken ct)
    {
        var autor = await db.Autores.FindAsync([id], ct)
            ?? throw new NotFoundException($"Autor {id} não encontrado.");

        if (await db.Livros.AnyAsync(l => l.AutorId == id, ct))
            throw new ConflictException("Não é possível excluir um autor que possui livros cadastrados.");

        db.Autores.Remove(autor);
        await db.SaveChangesAsync(ct);
    }

    private static DateTime? ParaDateTime(DateOnly? data) =>
        data?.ToDateTime(TimeOnly.MinValue);
}
