using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage_API.Migrations
{
    /// <inheritdoc />
    public partial class FixProductEmployeeNullable : Migration
    {
        // Migration sem operações de schema: serve apenas para realinhar o snapshot do modelo.
        //
        // Product.ProEmpId passou de int para int?, o que apenas remove o ALTER COLUMN NOT NULL
        // que o EF queria gerar — o banco já tem a coluna anulável, então não há DDL a aplicar.
        //
        // O EF também tentou renomear dbo.User.refresh_token_expiry_time para
        // UseRefreshTokenExpiryTime, mas a migration NewFieldUseType (20260127161047) já faz esse
        // rename no Up(); o que ficou desatualizado foi o snapshot dela, que continuou registrando
        // o nome antigo. Executar o rename de novo falharia, pois a coluna de origem não existe
        // mais. O SmartStorageContextModelSnapshot.cs gerado junto com esta migration já traz o
        // nome correto, então o Up() vazio corrige a divergência sem tocar no banco.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
