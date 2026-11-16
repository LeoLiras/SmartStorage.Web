using SmartStorage.Shared.Enum;
using SmartStorage.Shared.VO;
using SmartStorage_Shared.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SmartStorage.AuthenticationAPI.Tests;

public class VOsDeUsuarioTests
{
    private static User Usuario() => new()
    {
        Id = 7,
        Username = "usuario",
        FullName = "Usuário Comum",
        Password = "hash-da-senha",
        RefreshToken = "refresh-secreto",
        RefreshTokenExpiryTime = new DateTime(2026, 11, 16),
        UseType = TipoUsuario.Administrador
    };

    private static string[] Propriedades<T>() =>
        typeof(T).GetProperties().Select(p => p.Name.ToLowerInvariant()).ToArray();

    [Fact]
    public void UserAccountVO_copia_so_os_dados_da_conta()
    {
        var vo = UserAccountVO.Parse(Usuario());

        Assert.Equal(7, vo.Id);
        Assert.Equal("usuario", vo.Username);
        Assert.Equal("Usuário Comum", vo.FullName);
        Assert.Equal(TipoUsuario.Administrador, vo.Type);
    }

    [Fact]
    public void UserAccountVO_serializado_nao_tem_senha_nem_refresh_token()
    {
        var json = JsonSerializer.Serialize(UserAccountVO.ParseList([Usuario()]));

        Assert.DoesNotContain("hash-da-senha", json);
        Assert.DoesNotContain("refresh-secreto", json);
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refreshToken", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_de_nulo_devolve_nulo()
    {
        Assert.Null(UserAccountVO.Parse(null));
        Assert.Null(UserAccountVO.ParseList(null));
    }

    [Fact]
    public void Nenhum_VO_de_usuario_tem_refresh_token()
    {
        Assert.DoesNotContain(Propriedades<UserAccountVO>(), p => p.Contains("refresh"));
        Assert.DoesNotContain(Propriedades<UserUpdateVO>(), p => p.Contains("refresh"));
        Assert.DoesNotContain(Propriedades<MyAccountUpdateVO>(), p => p.Contains("refresh"));
    }

    [Fact]
    public void UserUpdateVO_e_UserAccountVO_nao_tem_senha()
    {
        Assert.DoesNotContain(Propriedades<UserUpdateVO>(), p => p.Contains("password"));
        Assert.DoesNotContain(Propriedades<UserAccountVO>(), p => p.Contains("password"));
    }

    private static List<ValidationResult> Validar(object vo)
    {
        var erros = new List<ValidationResult>();
        Validator.TryValidateObject(vo, new ValidationContext(vo), erros, validateAllProperties: true);
        return erros;
    }

    [Fact]
    public void UserUpdateVO_recusa_nome_vazio_longo_e_tipo_inexistente()
    {
        Assert.Empty(Validar(new UserUpdateVO { FullName = "Usuário Comum", Type = TipoUsuario.Usuario }));

        Assert.Contains(Validar(new UserUpdateVO { FullName = "" }), e => e.ErrorMessage == "O nome completo é obrigatório.");
        Assert.Contains(Validar(new UserUpdateVO { FullName = new string('a', 121) }), e => e.ErrorMessage == "Insira no máximo 120 caracteres.");
        Assert.Contains(Validar(new UserUpdateVO { FullName = "Usuário Comum", Type = (TipoUsuario)9 }), e => e.ErrorMessage == "Tipo de usuário inválido.");
    }

    [Fact]
    public void MyAccountUpdateVO_exige_a_senha_atual_e_aceita_sem_senha_nova()
    {
        Assert.Empty(Validar(new MyAccountUpdateVO { FullName = "Usuário Comum", CurrentPassword = "usuario123" }));

        Assert.Contains(Validar(new MyAccountUpdateVO { FullName = "Usuário Comum" }), e => e.ErrorMessage == "A senha atual é obrigatória.");
    }
}
