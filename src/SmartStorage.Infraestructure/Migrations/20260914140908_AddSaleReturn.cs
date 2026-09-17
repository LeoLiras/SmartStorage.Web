using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.AddColumn<int>(
                name: "SalReturnedQntd",
                schema: "dbo",
                table: "Sale",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement",
                sql: "[PsmType] IN (0, 1, 2, 3, 4, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.DropColumn(
                name: "SalReturnedQntd",
                schema: "dbo",
                table: "Sale");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement",
                sql: "[PsmType] IN (0, 1, 2, 3, 4)");
        }
    }
}
