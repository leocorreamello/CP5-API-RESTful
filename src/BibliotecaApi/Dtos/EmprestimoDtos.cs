using System.ComponentModel.DataAnnotations;
using BibliotecaApi.Models;

namespace BibliotecaApi.Dtos;

/// <summary>Dados para registrar um novo empréstimo.</summary>
public class EmprestimoRequest
{
    /// <summary>Id de um livro cadastrado e disponível.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um LivroId válido.")]
    public int LivroId { get; set; }

    /// <example>Ana Souza</example>
    [Required(ErrorMessage = "O nome do leitor é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome do leitor deve ter entre 2 e 100 caracteres.")]
    public string NomeLeitor { get; set; } = string.Empty;

    /// <example>ana.souza@email.com</example>
    [Required(ErrorMessage = "O e-mail do leitor é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string EmailLeitor { get; set; } = string.Empty;

    /// <summary>Não pode ser anterior à data de hoje.</summary>
    /// <example>2030-01-15T00:00:00</example>
    [Required(ErrorMessage = "A data prevista de devolução é obrigatória.")]
    public DateTime? DataPrevistaDevolucao { get; set; }
}

/// <summary>Dados editáveis de um empréstimo ainda ativo (o livro não pode ser trocado).</summary>
public class EmprestimoUpdateRequest
{
    /// <example>Ana Souza</example>
    [Required(ErrorMessage = "O nome do leitor é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome do leitor deve ter entre 2 e 100 caracteres.")]
    public string NomeLeitor { get; set; } = string.Empty;

    /// <example>ana.souza@email.com</example>
    [Required(ErrorMessage = "O e-mail do leitor é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string EmailLeitor { get; set; } = string.Empty;

    /// <example>2030-02-01T00:00:00</example>
    [Required(ErrorMessage = "A data prevista de devolução é obrigatória.")]
    public DateTime? DataPrevistaDevolucao { get; set; }
}

public record EmprestimoResponse(
    int Id,
    int LivroId,
    string? LivroTitulo,
    string NomeLeitor,
    string EmailLeitor,
    DateTime DataEmprestimo,
    DateTime DataPrevistaDevolucao,
    DateTime? DataDevolucao,
    bool Devolvido)
{
    public static EmprestimoResponse From(Emprestimo e) =>
        new(e.Id, e.LivroId, e.Livro?.Titulo, e.NomeLeitor, e.EmailLeitor,
            e.DataEmprestimo, e.DataPrevistaDevolucao, e.DataDevolucao, e.Devolvido);
}
