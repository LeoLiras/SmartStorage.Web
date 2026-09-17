using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMinimumStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProMinimumStock",
                schema: "dbo",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 1,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 2,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 3,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 4,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 5,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 6,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 7,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 8,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 9,
                column: "ProMinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 10,
                column: "ProMinimumStock",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProMinimumStock",
                schema: "dbo",
                table: "Product");
        }
    }
}
