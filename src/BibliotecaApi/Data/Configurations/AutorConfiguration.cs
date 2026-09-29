using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotecaApi.Data.Configurations;

public class AutorConfiguration : IEntityTypeConfiguration<Autor>
{
    public void Configure(EntityTypeBuilder<Autor> builder)
    {
        builder.ToTable("TB_AUTOR");

        builder.HasKey(a => a.Id).HasName("PK_AUTOR");

        builder.Property(a => a.Nome).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Nacionalidade).HasMaxLength(60);
        builder.Property(a => a.DataNascimento).HasColumnType("DATE");
    }
}
