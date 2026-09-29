using System.ComponentModel.DataAnnotations;
using BibliotecaApi.Models;

namespace BibliotecaApi.Dtos;

/// <summary>Dados para criar ou atualizar um livro.</summary>
public class LivroRequest
{
    /// <example>Dom Casmurro</example>
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>ISBN-10 ou ISBN-13, somente dígitos.</summary>
    /// <example>9788535914849</example>
    [Required(ErrorMessage = "O ISBN é obrigatório.")]
    [RegularExpression(@"^(\d{10}|\d{13})$", ErrorMessage = "O ISBN deve conter 10 ou 13 dígitos, sem hífens.")]
    public string Isbn { get; set; } = string.Empty;

    /// <example>1899</example>
    [Range(1000, 2100, ErrorMessage = "O ano de publicação deve estar entre 1000 e 2100.")]
    public int AnoPublicacao { get; set; }

    /// <summary>Id de um autor já cadastrado.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um AutorId válido.")]
    public int AutorId { get; set; }
}

public record LivroResponse(
    int Id,
    string Titulo,
    string Isbn,
    int AnoPublicacao,
    bool Disponivel,
    int AutorId,
    string? AutorNome)
{
    public static LivroResponse From(Livro livro) =>
        new(livro.Id, livro.Titulo, livro.Isbn, livro.AnoPublicacao, livro.Disponivel, livro.AutorId, livro.Autor?.Nome);
}

/// <summary>Filtros e paginação da listagem de livros (v2).</summary>
public class LivroConsultaV2
{
    /// <summary>Número da página, começando em 1.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a 1.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de itens por página (máximo 100).</summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;

    /// <summary>Trecho do título (sem diferenciar maiúsculas/minúsculas).</summary>
    public string? Busca { get; set; }

    public int? AutorId { get; set; }
    public bool? Disponivel { get; set; }
}
