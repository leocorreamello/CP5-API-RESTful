using System.Text.RegularExpressions;
using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Oracle guarda identificadores sem aspas em MAIÚSCULAS: usar AUTOR_ID (e não "AutorId")
        // permite consultar as tabelas no SQL Developer sem precisar de aspas duplas.
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetProperties()))
        {
            var nome = property.GetColumnName();
            property.SetColumnName(Regex.Replace(nome, "([a-z0-9])([A-Z])", "$1_$2").ToUpperInvariant());
        }
    }
}
