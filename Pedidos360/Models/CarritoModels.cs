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
}
