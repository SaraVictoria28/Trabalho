using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoElvis2.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imovel",
                table: "Condominos");

            migrationBuilder.DropColumn(
                name: "QtdeImoveis",
                table: "Condominos");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorAluguel",
                table: "Condominos",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ValorAluguel",
                table: "Condominos",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imovel",
                table: "Condominos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QtdeImoveis",
                table: "Condominos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
