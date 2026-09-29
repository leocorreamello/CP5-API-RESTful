using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaApi.Migrations
{
    /// <inheritdoc />
    public partial class MigracaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_AUTOR",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    NACIONALIDADE = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: true),
                    DATA_NASCIMENTO = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUTOR", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_LIVRO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TITULO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    ISBN = table.Column<string>(type: "NVARCHAR2(13)", maxLength: 13, nullable: false),
                    ANO_PUBLICACAO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DISPONIVEL = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    AUTOR_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LIVRO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LIVRO_AUTOR",
                        column: x => x.AUTOR_ID,
                        principalTable: "TB_AUTOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_EMPRESTIMO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME_LEITOR = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL_LEITOR = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DATA_EMPRESTIMO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DATA_PREVISTA_DEVOLUCAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DATA_DEVOLUCAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    LIVRO_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPRESTIMO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMPRESTIMO_LIVRO",
                        column: x => x.LIVRO_ID,
                        principalTable: "TB_LIVRO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EMPRESTIMO_LIVRO",
                table: "TB_EMPRESTIMO",
                column: "LIVRO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LIVRO_AUTOR",
                table: "TB_LIVRO",
                column: "AUTOR_ID");

            migrationBuilder.CreateIndex(
                name: "UX_LIVRO_ISBN",
                table: "TB_LIVRO",
                column: "ISBN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_EMPRESTIMO");

            migrationBuilder.DropTable(
                name: "TB_LIVRO");

            migrationBuilder.DropTable(
                name: "TB_AUTOR");
        }
    }
}
