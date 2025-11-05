using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoElvis2.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarMigr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Condominos_CondominoId",
                table: "Imoveis");

            migrationBuilder.AlterColumn<int>(
                name: "CondominoId",
                table: "Imoveis",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Condominos_CondominoId",
                table: "Imoveis",
                column: "CondominoId",
                principalTable: "Condominos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Condominos_CondominoId",
                table: "Imoveis");

            migrationBuilder.AlterColumn<int>(
                name: "CondominoId",
                table: "Imoveis",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Condominos_CondominoId",
                table: "Imoveis",
                column: "CondominoId",
                principalTable: "Condominos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
