using SmartStorage_API.Service.Nfe;

namespace SmartStorage.API.Tests;

public class LeituraDaNotaFiscalTests
{
    private const string Chave = "35260912345678000195550010000012341000012345";

    internal static string Nota(string itens, string modelo = "55", string id = "NFe" + Chave, string emissao = "<dhEmi>2026-09-15T10:30:00-03:00</dhEmi>") => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
          <NFe>
            <infNFe Id="{id}" versao="4.00">
              <ide>
                <mod>{modelo}</mod>
                <serie>1</serie>
                <nNF>1234</nNF>
                {emissao}
              </ide>
              <emit>
                <CNPJ>12345678000195</CNPJ>
                <xNome>Distribuidora de Ferramentas Ltda</xNome>
              </emit>
              {itens}
            </infNFe>
          </NFe>
          <protNFe versao="4.00"><infProt><chNFe>{Chave}</chNFe></infProt></protNFe>
        </nfeProc>
        """;

    internal static string Item(int numero, string cean, string descricao, string unidade, string quantidade, string unitario, string total) => $"""
        <det nItem="{numero}">
          <prod>
            <cProd>F-{numero}</cProd>
            <cEAN>{cean}</cEAN>
            <xProd>{descricao}</xProd>
            <uCom>{unidade}</uCom>
            <qCom>{quantidade}</qCom>
            <vUnCom>{unitario}</vUnCom>
            <vProd>{total}</vProd>
          </prod>
        </det>
        """;

    [Fact]
    public void Le_cabecalho_e_itens_da_nota()
    {
        var nfe = NfeXmlReader.Read(Nota(
            Item(1, "7891000100103", "Chave Philips 3/16", "CX", "2.0000", "120.0000000000", "240.00") +
            Item(2, "7891000100110", "Fita Isolante 19mm", "UN", "10.0000", "8.9000000000", "89.00")));

        Assert.Equal(Chave, nfe.Chave);
        Assert.Equal("1234", nfe.Numero);
        Assert.Equal("1", nfe.Serie);
        Assert.Equal("12345678000195", nfe.EmitenteCnpj);
        Assert.Equal("Distribuidora de Ferramentas Ltda", nfe.EmitenteNome);
        Assert.Equal(new DateTime(2026, 9, 15, 10, 30, 0), nfe.DataEmissao);
        Assert.Equal(2, nfe.Items.Count);

        var caixa = nfe.Items[0];
        Assert.Equal(1, caixa.Numero);
        Assert.Equal("F-1", caixa.CodigoProduto);
        Assert.Equal("7891000100103", caixa.Codigo);
        Assert.Null(caixa.CodigoInvalido);
        Assert.Equal("Chave Philips 3/16", caixa.Descricao);
        Assert.Equal("CX", caixa.Unidade);
        Assert.Equal(2m, caixa.Quantidade);
        Assert.Equal(120m, caixa.ValorUnitario);
        Assert.Equal(240m, caixa.ValorTotal);
    }

    [Fact]
    public void Item_sem_gtin_fica_sem_codigo()
    {
        var nfe = NfeXmlReader.Read(Nota(Item(1, "SEM GTIN", "Parafuso a granel", "KG", "1.5000", "30.00", "45.00")));

        Assert.Null(nfe.Items[0].Codigo);
        Assert.Null(nfe.Items[0].CodigoInvalido);
        Assert.Equal(1.5m, nfe.Items[0].Quantidade);
    }

    [Fact]
    public void Codigo_com_digito_verificador_errado_fica_como_invalido()
    {
        var nfe = NfeXmlReader.Read(Nota(Item(1, "7891000100104", "Chave Philips 3/16", "UN", "1", "10", "10")));

        Assert.Null(nfe.Items[0].Codigo);
        Assert.Equal("7891000100104", nfe.Items[0].CodigoInvalido);
    }

    [Theory]
    [InlineData("4006381333931", true)]
    [InlineData("7891000100103", true)]
    [InlineData("78910003", true)]
    [InlineData("17891000100100", true)]
    [InlineData("7891000100104", false)]
    [InlineData("789100010010", false)]
    [InlineData("78910A0100103", false)]
    [InlineData("", false)]
    public void Confere_o_digito_verificador_do_gtin(string codigo, bool valido)
    {
        Assert.Equal(valido, NfeXmlReader.IsValidGtin(codigo));
    }

    [Fact]
    public void Chave_vem_do_protocolo_quando_o_id_nao_tem_prefixo()
    {
        var nfe = NfeXmlReader.Read(Nota(Item(1, "SEM GTIN", "Parafuso a granel", "UN", "1", "1", "1"), id: "sem-prefixo"));

        Assert.Equal(Chave, nfe.Chave);
    }

    [Fact]
    public void Aceita_data_de_emissao_da_versao_antiga()
    {
        var nfe = NfeXmlReader.Read(Nota(Item(1, "SEM GTIN", "Parafuso a granel", "UN", "1", "1", "1"), emissao: "<dEmi>2010-05-20</dEmi>"));

        Assert.Equal(new DateTime(2010, 5, 20), nfe.DataEmissao);
    }

    [Theory]
    [InlineData("isto nao e xml", "não é um XML válido")]
    [InlineData("<pedido><item/></pedido>", "não é uma NF-e")]
    public void Recusa_arquivo_que_nao_e_nfe(string conteudo, string mensagem)
    {
        var erro = Assert.Throws<Exception>(() => NfeXmlReader.Read(conteudo));

        Assert.Contains(mensagem, erro.Message);
    }

    [Fact]
    public void Recusa_nota_de_consumidor()
    {
        var erro = Assert.Throws<Exception>(() => NfeXmlReader.Read(Nota(Item(1, "SEM GTIN", "Parafuso", "UN", "1", "1", "1"), modelo: "65")));

        Assert.Contains("modelo 55", erro.Message);
    }

    [Fact]
    public void Recusa_nota_sem_itens()
    {
        var erro = Assert.Throws<Exception>(() => NfeXmlReader.Read(Nota(string.Empty)));

        Assert.Contains("não tem itens", erro.Message);
    }
}
