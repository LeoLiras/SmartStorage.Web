using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AuthorStockMovementByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStockMovement_Employee_PsmEmpId",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.DropIndex(
                name: "IX_ProductStockMovement_employeeId",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.DropColumn(
                name: "PsmEmpId",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.AddColumn<long>(
                name: "PsmUseId",
                schema: "dbo",
                table: "ProductStockMovement",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovement_userId",
                schema: "dbo",
                table: "ProductStockMovement",
                column: "PsmUseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStockMovement_User_PsmUseId",
                schema: "dbo",
                table: "ProductStockMovement",
                column: "PsmUseId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStockMovement_User_PsmUseId",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.DropIndex(
                name: "IX_ProductStockMovement_userId",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.DropColumn(
                name: "PsmUseId",
                schema: "dbo",
                table: "ProductStockMovement");

            migrationBuilder.AddColumn<int>(
                name: "PsmEmpId",
                schema: "dbo",
                table: "ProductStockMovement",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovement_employeeId",
                schema: "dbo",
                table: "ProductStockMovement",
                column: "PsmEmpId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStockMovement_Employee_PsmEmpId",
                schema: "dbo",
                table: "ProductStockMovement",
                column: "PsmEmpId",
                principalSchema: "dbo",
                principalTable: "Employee",
                principalColumn: "EmpId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
