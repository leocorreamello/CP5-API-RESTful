namespace BibliotecaApi.Models;

public class Livro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int AnoPublicacao { get; set; }

    /// <summary>Controlado pelo sistema: falso enquanto existir um empréstimo ativo.</summary>
    public bool Disponivel { get; set; } = true;

    public int AutorId { get; set; }
    public Autor? Autor { get; set; }

    public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
}
