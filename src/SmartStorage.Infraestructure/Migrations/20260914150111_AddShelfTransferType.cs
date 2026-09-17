using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfTransferType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement",
                sql: "[PsmType] IN (0, 1, 2, 3, 4, 5, 6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductStockMovement_Tipo",
                schema: "dbo",
                table: "ProductStockMovement",
                sql: "[PsmType] IN (0, 1, 2, 3, 4, 5)");
        }
    }
}
