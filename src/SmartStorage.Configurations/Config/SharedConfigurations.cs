using Microsoft.Extensions.Configuration;

namespace SmartStorage.Configurations.Config
{
    public static class SharedConfigurations
    {
        /// <summary>
        /// Carrega o appsettings.Shared.json, que concentra as chaves comuns aos servicos
        /// (TokenConfigurations) e e copiado para o output pelo csproj deste projeto.
        /// </summary>
        /// <remarks>
        /// As variaveis de ambiente sao readicionadas logo depois para voltarem ao topo da
        /// precedencia: sem isso o arquivo, por ser a fonte mais recente, sobrescreveria
        /// TokenConfigurations__Secret definido pelo compose.
        /// </remarks>
        public static IConfigurationBuilder AddSharedConfiguration(
            this IConfigurationBuilder configuration)
        {
            configuration.AddJsonFile(
                "appsettings.Shared.json",
                optional: false,
                reloadOnChange: true);

            configuration.AddEnvironmentVariables();

            return configuration;
        }
    }
}
