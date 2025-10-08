using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeeAnnotationsModelBuilder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "rg",
                table: "Employees",
                newName: "EmpRg");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Employees",
                newName: "EmpName");

            migrationBuilder.RenameColumn(
                name: "date_register",
                table: "Employees",
                newName: "EmpDateRegister");

            migrationBuilder.RenameColumn(
                name: "cpf",
                table: "Employees",
                newName: "EmpCpf");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmpRg",
                table: "Employees",
                newName: "rg");

            migrationBuilder.RenameColumn(
                name: "EmpName",
                table: "Employees",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "EmpDateRegister",
                table: "Employees",
                newName: "date_register");

            migrationBuilder.RenameColumn(
                name: "EmpCpf",
                table: "Employees",
                newName: "cpf");
        }
    }
}
