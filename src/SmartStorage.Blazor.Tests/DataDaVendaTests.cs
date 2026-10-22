using SmartStorage.Blazor.Utils.Globalization;

namespace SmartStorage.Blazor.Tests;

public class DataDaVendaTests
{
    private static readonly DateTime Envio = new(2026, 10, 22, 15, 55, 37);

    [Fact]
    public void Dia_mantido_desde_a_abertura_da_tela_leva_a_hora_do_envio()
    {
        var abertura = new DateTime(2026, 10, 22, 15, 53, 19);

        var data = SaleDate.At(abertura, Envio);

        Assert.Equal(Envio, data);
    }

    [Fact]
    public void Dia_escolhido_no_calendario_leva_a_hora_do_envio()
    {
        var escolhido = new DateTime(2026, 10, 20);

        var data = SaleDate.At(escolhido, Envio);

        Assert.Equal(new DateTime(2026, 10, 20, 15, 55, 37), data);
    }

    [Fact]
    public void Sem_dia_escolhido_usa_o_instante_do_envio()
    {
        Assert.Equal(Envio, SaleDate.At(null, Envio));
    }

    [Fact]
    public void Data_sai_com_o_fuso_do_navegador()
    {
        var escolhido = DateTime.SpecifyKind(new DateTime(2026, 10, 20), DateTimeKind.Unspecified);

        Assert.Equal(DateTimeKind.Local, SaleDate.At(escolhido, Envio).Kind);
    }
}
