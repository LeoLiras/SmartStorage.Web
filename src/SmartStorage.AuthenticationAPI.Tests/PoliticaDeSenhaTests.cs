using SmartStorage.Shared.Validation;

namespace SmartStorage.AuthenticationAPI.Tests;

public class PoliticaDeSenhaTests
{
    [Theory]
    [InlineData("adm@123")]
    [InlineData("senhasemnumero!")]
    [InlineData("12345678!")]
    [InlineData("admin123")]
    [InlineData("admin 123")]
    [InlineData("")]
    [InlineData(null)]
    public void Senha_fora_da_regra_e_recusada_com_a_mensagem_da_regra(string senha)
    {
        Assert.Equal(PasswordPolicy.RuleMessage, PasswordPolicy.Validate(senha));
    }

    [Theory]
    [InlineData("admin@123")]
    [InlineData("Senha#2026")]
    [InlineData("abc-12345")]
    [InlineData("ação~2026")]
    public void Senha_com_letra_numero_e_especial_passa(string senha)
    {
        Assert.Null(PasswordPolicy.Validate(senha));
    }

    [Fact]
    public void Maiuscula_nao_e_exigida()
    {
        Assert.Null(PasswordPolicy.Validate("minuscula@1"));
    }
}
