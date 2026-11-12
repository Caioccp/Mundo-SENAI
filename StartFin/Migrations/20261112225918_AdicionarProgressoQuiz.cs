using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartFin.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarProgressoQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DataConclusao",
                table: "ResultadosQuiz",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "ResultadosQuiz",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "IndicePerguntaAtual",
                table: "ResultadosQuiz",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RespostasJson",
                table: "ResultadosQuiz",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ResultadosQuiz",
                type: "text",
                nullable: false,
                defaultValue: "Concluída");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "ResultadosQuiz");

            migrationBuilder.DropColumn(
                name: "IndicePerguntaAtual",
                table: "ResultadosQuiz");

            migrationBuilder.DropColumn(
                name: "RespostasJson",
                table: "ResultadosQuiz");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ResultadosQuiz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataConclusao",
                table: "ResultadosQuiz",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
