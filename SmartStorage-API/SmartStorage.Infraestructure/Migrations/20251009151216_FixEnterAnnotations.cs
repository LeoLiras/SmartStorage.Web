using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class FixEnterAnnotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enter_Product_productId",
                table: "Enter");

            migrationBuilder.DropForeignKey(
                name: "FK_Enter_Shelf_shelfId",
                table: "Enter");

            migrationBuilder.DropForeignKey(
                name: "FK_Sale_Enter_enterId",
                table: "Sale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enter",
                table: "Enter");

            migrationBuilder.DropIndex(
                name: "IX_Enter_productId",
                table: "Enter");

            migrationBuilder.DropIndex(
                name: "IX_Enter_shelfId",
                table: "Enter");

            migrationBuilder.DropColumn(
                name: "price",
                table: "Enter");

            migrationBuilder.DropColumn(
                name: "productId",
                table: "Enter");

            migrationBuilder.DropColumn(
                name: "qntd",
                table: "Enter");

            migrationBuilder.DropColumn(
                name: "shelfId",
                table: "Enter");

            migrationBuilder.RenameTable(
                name: "Enter",
                newName: "Enters");

            migrationBuilder.RenameColumn(
                name: "id_shelf",
                table: "Enters",
                newName: "EntSheId");

            migrationBuilder.RenameColumn(
                name: "id_product",
                table: "Enters",
                newName: "EntQntd");

            migrationBuilder.RenameColumn(
                name: "date_enter",
                table: "Enters",
                newName: "EntDateEnter");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Enters",
                newName: "EntId");

            migrationBuilder.AddColumn<decimal>(
                name: "EntPrice",
                table: "Enters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "EntProId",
                table: "Enters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enters",
                table: "Enters",
                column: "EntId");

            migrationBuilder.CreateIndex(
                name: "IX_Enter_productId",
                table: "Enters",
                column: "EntProId");

            migrationBuilder.CreateIndex(
                name: "IX_Enter_shelfId",
                table: "Enters",
                column: "EntSheId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Product_EntProId",
                table: "Enters",
                column: "EntProId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Shelf_EntSheId",
                table: "Enters",
                column: "EntSheId",
                principalTable: "Shelf",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sale_Enters_enterId",
                table: "Sale",
                column: "enterId",
                principalTable: "Enters",
                principalColumn: "EntId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Product_EntProId",
                table: "Enters");

            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Shelf_EntSheId",
                table: "Enters");

            migrationBuilder.DropForeignKey(
                name: "FK_Sale_Enters_enterId",
                table: "Sale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enters",
                table: "Enters");

            migrationBuilder.DropIndex(
                name: "IX_Enter_productId",
                table: "Enters");

            migrationBuilder.DropIndex(
                name: "IX_Enter_shelfId",
                table: "Enters");

            migrationBuilder.DropColumn(
                name: "EntPrice",
                table: "Enters");

            migrationBuilder.DropColumn(
                name: "EntProId",
                table: "Enters");

            migrationBuilder.RenameTable(
                name: "Enters",
                newName: "Enter");

            migrationBuilder.RenameColumn(
                name: "EntSheId",
                table: "Enter",
                newName: "id_shelf");

            migrationBuilder.RenameColumn(
                name: "EntQntd",
                table: "Enter",
                newName: "id_product");

            migrationBuilder.RenameColumn(
                name: "EntDateEnter",
                table: "Enter",
                newName: "date_enter");

            migrationBuilder.RenameColumn(
                name: "EntId",
                table: "Enter",
                newName: "Id");

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "Enter",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValueSql: "10.00");

            migrationBuilder.AddColumn<int>(
                name: "productId",
                table: "Enter",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "qntd",
                table: "Enter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "shelfId",
                table: "Enter",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enter",
                table: "Enter",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Enter_productId",
                table: "Enter",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_Enter_shelfId",
                table: "Enter",
                column: "shelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enter_Product_productId",
                table: "Enter",
                column: "productId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enter_Shelf_shelfId",
                table: "Enter",
                column: "shelfId",
                principalTable: "Shelf",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sale_Enter_enterId",
                table: "Sale",
                column: "enterId",
                principalTable: "Enter",
                principalColumn: "Id");
        }
    }
}
