using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoElvis2.Migrations
{
    /// <inheritdoc />
    public partial class dado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imovel",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "Locatario",
                table: "Contratos");

            migrationBuilder.AddColumn<int>(
                name: "CondominoId",
                table: "Contratos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImovelId",
                table: "Contratos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_CondominoId",
                table: "Contratos",
                column: "CondominoId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_ImovelId",
                table: "Contratos",
                column: "ImovelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Condominos_CondominoId",
                table: "Contratos",
                column: "CondominoId",
                principalTable: "Condominos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Imoveis_ImovelId",
                table: "Contratos",
                column: "ImovelId",
                principalTable: "Imoveis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Condominos_CondominoId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Imoveis_ImovelId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_CondominoId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_ImovelId",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "CondominoId",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "ImovelId",
                table: "Contratos");

            migrationBuilder.AddColumn<string>(
                name: "Imovel",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Locatario",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
