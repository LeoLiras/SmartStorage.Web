using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class FixSaleAnnotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sale_Enters_enterId",
                table: "Sale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sale",
                table: "Sale");

            migrationBuilder.DropIndex(
                name: "IX_Sale_enterId",
                table: "Sale");

            migrationBuilder.DropColumn(
                name: "date_sale",
                table: "Sale");

            migrationBuilder.DropColumn(
                name: "enterId",
                table: "Sale");

            migrationBuilder.DropColumn(
                name: "qntd",
                table: "Sale");

            migrationBuilder.RenameTable(
                name: "Sale",
                newName: "Sales");

            migrationBuilder.RenameColumn(
                name: "id_enter",
                table: "Sales",
                newName: "SalQntd");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sales",
                newName: "SalId");

            migrationBuilder.AddColumn<DateTime>(
                name: "SalDateSale",
                table: "Sales",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SalEntId",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sales",
                table: "Sales",
                column: "SalId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_enterId",
                table: "Sales",
                column: "SalEntId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Enters_SalEntId",
                table: "Sales",
                column: "SalEntId",
                principalTable: "Enters",
                principalColumn: "EntId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Enters_SalEntId",
                table: "Sales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sales",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sale_enterId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "SalDateSale",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "SalEntId",
                table: "Sales");

            migrationBuilder.RenameTable(
                name: "Sales",
                newName: "Sale");

            migrationBuilder.RenameColumn(
                name: "SalQntd",
                table: "Sale",
                newName: "id_enter");

            migrationBuilder.RenameColumn(
                name: "SalId",
                table: "Sale",
                newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_sale",
                table: "Sale",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "'2024-07-08 00:00:00-03'");

            migrationBuilder.AddColumn<int>(
                name: "enterId",
                table: "Sale",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "qntd",
                table: "Sale",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sale",
                table: "Sale",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_enterId",
                table: "Sale",
                column: "enterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sale_Enters_enterId",
                table: "Sale",
                column: "enterId",
                principalTable: "Enters",
                principalColumn: "EntId");
        }
    }
}
