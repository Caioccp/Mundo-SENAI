using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartFin.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposVerificacaoDesafios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Desafios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dificuldade",
                table: "Desafios",
                type: "text",
                nullable: false,
                defaultValue: "Fácil");

            migrationBuilder.AddColumn<decimal>(
                name: "MetaProgresso",
                table: "Desafios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProgressoAtual",
                table: "Desafios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Desafios");

            migrationBuilder.DropColumn(
                name: "Dificuldade",
                table: "Desafios");

            migrationBuilder.DropColumn(
                name: "MetaProgresso",
                table: "Desafios");

            migrationBuilder.DropColumn(
                name: "ProgressoAtual",
                table: "Desafios");
        }
    }
}
