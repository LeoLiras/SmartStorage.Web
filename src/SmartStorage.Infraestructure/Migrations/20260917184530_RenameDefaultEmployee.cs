using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class RenameDefaultEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 6,
                column: "EmpName",
                value: "Admin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 6,
                column: "EmpName",
                value: "Colaborador Padrão");
        }
    }
}
