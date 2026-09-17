using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SmartStorage_API.Model.Context;

namespace SmartStorage.Infraestructure.Context
{
    /// <summary>
    /// Cria o SmartStorageContext para as ferramentas do EF em tempo de design
    /// (migrations add, database update, migrations bundle).
    /// </summary>
    /// <remarks>
    /// Sem esta fabrica o EF precisa instanciar o host do startup project para obter o
    /// contexto, o que arrasta toda a configuracao da aplicacao - inclusive a de
    /// autenticacao, que exige TokenConfigurations:Secret e nao esta disponivel em tempo
    /// de design. A fabrica desacopla as migrations da subida da aplicacao.
    ///
    /// A connection string so precisa ser valida o suficiente para o provider montar o
    /// modelo: em tempo de design nenhuma conexao e aberta, e o bundle gerado recebe a
    /// connection string real por --connection na hora de aplicar.
    /// </remarks>
    public class SmartStorageContextFactory : IDesignTimeDbContextFactory<SmartStorageContext>
    {
        public SmartStorageContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__SqlServerConnection")
                ?? "Server=localhost;Database=SmartStorageWeb.db;Trusted_Connection=True;TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<SmartStorageContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new SmartStorageContext(options);
        }
    }
}
