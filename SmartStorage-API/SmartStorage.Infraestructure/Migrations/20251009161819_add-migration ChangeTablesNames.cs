using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class addmigrationChangeTablesNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Products_EntProId",
                table: "Enters");

            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Shelves_EntSheId",
                table: "Enters");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Employees_ProEmpId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Enters_SalEntId",
                table: "Sales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shelves",
                table: "Shelves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sales",
                table: "Sales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enters",
                table: "Enters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "Shelves",
                newName: "Shelf",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Sales",
                newName: "Sale",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Enters",
                newName: "Enter",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "Employee",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shelf",
                schema: "dbo",
                table: "Shelf",
                column: "SheId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sale",
                schema: "dbo",
                table: "Sale",
                column: "SalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                schema: "dbo",
                table: "Product",
                column: "ProId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enter",
                schema: "dbo",
                table: "Enter",
                column: "EntId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                schema: "dbo",
                table: "Employee",
                column: "EmpId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enter_Product_EntProId",
                schema: "dbo",
                table: "Enter",
                column: "EntProId",
                principalSchema: "dbo",
                principalTable: "Product",
                principalColumn: "ProId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enter_Shelf_EntSheId",
                schema: "dbo",
                table: "Enter",
                column: "EntSheId",
                principalSchema: "dbo",
                principalTable: "Shelf",
                principalColumn: "SheId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Employee_ProEmpId",
                schema: "dbo",
                table: "Product",
                column: "ProEmpId",
                principalSchema: "dbo",
                principalTable: "Employee",
                principalColumn: "EmpId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sale_Enter_SalEntId",
                schema: "dbo",
                table: "Sale",
                column: "SalEntId",
                principalSchema: "dbo",
                principalTable: "Enter",
                principalColumn: "EntId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enter_Product_EntProId",
                schema: "dbo",
                table: "Enter");

            migrationBuilder.DropForeignKey(
                name: "FK_Enter_Shelf_EntSheId",
                schema: "dbo",
                table: "Enter");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Employee_ProEmpId",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Sale_Enter_SalEntId",
                schema: "dbo",
                table: "Sale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shelf",
                schema: "dbo",
                table: "Shelf");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sale",
                schema: "dbo",
                table: "Sale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enter",
                schema: "dbo",
                table: "Enter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                schema: "dbo",
                table: "Employee");

            migrationBuilder.RenameTable(
                name: "Shelf",
                schema: "dbo",
                newName: "Shelves");

            migrationBuilder.RenameTable(
                name: "Sale",
                schema: "dbo",
                newName: "Sales");

            migrationBuilder.RenameTable(
                name: "Product",
                schema: "dbo",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Enter",
                schema: "dbo",
                newName: "Enters");

            migrationBuilder.RenameTable(
                name: "Employee",
                schema: "dbo",
                newName: "Employees");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shelves",
                table: "Shelves",
                column: "SheId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sales",
                table: "Sales",
                column: "SalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "ProId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enters",
                table: "Enters",
                column: "EntId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "EmpId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Products_EntProId",
                table: "Enters",
                column: "EntProId",
                principalTable: "Products",
                principalColumn: "ProId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Shelves_EntSheId",
                table: "Enters",
                column: "EntSheId",
                principalTable: "Shelves",
                principalColumn: "SheId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Employees_ProEmpId",
                table: "Products",
                column: "ProEmpId",
                principalTable: "Employees",
                principalColumn: "EmpId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Enters_SalEntId",
                table: "Sales",
                column: "SalEntId",
                principalTable: "Enters",
                principalColumn: "EntId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
