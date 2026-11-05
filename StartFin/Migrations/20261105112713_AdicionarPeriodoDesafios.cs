using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartFin.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPeriodoDesafios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql("""
        ALTER TABLE "Desafios"
        ADD COLUMN IF NOT EXISTS "DataFim"
        timestamp with time zone NOT NULL
        DEFAULT '-infinity'::timestamp with time zone;

        ALTER TABLE "Desafios"
        ADD COLUMN IF NOT EXISTS "DataInicio"
        timestamp with time zone NOT NULL
        DEFAULT '-infinity'::timestamp with time zone;
        """);
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFim",
                table: "Desafios");

            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "Desafios");
        }
    }
}
