using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaApi.Migrations
{
    /// <inheritdoc />
    public partial class KeysAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Libros",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdPersona = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Autores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPersona = table.Column<long>(type: "bigint", nullable: false),
                    LibroId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Autores_Libros_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libros",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibroId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Libros_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libros",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    Token_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Valido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutoresLibros",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutorId = table.Column<long>(type: "bigint", nullable: false),
                    LibroId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoresLibros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoresLibros_Autores_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Autores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutoresLibros_Libros_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibrosCategorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibroId = table.Column<long>(type: "bigint", nullable: false),
                    CategoriaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibrosCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibrosCategorias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Autores_LibroId",
                table: "Autores",
                column: "LibroId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoresLibros_AutorId",
                table: "AutoresLibros",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoresLibros_LibroId",
                table: "AutoresLibros",
                column: "LibroId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_LibroId",
                table: "Categorias",
                column: "LibroId");

            migrationBuilder.CreateIndex(
                name: "IX_LibrosCategorias_CategoriaId",
                table: "LibrosCategorias",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UsuarioId",
                table: "UserTokens",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoresLibros");

            migrationBuilder.DropTable(
                name: "LibrosCategorias");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Autores");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Libros");
        }
    }
}
