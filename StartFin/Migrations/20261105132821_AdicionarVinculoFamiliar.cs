using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartFin.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarVinculoFamiliar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoVinculoFamiliar",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CodigoVinculoFamiliar",
                table: "AspNetUsers",
                column: "CodigoVinculoFamiliar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ResponsavelId",
                table: "AspNetUsers",
                column: "ResponsavelId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ResponsavelId",
                table: "AspNetUsers",
                column: "ResponsavelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ResponsavelId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CodigoVinculoFamiliar",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ResponsavelId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CodigoVinculoFamiliar",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ResponsavelId",
                table: "AspNetUsers");
        }
    }
}
