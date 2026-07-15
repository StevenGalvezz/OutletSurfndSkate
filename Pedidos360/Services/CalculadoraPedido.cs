namespace Pedidos360.Services
{
    public class CalculadoraPedido : ICalculadoraPedido
    {
        public LineaCalculada CalcularLinea(int productoId, decimal precioUnit, int cantidad, decimal descuento, decimal impuestoPorc)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));
            }

            if (descuento < 0)
            {
                throw new ArgumentException("El descuento no puede ser negativo.", nameof(descuento));
            }

            // precio x cantidad, menos el descuento, sin dejarlo bajar de cero
            var subtotal = (precioUnit * cantidad) - descuento;
            if (subtotal < 0)
            {
                subtotal = 0;
            }

            var montoImpuesto = Math.Round(subtotal * (impuestoPorc / 100m), 2, MidpointRounding.AwayFromZero);
            subtotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);

            return new LineaCalculada
            {
                ProductoId = productoId,
                Cantidad = cantidad,
                PrecioUnit = precioUnit,
                Descuento = descuento,
                ImpuestoPorc = impuestoPorc,
                Subtotal = subtotal,
                MontoImpuesto = montoImpuesto,
                TotalLinea = subtotal + montoImpuesto
            };
        }

        public TotalesPedido CalcularTotales(IEnumerable<LineaCalculada> lineas)
        {
            var lista = lineas.ToList();

            return new TotalesPedido
            {
                Subtotal = lista.Sum(l => l.Subtotal),
                Impuestos = lista.Sum(l => l.MontoImpuesto),
                Total = lista.Sum(l => l.TotalLinea)
            };
        }
    }
}
