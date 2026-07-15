namespace Pedidos360.Services
{
    // Los montos que salen de calcular una sola línea del pedido.
    public class LineaCalculada
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal Descuento { get; set; }
        public decimal ImpuestoPorc { get; set; }
        public decimal Subtotal { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal TotalLinea { get; set; }
    }

    // Los tres montos que se muestran al pie del pedido.
    public class TotalesPedido
    {
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
    }

    // Toda la matemática del pedido vive aquí para que el controlador
    // solo tenga que pedir el resultado, nunca calcularlo él mismo.
    public interface ICalculadoraPedido
    {
        LineaCalculada CalcularLinea(int productoId, decimal precioUnit, int cantidad, decimal descuento, decimal impuestoPorc);

        TotalesPedido CalcularTotales(IEnumerable<LineaCalculada> lineas);
    }
}
