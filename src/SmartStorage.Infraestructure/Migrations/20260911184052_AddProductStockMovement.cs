using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductStockMovement",
                schema: "dbo",
                columns: table => new
                {
                    PsmId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PsmProId = table.Column<int>(type: "int", nullable: false),
                    PsmSheId = table.Column<int>(type: "int", nullable: true),
                    PsmType = table.Column<byte>(type: "tinyint", nullable: false),
                    PsmQntd = table.Column<int>(type: "int", nullable: false),
                    PsmDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PsmEmpId = table.Column<int>(type: "int", nullable: true),
                    PsmReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStockMovement", x => x.PsmId);
                    table.CheckConstraint("CK_ProductStockMovement_Tipo", "[PsmType] IN (0, 1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_ProductStockMovement_Employee_PsmEmpId",
                        column: x => x.PsmEmpId,
                        principalSchema: "dbo",
                        principalTable: "Employee",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductStockMovement_Product_PsmProId",
                        column: x => x.PsmProId,
                        principalSchema: "dbo",
                        principalTable: "Product",
                        principalColumn: "ProId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductStockMovement_Shelf_PsmSheId",
                        column: x => x.PsmSheId,
                        principalSchema: "dbo",
                        principalTable: "Shelf",
                        principalColumn: "SheId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovement_employeeId",
                schema: "dbo",
                table: "ProductStockMovement",
                column: "PsmEmpId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovement_productId",
                schema: "dbo",
                table: "ProductStockMovement",
                column: "PsmProId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovement_shelfId",
                schema: "dbo",
                table: "ProductStockMovement",
                column: "PsmSheId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductStockMovement",
                schema: "dbo");
        }
    }
}
