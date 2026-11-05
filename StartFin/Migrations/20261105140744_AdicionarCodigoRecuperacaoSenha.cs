using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartFin.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCodigoRecuperacaoSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoRecuperacaoSenhaHash",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoRecuperacaoSenhaHash",
                table: "AspNetUsers");
        }
    }
}
