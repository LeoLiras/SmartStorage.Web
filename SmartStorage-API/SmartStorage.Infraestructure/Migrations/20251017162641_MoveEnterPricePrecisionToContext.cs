using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class MoveEnterPricePrecisionToContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Employee",
                schema: "dbo",
                columns: table => new
                {
                    EmpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmpCpf = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    EmpRg = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    EmpDateRegister = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.EmpId);
                });

            migrationBuilder.CreateTable(
                name: "Shelf",
                schema: "dbo",
                columns: table => new
                {
                    SheId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SheName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SheDataRegister = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelf", x => x.SheId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "dbo",
                columns: table => new
                {
                    ProId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ProDateRegister = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProQntd = table.Column<int>(type: "int", nullable: false),
                    ProEmpId = table.Column<int>(type: "int", nullable: true),
                    ProImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ProId);
                    table.ForeignKey(
                        name: "FK_Product_Employee_ProEmpId",
                        column: x => x.ProEmpId,
                        principalSchema: "dbo",
                        principalTable: "Employee",
                        principalColumn: "EmpId");
                });

            migrationBuilder.CreateTable(
                name: "Enter",
                schema: "dbo",
                columns: table => new
                {
                    EntId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntProId = table.Column<int>(type: "int", nullable: false),
                    EntSheId = table.Column<int>(type: "int", nullable: false),
                    EntDateEnter = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntQntd = table.Column<int>(type: "int", nullable: false),
                    EntPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enter", x => x.EntId);
                    table.ForeignKey(
                        name: "FK_Enter_Product_EntProId",
                        column: x => x.EntProId,
                        principalSchema: "dbo",
                        principalTable: "Product",
                        principalColumn: "ProId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enter_Shelf_EntSheId",
                        column: x => x.EntSheId,
                        principalSchema: "dbo",
                        principalTable: "Shelf",
                        principalColumn: "SheId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                schema: "dbo",
                columns: table => new
                {
                    SalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalEntId = table.Column<int>(type: "int", nullable: false),
                    SalQntd = table.Column<int>(type: "int", nullable: false),
                    SalDateSale = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sale", x => x.SalId);
                    table.ForeignKey(
                        name: "FK_Sale_Enter_SalEntId",
                        column: x => x.SalEntId,
                        principalSchema: "dbo",
                        principalTable: "Enter",
                        principalColumn: "EntId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enter_productId",
                schema: "dbo",
                table: "Enter",
                column: "EntProId");

            migrationBuilder.CreateIndex(
                name: "IX_Enter_shelfId",
                schema: "dbo",
                table: "Enter",
                column: "EntSheId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_employeeId",
                schema: "dbo",
                table: "Product",
                column: "ProEmpId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_enterId",
                schema: "dbo",
                table: "Sale",
                column: "SalEntId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sale",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Enter",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Shelf",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Employee",
                schema: "dbo");
        }
    }
}
