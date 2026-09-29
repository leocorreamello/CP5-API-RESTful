# Biblioteca Comunitária API

Checkpoint 05 - C# Software Development — API RESTful com C# .NET 10, Entity Framework Core e Oracle.

## Integrantes

| Nome | RM |
|---|---|
| Leonardo Correa de Mello | RM 555573 |
| Felipe Soares Xavier | RM 556931 |
| Pedro Visconti Guidotte | RM 556630 |
| Herbert de Sousa Vilela | RM 555701 |
| Gabriel Figueira Flora | RM 556476 |

## Contexto do projeto

**O que é:** uma API RESTful para gerenciar o acervo e os empréstimos de uma **biblioteca comunitária** (de bairro, escola ou ONG), com três recursos: **autores**, **livros** e **empréstimos**.

**Problema que resolve:** bibliotecas pequenas costumam controlar acervo e empréstimos em cadernos ou planilhas. Isso gera perda de registros, o mesmo livro emprestado a duas pessoas e nenhuma visão de quais livros estão disponíveis ou com devolução pendente. A API centraliza esses dados e aplica as regras automaticamente:

- um livro só pode ser emprestado se estiver **disponível**;
- ao registrar o empréstimo o livro fica **indisponível**, e ao registrar a devolução volta a ficar **disponível**;
- o ISBN é **único** no acervo;
- um autor com livros, ou um livro com empréstimos, **não pode ser excluído** (preserva o histórico).

**Para quem é:** bibliotecários e voluntários que administram o acervo. A API foi pensada para ser consumida por um sistema web ou app de balcão/consulta.

## Tecnologias

- C# / .NET 10 (ASP.NET Core Web API com controllers)
- Entity Framework Core 10 + `Oracle.EntityFrameworkCore`
- `Asp.Versioning` (versionamento por URL) e Swashbuckle (Swagger / OpenAPI)
- xUnit + `WebApplicationFactory` (testes de integração)

## Banco de dados utilizado

**Oracle Database** da FIAP, acessado também pelo Oracle SQL Developer:

| Parâmetro | Valor |
|---|---|
| Hostname | `oracle.fiap.com.br` |
| Porta | `1521` |
| SID | `ORCL` |
| Usuário | *pessoal* |
| Senha | *pessoal* |

Tabelas criadas pela migration (prefixo `TB_` para não conflitar com outras tabelas do schema): `TB_AUTOR`, `TB_LIVRO` e `TB_EMPRESTIMO`, além da `__EFMigrationsHistory`.

```
TB_AUTOR 1 ──── N TB_LIVRO 1 ──── N TB_EMPRESTIMO
```

## Como rodar o projeto localmente

