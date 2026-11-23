using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.Shared.Enum;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

/// <summary>
/// A sessão dura até 9 horas desde o login. O prazo do refresh token era de 7 dias
/// (DaysToExpiry) e passa a ser o fim da sessão, configurado em SessionHours.
/// </summary>
public class PrazoDaSessaoTests : IClassFixture<ApiEmMemoria>
{
    private readonly ApiEmMemoria _api;

    private readonly User _usuario;

    public PrazoDaSessaoTests(ApiEmMemoria api)
    {
        _api = api;

        IPasswordHasher hasher;
        using (var escopo = _api.Services.CreateScope())
            hasher = escopo.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _usuario = new User
        {
            Id = 4,
            Username = "sessao",
            FullName = "Usuário da Sessão",
            Password = hasher.Hash("sessao@123"),
            UseType = TipoUsuario.Usuario
        };

        _api.Usuarios.FindByUsername("sessao").Returns(_usuario);
        _api.Usuarios.Update(Arg.Any<User>()).Returns(c => c.Arg<User>());
    }

    [Fact]
    public async Task Login_grava_o_fim_da_sessao_9_horas_depois()
    {
        using var cliente = _api.CreateClient();

        var antes = DateTime.Now;
        var resposta = await cliente.PostAsJsonAsync("/api/auth/v1/signin", new { userName = "sessao", password = "sessao@123" });
        var depois = DateTime.Now;

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(_usuario.RefreshTokenExpiryTime);
        Assert.InRange(_usuario.RefreshTokenExpiryTime!.Value, antes.AddHours(9), depois.AddHours(9));
    }

    [Fact]
    public void Configuracao_compartilhada_tem_SessionHours_e_nao_DaysToExpiry()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);
        while (pasta is not null && !File.Exists(Path.Combine(pasta.FullName, "SmartStorageWeb.sln")))
            pasta = pasta.Parent;

        Assert.NotNull(pasta);

        var arquivo = Path.Combine(pasta!.FullName, "SmartStorage.Configurations", "Json", "appsettings.Shared.json");
        using var json = JsonDocument.Parse(File.ReadAllText(arquivo));
        var token = json.RootElement.GetProperty("TokenConfigurations");

        Assert.Equal(9, token.GetProperty("SessionHours").GetInt32());
        Assert.False(token.TryGetProperty("DaysToExpiry", out _));
    }
}
