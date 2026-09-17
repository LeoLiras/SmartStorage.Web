using SmartStorage.Blazor.Utils.Variables;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// Os holders de objeto do estado global precisam comecar nulos. Ja foram
/// inicializados com new(), o que tornava todo `is null` do app inalcancavel:
/// as telas recebiam um VO vazio em vez de buscar o registro por id, e a venda
/// saia com ProductId zero.
/// </summary>
public class EstadoGlobalTests
{
    [Fact]
    public void ActualProduct_comeca_nulo()
    {
        var app = new VariablesExtensions();

        Assert.Null(app.ActualProduct);
    }

    [Fact]
    public void ActualEntry_comeca_nulo()
    {
        var app = new VariablesExtensions();

        Assert.Null(app.ActualEntry);
    }

    [Fact]
    public void User_continua_inicializado_porque_usa_sentinela_de_id()
    {
        var app = new VariablesExtensions();

        Assert.NotNull(app.User);
        Assert.Equal(0, app.User.Id);
    }

    [Fact]
    public void Colecoes_continuam_inicializadas()
    {
        var app = new VariablesExtensions();

        Assert.NotNull(app.Employees);
        Assert.NotNull(app.ProductEntries);
        Assert.NotNull(app.Sales);
        Assert.NotNull(app.Shelves);
        Assert.NotNull(app.ProductInStock);
    }
}
