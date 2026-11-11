using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartStorage.AuthenticationAPI.Repositories;

namespace SmartStorage.AuthenticationAPI.Tests;

/// <summary>
/// Sobe a AuthenticationAPI inteira em memória, com o repositório de usuários trocado por um falso.
/// </summary>
public class ApiEmMemoria : WebApplicationFactory<Program>
{
    public IUserRepository Usuarios { get; } = Substitute.For<IUserRepository>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // O appsettings.Shared.json só existe na pasta de saída, copiado pelo SmartStorage.Configurations;
        // na raiz padrão da factory, a pasta do código da API, o AddSharedConfiguration não o acha.
        builder.UseContentRoot(AppContext.BaseDirectory);

        // UseSetting, e não ConfigureAppConfiguration: o Program lê o segredo ainda no builder,
        // antes do Build, e o AddAuthConfiguration lança se ele estiver vazio.
        builder.UseSetting("TokenConfigurations:Secret", "segredo-dos-testes-com-pelo-menos-32-bytes");
        builder.UseSetting("ConnectionStrings:SqlServerConnection", "Server=nenhum;Database=nenhum");

        builder.ConfigureTestServices(services => services.AddScoped(_ => Usuarios));
    }
}
