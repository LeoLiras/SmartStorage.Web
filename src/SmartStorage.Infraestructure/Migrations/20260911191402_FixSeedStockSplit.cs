using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedStockSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 1,
                column: "EntQntd",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 3,
                column: "EntQntd",
                value: 4);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 5,
                column: "EntQntd",
                value: 20);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 7,
                column: "EntQntd",
                value: 25);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 8,
                column: "EntQntd",
                value: 100);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 10,
                column: "EntQntd",
                value: 120);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 1,
                column: "ProQntd",
                value: 14);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 2,
                column: "ProQntd",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 3,
                column: "ProQntd",
                value: 5);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 4,
                column: "ProQntd",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 5,
                column: "ProQntd",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 6,
                column: "ProQntd",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 7,
                column: "ProQntd",
                value: 35);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 8,
                column: "ProQntd",
                value: 50);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 9,
                column: "ProQntd",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 10,
                column: "ProQntd",
                value: 80);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 1,
                column: "EntQntd",
                value: 24);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 3,
                column: "EntQntd",
                value: 9);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 5,
                column: "EntQntd",
                value: 35);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 7,
                column: "EntQntd",
                value: 60);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 8,
                column: "EntQntd",
                value: 150);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Enter",
                keyColumn: "EntId",
                keyValue: 10,
                column: "EntQntd",
                value: 200);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 1,
                column: "ProQntd",
                value: 24);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 2,
                column: "ProQntd",
                value: 18);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 3,
                column: "ProQntd",
                value: 9);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 4,
                column: "ProQntd",
                value: 40);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 5,
                column: "ProQntd",
                value: 35);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 6,
                column: "ProQntd",
                value: 12);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 7,
                column: "ProQntd",
                value: 60);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 8,
                column: "ProQntd",
                value: 150);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 9,
                column: "ProQntd",
                value: 90);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 10,
                column: "ProQntd",
                value: 200);
        }
    }
}
