using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaApi.Migrations
{
    /// <inheritdoc />
    public partial class PequenosUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibrosCategorias_Categorias_CategoriaId",
                table: "LibrosCategorias");

            migrationBuilder.AlterColumn<long>(
                name: "CategoriaId",
                table: "LibrosCategorias",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_LibrosCategorias_Categorias_CategoriaId",
                table: "LibrosCategorias",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibrosCategorias_Categorias_CategoriaId",
                table: "LibrosCategorias");

            migrationBuilder.AlterColumn<long>(
                name: "CategoriaId",
                table: "LibrosCategorias",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LibrosCategorias_Categorias_CategoriaId",
                table: "LibrosCategorias",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
