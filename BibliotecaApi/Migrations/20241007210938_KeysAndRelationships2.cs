using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaApi.Migrations
{
    /// <inheritdoc />
    public partial class KeysAndRelationships2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdPersona",
                table: "Usuarios",
                newName: "PersonaId");

            migrationBuilder.RenameColumn(
                name: "IdPersona",
                table: "Autores",
                newName: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PersonaId",
                table: "Usuarios",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Autores_PersonaId",
                table: "Autores",
                column: "PersonaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Autores_Personas_PersonaId",
                table: "Autores",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Personas_PersonaId",
                table: "Usuarios",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Autores_Personas_PersonaId",
                table: "Autores");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Personas_PersonaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_PersonaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Autores_PersonaId",
                table: "Autores");

            migrationBuilder.RenameColumn(
                name: "PersonaId",
                table: "Usuarios",
                newName: "IdPersona");

            migrationBuilder.RenameColumn(
                name: "PersonaId",
                table: "Autores",
                newName: "IdPersona");
        }
    }
}
