using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartFin.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIdentificadorQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentificadorQuiz",
                table: "ResultadosQuiz",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentificadorQuiz",
                table: "ResultadosQuiz");
        }
    }
}
