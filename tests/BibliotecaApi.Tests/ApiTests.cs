using System.Net;
using System.Net.Http.Json;
using BibliotecaApi.Dtos;

namespace BibliotecaApi.Tests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static int _isbnSeq = 1000000000;

    private static string NovoIsbn() => Interlocked.Increment(ref _isbnSeq).ToString();

    private async Task<AutorResponse> CriarAutor(string nome = "Machado de Assis")
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/autores", new { nome, nacionalidade = "Brasileiro", dataNascimento = "1839-06-21" });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        return (await resp.Content.ReadFromJsonAsync<AutorResponse>())!;
    }

    private async Task<LivroResponse> CriarLivro(int autorId, string titulo = "Dom Casmurro")
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/livros", new { titulo, isbn = NovoIsbn(), anoPublicacao = 1899, autorId });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        return (await resp.Content.ReadFromJsonAsync<LivroResponse>())!;
    }

    [Fact]
    public async Task Autor_CrudCompleto_RetornaStatusCodesCorretos()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/autores", new { nome = "Clarice Lispector", nacionalidade = "Brasileira" });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        Assert.Contains("/api/v1/autores/", resp.Headers.Location!.ToString());
        var autor = (await resp.Content.ReadFromJsonAsync<AutorResponse>())!;

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/v1/autores/{autor.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/v1/autores")).StatusCode);

        var put = await _client.PutAsJsonAsync($"/api/v1/autores/{autor.Id}", new { nome = "Clarice L.", nacionalidade = "Brasileira" });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        var atualizado = await _client.GetFromJsonAsync<AutorResponse>($"/api/v1/autores/{autor.Id}");
        Assert.Equal("Clarice L.", atualizado!.Nome);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/autores/{autor.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/v1/autores/{autor.Id}")).StatusCode);
    }

    [Fact]
    public async Task Autor_DadosInvalidos_Retorna400()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/autores", new { nome = "" });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Autor_Inexistente_Retorna404_ComProblemDetails()
    {
        var resp = await _client.GetAsync("/api/v1/autores/99999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType!.MediaType);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/v1/autores/99999", new { nome = "Fulano" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/v1/autores/99999")).StatusCode);
    }

    [Fact]
    public async Task Autor_ComLivros_NaoPodeSerExcluido()
    {
        var autor = await CriarAutor("Autor com livro");
        await CriarLivro(autor.Id);

        var resp = await _client.DeleteAsync($"/api/v1/autores/{autor.Id}");
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Livro_AutorInexistente_Retorna400()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/livros", new { titulo = "X", isbn = NovoIsbn(), anoPublicacao = 2000, autorId = 99999 });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Livro_IsbnInvalido_Retorna400_E_IsbnDuplicado_Retorna409()
    {
        var autor = await CriarAutor();
        var invalido = await _client.PostAsJsonAsync("/api/v1/livros", new { titulo = "X", isbn = "abc", anoPublicacao = 2000, autorId = autor.Id });
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);

        var livro = await CriarLivro(autor.Id);
        var duplicado = await _client.PostAsJsonAsync("/api/v1/livros", new { titulo = "Outro", isbn = livro.Isbn, anoPublicacao = 2001, autorId = autor.Id });
        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
    }

    [Fact]
    public async Task Livro_Atualizar_E_FiltrarPorAutor()
    {
        var autor = await CriarAutor("Autor filtro");
        var livro = await CriarLivro(autor.Id, "Titulo antigo");

        var put = await _client.PutAsJsonAsync($"/api/v1/livros/{livro.Id}",
            new { titulo = "Titulo novo", isbn = livro.Isbn, anoPublicacao = 1900, autorId = autor.Id });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var lista = await _client.GetFromJsonAsync<List<LivroResponse>>($"/api/v1/livros?autorId={autor.Id}");
        var unico = Assert.Single(lista!);
        Assert.Equal("Titulo novo", unico.Titulo);
        Assert.Equal("Autor filtro", unico.AutorNome);
    }

    [Fact]
    public async Task Emprestimo_FluxoCompleto_ControlaDisponibilidadeDoLivro()
    {
        var autor = await CriarAutor();
        var livro = await CriarLivro(autor.Id);
        var prazo = DateTime.UtcNow.AddDays(14);

        var post = await _client.PostAsJsonAsync("/api/v1/emprestimos",
            new { livroId = livro.Id, nomeLeitor = "Ana Souza", emailLeitor = "ana@email.com", dataPrevistaDevolucao = prazo });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var emprestimo = (await post.Content.ReadFromJsonAsync<EmprestimoResponse>())!;
        Assert.False(emprestimo.Devolvido);

        Assert.False((await _client.GetFromJsonAsync<LivroResponse>($"/api/v1/livros/{livro.Id}"))!.Disponivel);

        // mesmo livro emprestado de novo -> 409
        var segundo = await _client.PostAsJsonAsync("/api/v1/emprestimos",
            new { livroId = livro.Id, nomeLeitor = "Bruno", emailLeitor = "bruno@email.com", dataPrevistaDevolucao = prazo });
        Assert.Equal(HttpStatusCode.Conflict, segundo.StatusCode);

        // livro emprestado não pode ser excluído
        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync($"/api/v1/livros/{livro.Id}")).StatusCode);

        var put = await _client.PutAsJsonAsync($"/api/v1/emprestimos/{emprestimo.Id}",
            new { nomeLeitor = "Ana S.", emailLeitor = "ana@email.com", dataPrevistaDevolucao = prazo.AddDays(7) });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var devolucao = await _client.PostAsync($"/api/v1/emprestimos/{emprestimo.Id}/devolucao", null);
        Assert.Equal(HttpStatusCode.OK, devolucao.StatusCode);
        Assert.True((await devolucao.Content.ReadFromJsonAsync<EmprestimoResponse>())!.Devolvido);
        Assert.True((await _client.GetFromJsonAsync<LivroResponse>($"/api/v1/livros/{livro.Id}"))!.Disponivel);

        // devolver de novo ou editar após devolução -> 409
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsync($"/api/v1/emprestimos/{emprestimo.Id}/devolucao", null)).StatusCode);
        var putDevolvido = await _client.PutAsJsonAsync($"/api/v1/emprestimos/{emprestimo.Id}",
            new { nomeLeitor = "Ana", emailLeitor = "ana@email.com", dataPrevistaDevolucao = prazo });
        Assert.Equal(HttpStatusCode.Conflict, putDevolvido.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/emprestimos/{emprestimo.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/v1/emprestimos/{emprestimo.Id}")).StatusCode);
    }

    [Fact]
    public async Task Emprestimo_ExcluirAtivo_LiberaOLivro()
    {
        var autor = await CriarAutor();
        var livro = await CriarLivro(autor.Id);
        var post = await _client.PostAsJsonAsync("/api/v1/emprestimos",
            new { livroId = livro.Id, nomeLeitor = "Ana Souza", emailLeitor = "ana@email.com", dataPrevistaDevolucao = DateTime.UtcNow.AddDays(3) });
        var emprestimo = (await post.Content.ReadFromJsonAsync<EmprestimoResponse>())!;

        await _client.DeleteAsync($"/api/v1/emprestimos/{emprestimo.Id}");

        Assert.True((await _client.GetFromJsonAsync<LivroResponse>($"/api/v1/livros/{livro.Id}"))!.Disponivel);
    }

    [Fact]
    public async Task Emprestimo_DadosInvalidos_Retorna400()
    {
        var autor = await CriarAutor();
        var livro = await CriarLivro(autor.Id);

        var emailInvalido = await _client.PostAsJsonAsync("/api/v1/emprestimos",
            new { livroId = livro.Id, nomeLeitor = "Ana", emailLeitor = "nao-e-email", dataPrevistaDevolucao = DateTime.UtcNow.AddDays(3) });
        Assert.Equal(HttpStatusCode.BadRequest, emailInvalido.StatusCode);

        var prazoNoPassado = await _client.PostAsJsonAsync("/api/v1/emprestimos",
            new { livroId = livro.Id, nomeLeitor = "Ana", emailLeitor = "ana@email.com", dataPrevistaDevolucao = DateTime.UtcNow.AddDays(-2) });
        Assert.Equal(HttpStatusCode.BadRequest, prazoNoPassado.StatusCode);

        var livroInexistente = await _client.PostAsJsonAsync("/api/v1/emprestimos",
            new { livroId = 99999, nomeLeitor = "Ana", emailLeitor = "ana@email.com", dataPrevistaDevolucao = DateTime.UtcNow.AddDays(3) });
        Assert.Equal(HttpStatusCode.BadRequest, livroInexistente.StatusCode);
    }

    [Fact]
    public async Task V2_Livros_RetornaListagemPaginadaComBusca()
    {
        var autor = await CriarAutor("Autor paginacao");
        for (var i = 1; i <= 3; i++) await CriarLivro(autor.Id, $"Paginado {i}");

        var pagina = await _client.GetFromJsonAsync<PagedResponse<LivroResponse>>(
            $"/api/v2/livros?autorId={autor.Id}&pagina=1&tamanhoPagina=2&busca=paginado");

        Assert.Equal(3, pagina!.TotalItens);
        Assert.Equal(2, pagina.TotalPaginas);
        Assert.Equal(2, pagina.Itens.Count);

        Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync("/api/v2/livros?pagina=0")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/v2/livros/{pagina.Itens[0].Id}")).StatusCode);
    }

    [Fact]
    public async Task Swagger_ExpoeAsDuasVersoesDaApi()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/swagger/v1/swagger.json")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/swagger/v2/swagger.json")).StatusCode);
    }
}
