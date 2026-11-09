using Microsoft.AspNetCore.Mvc;
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
        // Sem business de propósito: se a recusa deixasse a chamada passar, a mensagem seria a da falha de referência nula.
        var controller = new ShelfController(null);

        var resposta = controller.AllocateProductToShelf(new EnterVO
        {
            ProductId = 1,
            ShelfId = 1,
            ProductQuantity = 1,
            ProductPrice = decimal.Parse(preco, System.Globalization.CultureInfo.InvariantCulture),
        });

        var recusa = Assert.IsType<BadRequestObjectResult>(resposta);
        Assert.Equal("O preço deve ser maior que zero.", recusa.Value);
    }
}
