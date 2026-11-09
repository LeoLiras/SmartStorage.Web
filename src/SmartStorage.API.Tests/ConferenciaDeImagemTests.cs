using System.Text;
using SmartStorage.Shared.Validation;

namespace SmartStorage.API.Tests;

public class ConferenciaDeImagemTests
{
    private static readonly byte[] CabecalhoPng = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    private static readonly byte[] CabecalhoJpeg = { 0xFF, 0xD8, 0xFF, 0xE0 };

    private static byte[] ComTamanho(byte[] cabecalho, int tamanho)
    {
        var arquivo = new byte[tamanho];
        cabecalho.CopyTo(arquivo, 0);

        return arquivo;
    }

    [Fact]
    public void Png_passa()
    {
        Assert.Null(ProductImagePolicy.Validate(ComTamanho(CabecalhoPng, 64)));
    }

    [Fact]
    public void Jpeg_passa()
    {
        Assert.Null(ProductImagePolicy.Validate(ComTamanho(CabecalhoJpeg, 64)));
    }

    [Fact]
    public void Imagem_nula_ou_vazia_passa_porque_o_produto_pode_nao_ter_imagem()
    {
        Assert.Null(ProductImagePolicy.Validate(null));
        Assert.Null(ProductImagePolicy.Validate(Array.Empty<byte>()));
    }

    [Fact]
    public void Gif_e_recusado()
    {
        var gif = Encoding.ASCII.GetBytes("GIF89a");

        Assert.Equal("A imagem do produto precisa ser PNG ou JPEG.", ProductImagePolicy.Validate(ComTamanho(gif, 64)));
    }

    [Fact]
    public void Texto_e_recusado()
    {
        var texto = Encoding.UTF8.GetBytes("isto não é uma imagem");

        Assert.Equal("A imagem do produto precisa ser PNG ou JPEG.", ProductImagePolicy.Validate(texto));
    }

    [Theory]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47 })]
    [InlineData(new byte[] { 0xFF, 0xD8 })]
    [InlineData(new byte[] { 0x00, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    public void Assinatura_incompleta_ou_fora_do_inicio_e_recusada(byte[] arquivo)
    {
        Assert.Equal("A imagem do produto precisa ser PNG ou JPEG.", ProductImagePolicy.Validate(arquivo));
    }

    [Fact]
    public void Exatamente_10_MB_passa()
    {
        Assert.Null(ProductImagePolicy.Validate(ComTamanho(CabecalhoPng, 10 * 1024 * 1024)));
    }

    [Fact]
    public void Um_byte_acima_de_10_MB_e_recusado()
    {
        Assert.Equal("A imagem do produto não pode passar de 10 MB.", ProductImagePolicy.Validate(ComTamanho(CabecalhoJpeg, 10 * 1024 * 1024 + 1)));
    }
}
