using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "User",
                columns: new[] { "id", "UseFullName", "UsePassword", "UseRefreshToken", "UseRefreshTokenExpiryTime", "UseType", "UseUsername" },
                values: new object[] { 2L, "Usuario de Teste", "dfa7a2273567dcd1efffb9a46308e91c20fa13c44c3441bc69cd6a7869b3f7fd", null, null, (byte)0, "usuario" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "User",
                keyColumn: "id",
                keyValue: 2L);
        }
    }
}
