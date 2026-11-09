using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SmartStorage.Infraestructure.Context;
using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Migrations;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage.API.Tests;

public class CustoNaVendaTests
{
    [Fact]
    public void Custo_da_venda_e_opcional_e_tem_a_precisao_do_custo_do_produto()
    {
        using var context = new SmartStorageContextFactory().CreateDbContext(Array.Empty<string>());

        var custo = context.Model.FindEntityType(typeof(Sale)).FindProperty(nameof(Sale.SalCusto));

        Assert.True(custo.IsNullable);
        Assert.Equal(18, custo.GetPrecision());
        Assert.Equal(4, custo.GetScale());
    }

    [Fact]
    public void Migration_so_cria_a_coluna_e_deixa_as_vendas_antigas_sem_custo()
    {
        var operacao = Assert.Single(new AddSaleCost().UpOperations);

        var coluna = Assert.IsType<AddColumnOperation>(operacao);
        Assert.Equal(nameof(Sale.SalCusto), coluna.Name);
        Assert.Equal("Sale", coluna.Table);
        Assert.True(coluna.IsNullable);
        Assert.Null(coluna.DefaultValue);
        Assert.Null(coluna.DefaultValueSql);
    }

    [Fact]
    public void Custo_enviado_na_requisicao_nao_chega_a_venda()
    {
        var enviada = new SaleVO { IdEnter = 1, Qntd = 1, SaleCost = 99m };

        Assert.Null(SaleVO.Parse(enviada).SalCusto);
        Assert.Null(new SaleConverter(null).Parse(enviada).SalCusto);
    }
}
