using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAndShelfVolume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SheVolume",
                schema: "dbo",
                table: "Shelf",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProVolume",
                schema: "dbo",
                table: "Product",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 1,
                column: "ProVolume",
                value: 6.0m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 2,
                column: "ProVolume",
                value: 4.5m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 3,
                column: "ProVolume",
                value: 12.0m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 4,
                column: "ProVolume",
                value: 1.5m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 5,
                column: "ProVolume",
                value: 1.2m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 6,
                column: "ProVolume",
                value: 0.8m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 7,
                column: "ProVolume",
                value: 6.0m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 8,
                column: "ProVolume",
                value: 0.3m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 9,
                column: "ProVolume",
                value: 0.4m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 10,
                column: "ProVolume",
                value: 0.1m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 1,
                column: "SheVolume",
                value: 400m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 2,
                column: "SheVolume",
                value: 400m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 3,
                column: "SheVolume",
                value: 300m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 4,
                column: "SheVolume",
                value: 300m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 5,
                column: "SheVolume",
                value: 300m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 6,
                column: "SheVolume",
                value: 300m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 7,
                column: "SheVolume",
                value: 300m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 8,
                column: "SheVolume",
                value: 300m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 9,
                column: "SheVolume",
                value: 300m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Shelf",
                keyColumn: "SheId",
                keyValue: 10,
                column: "SheVolume",
                value: 800m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SheVolume",
                schema: "dbo",
                table: "Shelf");

            migrationBuilder.DropColumn(
                name: "ProVolume",
                schema: "dbo",
                table: "Product");
        }
    }
}
