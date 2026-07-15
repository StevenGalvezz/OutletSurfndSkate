using Pedidos360.Services;

namespace Pedidos360.Tests;

public class CalculadoraPedidoTests
{
    private readonly ICalculadoraPedido _calculadora = new CalculadoraPedido();

    [Fact]
    public void CalcularLinea_ConDescuentoEImpuesto_DaElTotalCorrecto()
    {
        var linea = _calculadora.CalcularLinea(productoId: 1, precioUnit: 1000m, cantidad: 2, descuento: 200m, impuestoPorc: 13m);

        Assert.Equal(1800m, linea.Subtotal);
        Assert.Equal(234m, linea.MontoImpuesto);
        Assert.Equal(2034m, linea.TotalLinea);
    }

    [Fact]
    public void CalcularLinea_SinDescuentoNiImpuesto_ElTotalEsElBruto()
    {
        var linea = _calculadora.CalcularLinea(productoId: 1, precioUnit: 500m, cantidad: 3, descuento: 0m, impuestoPorc: 0m);

        Assert.Equal(1500m, linea.Subtotal);
        Assert.Equal(0m, linea.MontoImpuesto);
        Assert.Equal(1500m, linea.TotalLinea);
    }

    [Fact]
    public void CalcularLinea_DescuentoMayorQueElBruto_NoDejaElSubtotalNegativo()
    {
        var linea = _calculadora.CalcularLinea(productoId: 1, precioUnit: 100m, cantidad: 1, descuento: 500m, impuestoPorc: 13m);

        Assert.Equal(0m, linea.Subtotal);
        Assert.Equal(0m, linea.MontoImpuesto);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalcularLinea_CantidadInvalida_LanzaExcepcion(int cantidad)
    {
        Assert.Throws<ArgumentException>(() =>
            _calculadora.CalcularLinea(productoId: 1, precioUnit: 100m, cantidad: cantidad, descuento: 0m, impuestoPorc: 0m));
    }

    [Fact]
    public void CalcularLinea_DescuentoNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            _calculadora.CalcularLinea(productoId: 1, precioUnit: 100m, cantidad: 1, descuento: -5m, impuestoPorc: 0m));
    }

    [Fact]
    public void CalcularTotales_VariasLineas_SumaCadaMonto()
    {
        var lineas = new[]
        {
            _calculadora.CalcularLinea(1, 1000m, 2, 0m, 13m), // subtotal 2000, impuesto 260
            _calculadora.CalcularLinea(2, 500m, 1, 0m, 13m)   // subtotal 500, impuesto 65
        };

        var totales = _calculadora.CalcularTotales(lineas);

        Assert.Equal(2500m, totales.Subtotal);
        Assert.Equal(325m, totales.Impuestos);
        Assert.Equal(2825m, totales.Total);
    }
}
