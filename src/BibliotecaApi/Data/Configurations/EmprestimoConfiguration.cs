using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaApi.Data.Configurations;

public class EmprestimoConfiguration : IEntityTypeConfiguration<Emprestimo>
{
    public void Configure(EntityTypeBuilder<Emprestimo> builder)
    {
        builder.ToTable("TB_EMPRESTIMO");

        builder.HasKey(e => e.Id).HasName("PK_EMPRESTIMO");

        builder.Property(e => e.NomeLeitor).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EmailLeitor).IsRequired().HasMaxLength(150);

        // "Devolvido" é derivado de DataDevolucao, não é uma coluna.
        builder.Ignore(e => e.Devolvido);

        builder.HasIndex(e => e.LivroId).HasDatabaseName("IX_EMPRESTIMO_LIVRO");

        builder.HasOne(e => e.Livro)
            .WithMany(l => l.Emprestimos)
            .HasForeignKey(e => e.LivroId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EMPRESTIMO_LIVRO");
    }
}
