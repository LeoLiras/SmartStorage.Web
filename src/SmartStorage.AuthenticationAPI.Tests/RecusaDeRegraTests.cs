using NSubstitute;
using SmartStorage.AuthenticationAPI.Contract;
using SmartStorage.AuthenticationAPI.Repositories;
using SmartStorage.AuthenticationAPI.Services.Implementations;
using SmartStorage.Shared.Validation;
using SmartStorage_Shared.Model;

namespace SmartStorage.AuthenticationAPI.Tests;

public class RecusaDeRegraTests
{
    private readonly IUserRepository _usuarios = Substitute.For<IUserRepository>();

    private UserAuthServiceImplementation Servico() => new(_usuarios, Substitute.For<IPasswordHasher>());

    [Fact]
    public void Usuario_inexistente_e_recusa_de_regra()
    {
        _usuarios.FindById(42).Returns((User)null);

        var recusa = Assert.Throws<BusinessException>(() => Servico().FindUserById(42));

        Assert.Equal("Usuário não encontrado com o ID informado", recusa.Message);
    }

    [Fact]
    public void Excluir_usuario_que_movimentou_estoque_e_recusa_de_regra_e_nao_apaga()
    {
        _usuarios.HasStockMovements(7).Returns(true);

        var recusa = Assert.Throws<BusinessException>(() => Servico().DeleteUser(7));

        Assert.Equal("Não é possível excluir o usuário pois há movimentações de estoque registradas por ele.", recusa.Message);
        _usuarios.DidNotReceive().Delete(Arg.Any<long>());
    }
}
