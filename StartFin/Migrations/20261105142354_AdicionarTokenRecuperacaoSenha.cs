using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartFin.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTokenRecuperacaoSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TokenRecuperacaoSenhaExpiraEm",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRecuperacaoSenhaHash",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenRecuperacaoSenhaExpiraEm",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TokenRecuperacaoSenhaHash",
                table: "AspNetUsers");
        }
    }
}
