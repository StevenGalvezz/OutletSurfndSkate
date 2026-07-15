using System.ComponentModel.DataAnnotations;
using Pedidos360.Models;

namespace Pedidos360.Tests;

public class ValidacionesModeloTests
{
    private static IList<ValidationResult> Validar(object modelo)
    {
        var contexto = new ValidationContext(modelo);
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, contexto, resultados, validateAllProperties: true);
        return resultados;
    }

    private static Cliente ClienteValido() => new()
    {
        Nombre = "Juan Pérez",
        Cedula = "1-1234-5678",
        Correo = "juan@correo.com",
        Telefono = "8888-9999",
        Direccion = "San José"
    };

    [Fact]
    public void Cliente_ConDatosCorrectos_EsValido()
    {
        Assert.Empty(Validar(ClienteValido()));
    }

    [Fact]
    public void Cliente_CedulaConLetras_EsInvalida()
    {
        var cliente = ClienteValido();
        cliente.Cedula = "abc-123";

        var resultados = Validar(cliente);

        Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Cliente.Cedula)));
    }

    [Fact]
    public void Cliente_TelefonoConLetras_EsInvalido()
    {
        var cliente = ClienteValido();
        cliente.Telefono = "ochenta";

        var resultados = Validar(cliente);

        Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Cliente.Telefono)));
    }

    [Fact]
    public void Cliente_CorreoSinArroba_EsInvalido()
    {
        var cliente = ClienteValido();
        cliente.Correo = "juan-correo.com";

        var resultados = Validar(cliente);

        Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Cliente.Correo)));
    }

    [Fact]
    public void PedidoDetalleInput_CantidadCero_EsInvalido()
    {
        var linea = new PedidoDetalleInput { ProductoId = 1, Cantidad = 0, Descuento = 0 };

        var resultados = Validar(linea);

        Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(PedidoDetalleInput.Cantidad)));
    }

    [Fact]
    public void PedidoDetalleInput_DescuentoNegativo_EsInvalido()
    {
        var linea = new PedidoDetalleInput { ProductoId = 1, Cantidad = 1, Descuento = -10 };

        var resultados = Validar(linea);

        Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(PedidoDetalleInput.Descuento)));
    }
}
