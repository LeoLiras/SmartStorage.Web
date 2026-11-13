using SmartStorage.Shared.Validation;
using SmartStorage_API.Controllers;
using SmartStorage_Shared.VO;

namespace SmartStorage.API.Tests;

public class AlocacaoIndividualTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("-5")]
    public void Preco_zero_ou_negativo_e_recusado(string preco)
    {
        // Sem business de propósito: se a recusa deixasse a chamada passar, a exceção seria a da falha de referência nula.
        var controller = new ShelfController(null);

        // O controller só lança: quem transforma a recusa em 400 é o tratador de erros do pipeline.
        var recusa = Assert.Throws<BusinessException>(() => controller.AllocateProductToShelf(new EnterVO
        {
            ProductId = 1,
            ShelfId = 1,
            ProductQuantity = 1,
            ProductPrice = decimal.Parse(preco, System.Globalization.CultureInfo.InvariantCulture),
        }));

        Assert.Equal("O preço deve ser maior que zero.", recusa.Message);
    }
}
