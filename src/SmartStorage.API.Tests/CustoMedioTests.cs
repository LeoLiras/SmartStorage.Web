using SmartStorage_API.Service.Implementations;

namespace SmartStorage.API.Tests;

public class CustoMedioTests
{
    [Fact]
    public void Produto_sem_custo_recebe_o_da_nota()
    {
        Assert.Equal(10m, InvoiceBusinessImplementation.AverageCost(null, 30, 10m, 12));
    }

    [Fact]
    public void Produto_sem_saldo_recebe_o_da_nota()
    {
        Assert.Equal(10m, InvoiceBusinessImplementation.AverageCost(7m, 0, 10m, 12));
    }

    [Fact]
    public void Media_ponderada_pelo_saldo_e_pela_entrada()
    {
        Assert.Equal(8.5m, InvoiceBusinessImplementation.AverageCost(10m, 10, 7m, 10));
        Assert.Equal(8.2m, InvoiceBusinessImplementation.AverageCost(10m, 12, 5.5m, 8));
    }

    [Fact]
    public void Arredonda_em_quatro_casas()
    {
        Assert.Equal(3.3333m, InvoiceBusinessImplementation.AverageCost(2m, 1, 4m, 2));
    }
}
