namespace BibliotecaApi.Dtos;

public record PagedResponse<T>(int Pagina, int TamanhoPagina, int TotalItens, int TotalPaginas, IReadOnlyList<T> Itens);
