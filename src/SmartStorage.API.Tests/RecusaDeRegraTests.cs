using Microsoft.AspNetCore.Http;
using SmartStorage.Infraestructure.Context;
using SmartStorage.Shared.Validation;
using SmartStorage_API.Repository;
using SmartStorage_API.Service.Implementations;

namespace SmartStorage.API.Tests;

public class RecusaDeRegraTests
{
    // Os casos abaixo recusam antes de qualquer consulta, então o contexto não precisa de banco de verdade.
    private static ProductStockMovementRepository Ledger()
        => new(new SmartStorageContextFactory().CreateDbContext(Array.Empty<string>()), new HttpContextAccessor());

    private static ProductBusinessImplementation Produtos()
        => new(new SmartStorageContextFactory().CreateDbContext(Array.Empty<string>()), null, null);

    [Fact]
    public void Transferencia_sem_quantidade_e_recusa_de_regra()
    {
        var recusa = Assert.Throws<BusinessException>(() => Ledger().TransferProductBetweenLocations(1, null, 1, 0));

        Assert.Equal("A quantidade da transferência deve ser maior que zero.", recusa.Message);
    }

    [Fact]
    public void Transferencia_para_o_mesmo_local_e_recusa_de_regra()
    {
        var recusa = Assert.Throws<BusinessException>(() => Ledger().TransferProductBetweenLocations(1, 2, 2, 5));

        Assert.Equal("A origem e o destino da transferência são o mesmo local.", recusa.Message);
    }

    [Fact]
    public void Movimentacao_sem_usuario_autenticado_e_erro_inesperado_e_nao_recusa()
    {
        // Os endpoints exigem token: chegar ao ledger sem usuário é falha interna, que vira 500 genérico, não 400.
        var erro = Assert.Throws<InvalidOperationException>(() => Ledger().CreateNewStockMovement(1, null, SmartStorage.Shared.Enum.TipoMovimentacao.Ajuste, 1, "motivo"));

        Assert.IsNotType<BusinessException>(erro);
        Assert.Equal("Não foi possível identificar o usuário autenticado.", erro.Message);
    }

    [Theory]
    [InlineData(0, 10, "A página deve ser maior que zero.")]
    [InlineData(1, 0, "O tamanho da página deve estar entre 1 e 100.")]
    [InlineData(1, 101, "O tamanho da página deve estar entre 1 e 100.")]
    public void Pagina_invalida_de_produtos_e_recusa_de_regra(int pagina, int tamanho, string mensagem)
    {
        var recusa = Assert.Throws<BusinessException>(() => Produtos().FindProductsPage(pagina, tamanho, null));

        Assert.Equal(mensagem, recusa.Message);
    }

    [Theory]
    [InlineData("Repository", "ProductStockMovementRepository.cs")]
    [InlineData("Business", "Implementations", "ProductBusinessImplementation.cs")]
    [InlineData("Business", "Implementations", "EmployeeBusinessImplementation.cs")]
    public void Ledger_e_business_de_produtos_e_colaboradores_nao_lancam_Exception_generica(params string[] caminho)
    {
        // Lê o arquivo versionado: uma Exception genérica nova, depois do handler ligado, viraria 500 onde era 400.
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);

        while (pasta is not null && !File.Exists(Path.Combine(pasta.FullName, "SmartStorageWeb.sln")))
            pasta = pasta.Parent;

        Assert.NotNull(pasta);

        var codigo = File.ReadAllText(Path.Combine([pasta.FullName, "SmartStorage.API", .. caminho]));

        Assert.DoesNotContain("throw new Exception(", codigo);
    }
}
