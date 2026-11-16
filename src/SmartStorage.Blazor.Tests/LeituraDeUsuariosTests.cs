using System.Net.Http;
using Microsoft.JSInterop;
using NSubstitute;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Provider;
using SmartStorage.Blazor.Services;
using SmartStorage.Shared.Enum;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// A API passou a responder UserAccountVO, com "type" no lugar de "useType". Lido
/// direto como User, o papel chegava sempre como Usuario, e o EditUser rebaixava o
/// administrador ao salvar.
/// </summary>
public class LeituraDeUsuariosTests
{
    private const string Usuarios = "api/auth/v1";

    private static readonly object Admin = new { id = 1, username = "admin", fullName = "Administrador", type = 1 };

    private static AuthService Servico(ApiFalsa api)
        => new(api.Cliente(), new AuthStateProvider(Substitute.For<IJSRuntime>()), new SessionExpiration());

    [Fact]
    public async Task Lista_os_usuarios_com_o_papel_da_api()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, Usuarios, new[]
        {
            Admin,
            new { id = 2, username = "usuario", fullName = "Usuário Comum", type = 0 },
        });

        var usuarios = await Servico(api).GetUser();

        Assert.Equal(new[] { TipoUsuario.Administrador, TipoUsuario.Usuario }, usuarios.Select(u => u.UseType));
        Assert.Equal(new long[] { 1, 2 }, usuarios.Select(u => u.Id));
        Assert.Equal("Administrador", usuarios[0].FullName);
    }

    [Fact]
    public async Task Busca_por_id_mantem_o_papel_de_administrador()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, $"{Usuarios}/1", Admin);

        var usuario = await Servico(api).GetUser(1);

        Assert.Equal(TipoUsuario.Administrador, usuario.UseType);
        Assert.Equal("admin", usuario.Username);
    }

    [Fact]
    public async Task Usuario_logado_vem_da_minha_conta_com_o_papel_de_administrador()
    {
        var api = new ApiFalsa().Responde(HttpMethod.Get, $"{Usuarios}/my-account", Admin);

        var usuario = await Servico(api).GetUser("admin");

        Assert.Equal(TipoUsuario.Administrador, usuario.UseType);
        Assert.Equal(1, usuario.Id);
        Assert.Single(api.Requisicoes, r => r.Caminho == $"{Usuarios}/my-account");
        Assert.Equal("", api.Consultas.Single());
    }
}
