using System.ComponentModel.DataAnnotations;
using BibliotecaApi.Models;

namespace BibliotecaApi.Dtos;

/// <summary>Dados para criar ou atualizar um autor.</summary>
public class AutorRequest
{
    /// <example>Machado de Assis</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <example>Brasileiro</example>
    [StringLength(60, ErrorMessage = "A nacionalidade deve ter no máximo 60 caracteres.")]
    public string? Nacionalidade { get; set; }

    /// <example>1839-06-21</example>
    public DateOnly? DataNascimento { get; set; }
}

public record AutorResponse(int Id, string Nome, string? Nacionalidade, DateOnly? DataNascimento)
{
    public static AutorResponse From(Autor autor) =>
        new(autor.Id, autor.Nome, autor.Nacionalidade,
            autor.DataNascimento.HasValue ? DateOnly.FromDateTime(autor.DataNascimento.Value) : null);
}
