using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonaFK_PersonUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PersonaId",
                table: "PersonasUser",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PersonasUser_PersonaId",
                table: "PersonasUser",
                column: "PersonaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonasUser_Personas_PersonaId",
                table: "PersonasUser",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonasUser_Personas_PersonaId",
                table: "PersonasUser");

            migrationBuilder.DropIndex(
                name: "IX_PersonasUser_PersonaId",
                table: "PersonasUser");

            migrationBuilder.DropColumn(
                name: "PersonaId",
                table: "PersonasUser");
        }
    }
}
