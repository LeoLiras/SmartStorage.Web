using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class NewFieldUseType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                schema: "dbo",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "dbo",
                newName: "User",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "user_name",
                schema: "dbo",
                table: "User",
                newName: "UseUsername");

            migrationBuilder.RenameColumn(
                name: "refresh_token",
                schema: "dbo",
                table: "User",
                newName: "UseRefreshToken");

            migrationBuilder.RenameColumn(
                name: "refresh_token_expiry_time",
                schema: "dbo",
                table: "User",
                newName: "UseRefreshTokenExpiryTime");

            migrationBuilder.RenameColumn(
                name: "password",
                schema: "dbo",
                table: "User",
                newName: "UsePassword");

            migrationBuilder.RenameColumn(
                name: "full_name",
                schema: "dbo",
                table: "User",
                newName: "UseFullName");

            migrationBuilder.AddColumn<byte>(
                name: "UseType",
                schema: "dbo",
                table: "User",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                schema: "dbo",
                table: "User",
                column: "id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_Tipo",
                schema: "dbo",
                table: "User",
                sql: "[UseType] IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_Tipo",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UseType",
                schema: "dbo",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                schema: "dbo",
                newName: "users",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "UseUsername",
                schema: "dbo",
                table: "users",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "UseRefreshToken",
                schema: "dbo",
                table: "users",
                newName: "refresh_token");

            migrationBuilder.RenameColumn(
                name: "UsePassword",
                schema: "dbo",
                table: "users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "UseFullName",
                schema: "dbo",
                table: "users",
                newName: "full_name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                schema: "dbo",
                table: "users",
                column: "id");
        }
    }
}
