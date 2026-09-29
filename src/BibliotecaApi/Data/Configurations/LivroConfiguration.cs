using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaApi.Data.Configurations;

public class LivroConfiguration : IEntityTypeConfiguration<Livro>
{
    public void Configure(EntityTypeBuilder<Livro> builder)
    {
        builder.ToTable("TB_LIVRO");

        builder.HasKey(l => l.Id).HasName("PK_LIVRO");

        builder.Property(l => l.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Isbn).IsRequired().HasMaxLength(13);

        builder.HasIndex(l => l.Isbn).IsUnique().HasDatabaseName("UX_LIVRO_ISBN");
        builder.HasIndex(l => l.AutorId).HasDatabaseName("IX_LIVRO_AUTOR");

        // Um autor não pode ser removido enquanto possuir livros (Oracle: NO ACTION).
        builder.HasOne(l => l.Autor)
            .WithMany(a => a.Livros)
            .HasForeignKey(l => l.AutorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_LIVRO_AUTOR");
    }
}
