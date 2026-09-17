using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProCodigo",
                schema: "dbo",
                table: "Product",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProCusto",
                schema: "dbo",
                table: "Product",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProFatorConversao",
                schema: "dbo",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Invoice",
                schema: "dbo",
                columns: table => new
                {
                    InvId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvChave = table.Column<string>(type: "nvarchar(44)", maxLength: 44, nullable: false),
                    InvNumero = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    InvSerie = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    InvEmitenteCnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    InvEmitenteNome = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    InvDataEmissao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvDataImportacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvUseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.InvId);
                    table.ForeignKey(
                        name: "FK_Invoice_User_InvUseId",
                        column: x => x.InvUseId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItem",
                schema: "dbo",
                columns: table => new
                {
                    IniId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IniInvId = table.Column<int>(type: "int", nullable: false),
                    IniNumero = table.Column<int>(type: "int", nullable: false),
                    IniProId = table.Column<int>(type: "int", nullable: false),
                    IniCodigo = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    IniDescricao = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IniUnidade = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    IniQntdNota = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IniValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IniFatorConversao = table.Column<int>(type: "int", nullable: false),
                    IniQntdEstoque = table.Column<int>(type: "int", nullable: false),
                    IniCustoUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItem", x => x.IniId);
                    table.ForeignKey(
                        name: "FK_InvoiceItem_Invoice_IniInvId",
                        column: x => x.IniInvId,
                        principalSchema: "dbo",
                        principalTable: "Invoice",
                        principalColumn: "InvId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItem_Product_IniProId",
                        column: x => x.IniProId,
                        principalSchema: "dbo",
                        principalTable: "Product",
                        principalColumn: "ProId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Employee",
                columns: new[] { "EmpId", "EmpCpf", "EmpDateRegister", "EmpName", "EmpRg" },
                values: new object[] { 6, "12345678909", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Colaborador Padrão", "NA0000000" });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 1,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 2,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 3,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 4,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 5,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 6,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 7,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 8,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 9,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Product",
                keyColumn: "ProId",
                keyValue: 10,
                columns: new[] { "ProCodigo", "ProCusto", "ProFatorConversao" },
                values: new object[] { null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "UQ_Product_codigo",
                schema: "dbo",
                table: "Product",
                column: "ProCodigo",
                unique: true,
                filter: "[ProCodigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_userId",
                schema: "dbo",
                table: "Invoice",
                column: "InvUseId");

            migrationBuilder.CreateIndex(
                name: "UQ_Invoice_chave",
                schema: "dbo",
                table: "Invoice",
                column: "InvChave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItem_invoiceId",
                schema: "dbo",
                table: "InvoiceItem",
                column: "IniInvId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItem_productId",
                schema: "dbo",
                table: "InvoiceItem",
                column: "IniProId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceItem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Invoice",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "UQ_Product_codigo",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Employee",
                keyColumn: "EmpId",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "ProCodigo",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProCusto",
                schema: "dbo",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProFatorConversao",
                schema: "dbo",
                table: "Product");
        }
    }
}
