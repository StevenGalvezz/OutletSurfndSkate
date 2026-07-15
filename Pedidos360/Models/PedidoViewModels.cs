using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Models
{
    // Lo que llega del formulario de "nuevo pedido".
    public class PedidoCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente.")]
        public int ClienteId { get; set; }

        [MinLength(1, ErrorMessage = "Agregue al menos un producto al pedido.")]
        public List<PedidoDetalleInput> Detalles { get; set; } = new();
    }

    // Una línea del pedido tal como la arma el usuario en pantalla.
    // El precio y el impuesto no viajan desde aquí: se vuelven a leer
    // del producto en el servidor para que nadie los pueda alterar.
    public class PedidoDetalleInput
    {
        [Required]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo.")]
        public decimal Descuento { get; set; }
    }
}
