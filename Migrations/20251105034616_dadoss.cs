using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoElvis2.Migrations
{
    /// <inheritdoc />
    public partial class dadoss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Observacoes",
                table: "Condominos",
                newName: "Imovel");

            migrationBuilder.AddColumn<int>(
                name: "QtdeImoveis",
                table: "Condominos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QtdeImoveis",
                table: "Condominos");

            migrationBuilder.RenameColumn(
                name: "Imovel",
                table: "Condominos",
                newName: "Observacoes");
        }
    }
}