**Pré-requisitos:** [.NET SDK 10](https://dotnet.microsoft.com/download) e acesso de rede ao `oracle.fiap.com.br:1521` (rede/VPN da FIAP, se necessário).

```bash
# 1. Clonar e entrar na pasta
git clone <url-do-repositorio>
cd CP5-API-RESTful

# 2. Restaurar pacotes e ferramentas (inclui o dotnet-ef, versionado em dotnet-tools.json)
dotnet restore
dotnet tool restore

# 3. Informar a senha do Oracle SEM commitar (user-secrets)
dotnet user-secrets set "ConnectionStrings:OracleFiap" \
  "User Id=SEU_RM;Password=SUA_SENHA;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SID=ORCL)));" \
  --project src/BibliotecaApi

# 4. Criar as tabelas no Oracle aplicando a migration
dotnet tool run dotnet-ef database update --project src/BibliotecaApi

# 5. Executar a API
dotnet run --project src/BibliotecaApi --launch-profile http
```

Abra **http://localhost:5080** — o Swagger UI abre na raiz e tem um seletor para as versões **v1** e **v2**.

> Alternativa à senha via user-secrets: variável de ambiente `ConnectionStrings__OracleFiap` com a mesma connection string.
>
> Alternativa à etapa 4: executar o script [docs/migrations/MigracaoInicial.sql](docs/migrations/MigracaoInicial.sql) no SQL Developer, conectado ao mesmo usuário.

### Rodar os testes

Os testes de integração usam SQLite em memória (não dependem do Oracle nem de senha):

```bash
dotnet test
```

## Migration (EF Core)

A migration inicial [`MigracaoInicial`](src/BibliotecaApi/Migrations) cria as três tabelas, chaves primárias (identity), chaves estrangeiras e o índice único do ISBN.

```bash
# como foi criada
dotnet tool run dotnet-ef migrations add MigracaoInicial --project src/BibliotecaApi --output-dir Migrations

# aplicar no banco
dotnet tool run dotnet-ef database update --project src/BibliotecaApi

# gerar o SQL equivalente (já versionado em docs/migrations/MigracaoInicial.sql)
dotnet tool run dotnet-ef migrations script --project src/BibliotecaApi -o docs/migrations/MigracaoInicial.sql
```

Observação: o provider está configurado com `UseOracleSQLCompatibility(DatabaseVersion19)`, o que faz o `bool` virar `NUMBER(1)` (o tipo `BOOLEAN` só existe no Oracle 23ai).

## Versionamento

O versionamento é feito **pela URL** (`/api/v{versão}/recurso`) com o pacote `Asp.Versioning`:

- **v1** — CRUD completo de autores, livros e empréstimos.
- **v2** — evolução da listagem de livros: `GET /api/v2/livros` devolve um envelope **paginado** (`pagina`, `tamanhoPagina`, `totalItens`, `totalPaginas`, `itens`) e aceita busca por título. A v1 continua devolvendo a lista simples, sem quebrar clientes existentes.

## Endpoints disponíveis

### Autores — `/api/v1/autores`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/autores` | Lista todos os autores | 200 |
| GET | `/api/v1/autores/{id}` | Busca um autor pelo id | 200, 404 |
| POST | `/api/v1/autores` | Cadastra um autor | 201, 400 |
| PUT | `/api/v1/autores/{id}` | Atualiza um autor | 204, 400, 404 |
| DELETE | `/api/v1/autores/{id}` | Exclui um autor (sem livros) | 204, 404, 409 |

### Livros — `/api/v1/livros`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/livros?autorId=&disponivel=` | Lista livros, com filtros opcionais | 200 |
| GET | `/api/v1/livros/{id}` | Busca um livro pelo id | 200, 404 |
| POST | `/api/v1/livros` | Cadastra um livro (nasce disponível) | 201, 400, 409 (ISBN duplicado) |
| PUT | `/api/v1/livros/{id}` | Atualiza um livro | 204, 400, 404, 409 |
| DELETE | `/api/v1/livros/{id}` | Exclui um livro (sem empréstimos) | 204, 404, 409 |
| GET | `/api/v2/livros?pagina=&tamanhoPagina=&busca=&autorId=&disponivel=` | **v2:** lista paginada com busca por título | 200, 400 |
| GET | `/api/v2/livros/{id}` | **v2:** busca um livro pelo id | 200, 404 |

### Empréstimos — `/api/v1/emprestimos`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/emprestimos?devolvido=` | Lista empréstimos (filtro: devolvidos ou em andamento) | 200 |
| GET | `/api/v1/emprestimos/{id}` | Busca um empréstimo pelo id | 200, 404 |
| POST | `/api/v1/emprestimos` | Registra um empréstimo e torna o livro indisponível | 201, 400, 409 (livro já emprestado) |
| PUT | `/api/v1/emprestimos/{id}` | Atualiza leitor e prazo de um empréstimo em andamento | 204, 400, 404, 409 |
| POST | `/api/v1/emprestimos/{id}/devolucao` | Registra a devolução e libera o livro | 200, 404, 409 (já devolvido) |
| DELETE | `/api/v1/emprestimos/{id}` | Exclui um empréstimo (se ativo, libera o livro) | 204, 404 |

### Exemplos de corpo (JSON)

```jsonc
// POST /api/v1/autores
{ "nome": "Machado de Assis", "nacionalidade": "Brasileiro", "dataNascimento": "1839-06-21" }

// POST /api/v1/livros
{ "titulo": "Dom Casmurro", "isbn": "9788535914849", "anoPublicacao": 1899, "autorId": 1 }

// POST /api/v1/emprestimos
{ "livroId": 1, "nomeLeitor": "Ana Souza", "emailLeitor": "ana.souza@email.com", "dataPrevistaDevolucao": "2030-01-15T00:00:00" }
```

### Erros

Todos os erros seguem o padrão **ProblemDetails** (RFC 9457, `application/problem+json`):

| Status | Quando |
|---|---|
| 400 | Validação do corpo (campos obrigatórios, e-mail, ISBN, faixas), referência a autor/livro inexistente, prazo anterior à data do empréstimo |
| 404 | Recurso não encontrado |
| 409 | Conflito de regra: ISBN duplicado, livro já emprestado, exclusão bloqueada por dependências, empréstimo já devolvido |
| 500 | Erro inesperado (detalhes internos só aparecem no log, nunca na resposta) |

## Estrutura do projeto

```
src/BibliotecaApi
├── Controllers/V1, V2   # endpoints HTTP versionados (finos: só traduzem HTTP)
├── Services/            # regras de negócio (AutorService, LivroService, EmprestimoService)
├── Data/                # AppDbContext + configurações Fluent API das entidades
├── Models/              # entidades (Autor, Livro, Emprestimo)
├── Dtos/                # contratos de entrada/saída com validação (DataAnnotations)
├── Exceptions/          # NotFound / Conflict / BusinessRule
├── Middleware/          # GlobalExceptionHandler (exceção -> status code + ProblemDetails)
├── Configuration/       # configuração do Swagger por versão
└── Migrations/          # migrations do EF Core
tests/BibliotecaApi.Tests  # testes de integração (xUnit)
docs/migrations            # script SQL gerado pela migration
docs/evidencias            # prints do Swagger
```

## Evidências de teste

Prints do Swagger com cada endpoint funcionando: pasta [docs/evidencias](docs/evidencias).
</br>GET:
<img width="1582" height="981" alt="GetAutores" src="https://github.com/user-attachments/assets/c13c0062-d28b-4d50-bf2c-592b044f6599" />

</br>GET{id}:
<img width="1582" height="981" alt="GetIDAutores" src="https://github.com/user-attachments/assets/5fdef82c-2a5c-43ff-8e1c-4e292a21f5b1" />

</br>PUT:
<img width="1582" height="981" alt="PutAutores" src="https://github.com/user-attachments/assets/e21264b5-03e0-4418-bcf5-cdd80a27279d" />

</br>POST:
<img width="1582" height="981" alt="PostAutores" src="https://github.com/user-attachments/assets/bd5b20dd-a223-4c79-93a0-8d2a29031507" />

</br>DEL:
<img width="1582" height="981" alt="DeleteAutores" src="https://github.com/user-attachments/assets/d90ca595-3548-4aaa-972e-5599ae69a8d2" />



