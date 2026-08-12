using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Models
{
    // Una línea del carrito ya con el producto cargado y los montos calculados,
    // lista para pintar en la vista.
    public class LineaCarritoViewModel
    {
        public Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal TotalLinea { get; set; }
    }

    public class CarritoViewModel
    {
        public List<LineaCarritoViewModel> Lineas { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
    }

    // Los datos de entrega que se confirman (o corrigen) en el checkout.
    // Vienen precargados de la ficha del cliente y, si los cambia acá,
    // también se actualiza su ficha al confirmar el pedido.
    public class DatosEntregaInput
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres.")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessage = "El teléfono solo puede contener números, espacios o guiones.")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "La dirección de entrega es obligatoria.")]
        [StringLength(250, ErrorMessage = "La dirección no puede exceder los 250 caracteres.")]
        public string Direccion { get; set; } = null!;
    }

    // Datos de una tarjeta de pago simulada: solo se usan para validar el
    // formulario y calcular una "aprobación" con el algoritmo de Luhn.
    // Nunca se guardan en la base ni se registran en logs.
    public class DatosPagoInput
    {
        [Required(ErrorMessage = "El nombre en la tarjeta es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre en la tarjeta")]
        public string NombreTitular { get; set; } = null!;

        [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
        [RegularExpression(@"^[0-9 ]{13,19}$", ErrorMessage = "Ingrese un número de tarjeta válido (13 a 19 dígitos).")]
        [Display(Name = "Número de tarjeta")]
        public string NumeroTarjeta { get; set; } = null!;

        [Required(ErrorMessage = "El vencimiento es obligatorio.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Use el formato MM/AA.")]
        [Display(Name = "Vencimiento (MM/AA)")]
        public string Vencimiento { get; set; } = null!;

        [Required(ErrorMessage = "El CVV es obligatorio.")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "El CVV debe tener 3 o 4 dígitos.")]
        public string Cvv { get; set; } = null!;
    }

    public class CheckoutViewModel
    {
        public DatosEntregaInput Entrega { get; set; } = new();
        public DatosPagoInput Pago { get; set; } = new();
        public CarritoViewModel Carrito { get; set; } = new();
    }
}
