using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_register = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shelf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_register = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelf", x => x.Id);
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
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_register = table.Column<DateTime>(type: "datetime2", nullable: false),
                    qntd = table.Column<int>(type: "int", nullable: false),
                    employeeId = table.Column<int>(type: "int", nullable: true),
                    employee_id = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Product-PK", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Enter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_product = table.Column<int>(type: "int", nullable: false),
                    id_shelf = table.Column<int>(type: "int", nullable: false),
                    date_enter = table.Column<DateTime>(type: "datetime2", nullable: false),
                    productId = table.Column<int>(type: "int", nullable: true),
                    shelfId = table.Column<int>(type: "int", nullable: true),
                    qntd = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValueSql: "10.00")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enter_Product_productId",
                        column: x => x.productId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Enter_Shelf_shelfId",
                        column: x => x.shelfId,
                        principalTable: "Shelf",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_enter = table.Column<int>(type: "int", nullable: false),
                    enterId = table.Column<int>(type: "int", nullable: true),
                    qntd = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    date_sale = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "'2024-07-08 00:00:00-03'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sale_Enter_enterId",
                        column: x => x.enterId,
                        principalTable: "Enter",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enter_productId",
                table: "Enter",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_Enter_shelfId",
                table: "Enter",
                column: "shelfId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_employeeId",
                table: "Product",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_enterId",
                table: "Sale",
                column: "enterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Enter");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Shelf");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
