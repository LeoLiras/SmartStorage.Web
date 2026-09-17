using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class FixProductAnnotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Product_EntProId",
                table: "Enters");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Employees_employeeId",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "Product-PK",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "employee_id",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "name",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameColumn(
                name: "qntd",
                table: "Products",
                newName: "ProQntd");

            migrationBuilder.RenameColumn(
                name: "employeeId",
                table: "Products",
                newName: "ProEmpId");

            migrationBuilder.RenameColumn(
                name: "date_register",
                table: "Products",
                newName: "ProDateRegister");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProId");

            migrationBuilder.AddColumn<string>(
                name: "ProDescription",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProImage",
                table: "Products",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProName",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "ProId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Products_EntProId",
                table: "Enters",
                column: "EntProId",
                principalTable: "Products",
                principalColumn: "ProId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Employees_ProEmpId",
                table: "Products",
                column: "ProEmpId",
                principalTable: "Employees",
                principalColumn: "EmpId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enters_Products_EntProId",
                table: "Enters");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Employees_ProEmpId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProDescription",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProImage",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProName",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameColumn(
                name: "ProQntd",
                table: "Product",
                newName: "qntd");

            migrationBuilder.RenameColumn(
                name: "ProEmpId",
                table: "Product",
                newName: "employeeId");

            migrationBuilder.RenameColumn(
                name: "ProDateRegister",
                table: "Product",
                newName: "date_register");

            migrationBuilder.RenameColumn(
                name: "ProId",
                table: "Product",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "employee_id",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "Product-PK",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enters_Product_EntProId",
                table: "Enters",
                column: "EntProId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Employees_employeeId",
                table: "Product",
                column: "employeeId",
                principalTable: "Employees",
                principalColumn: "EmpId");
        }
    }
}
