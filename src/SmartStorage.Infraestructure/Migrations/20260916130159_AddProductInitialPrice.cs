using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInitialPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProPrecoInicial",
                schema: "dbo",
                table: "Product",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 1,
                column: "ProPrecoInicial",
                value: 289.90m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 2,
                column: "ProPrecoInicial",
                value: 349.00m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 3,
                column: "ProPrecoInicial",
                value: 529.90m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 4,
                column: "ProPrecoInicial",
                value: 45.50m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 5,
                column: "ProPrecoInicial",
                value: 79.90m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 6,
                column: "ProPrecoInicial",
                value: 219.00m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 7,
                column: "ProPrecoInicial",
                value: 32.90m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 8,
                column: "ProPrecoInicial",
                value: 12.40m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 9,
                column: "ProPrecoInicial",
                value: 18.75m);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 10,
                column: "ProPrecoInicial",
                value: 8.90m);

            migrationBuilder.Sql(@"
                UPDATE p
                SET p.ProPrecoInicial = e.EntPrice
                FROM dbo.Product p
                CROSS APPLY (
                    SELECT TOP 1 EntPrice
                    FROM dbo.Enter
                    WHERE EntProId = p.ProId
                    ORDER BY CASE WHEN EntQntd > 0 THEN 0 ELSE 1 END, EntDateEnter DESC, EntId DESC
                ) e;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProPrecoInicial",
                schema: "dbo",
                table: "Product");
        }
    }
}
