using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class FixShelfAnnotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Shelf_EntSheId",
                table: "Enters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shelf",
                table: "Shelf");

            migrationBuilder.DropColumn(
                name: "name",
                table: "Shelf");

            migrationBuilder.RenameTable(
                name: "Shelf",
                newName: "Shelves");

            migrationBuilder.RenameColumn(
                name: "data_register",
                table: "Shelves",
                newName: "SheDataRegister");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Shelves",
                newName: "SheId");

            migrationBuilder.AddColumn<string>(
                name: "SheName",
                table: "Shelves",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shelves",
                table: "Shelves",
                column: "SheId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Shelves_EntSheId",
                table: "Enters",
                column: "EntSheId",
                principalTable: "Shelves",
                principalColumn: "SheId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Shelves_EntSheId",
                table: "Enters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shelves",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "SheName",
                table: "Shelves");

            migrationBuilder.RenameTable(
                name: "Shelves",
                newName: "Shelf");

            migrationBuilder.RenameColumn(
                name: "SheDataRegister",
                table: "Shelf",
                newName: "data_register");

            migrationBuilder.RenameColumn(
                name: "SheId",
                table: "Shelf",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "Shelf",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shelf",
                table: "Shelf",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Shelf_EntSheId",
                table: "Enters",
                column: "EntSheId",
                principalTable: "Shelf",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
