using Pedidos360.Data;
using Pedidos360.Models;

namespace Pedidos360.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICalculadoraPedido _calculadora;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(ApplicationDbContext context, ICalculadoraPedido calculadora, ILogger<PedidoService> logger)
        {
            _context = context;
            _calculadora = calculadora;
            _logger = logger;
        }

        public async Task<ResultadoPedido> CrearPedidoAsync(int clienteId, string usuarioId, IReadOnlyList<LineaPedidoInput> lineas)
        {
            var resultado = new ResultadoPedido();

            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                resultado.Errores.Add("El cliente seleccionado no existe.");
                return resultado;
            }

            if (lineas.Count == 0)
            {
                resultado.Errores.Add("Agregue al menos un producto al pedido.");
                return resultado;
            }

            // Los precios, impuestos y el stock se vuelven a leer de la base de
            // datos: lo que venga del navegador para esos campos no se usa.
            var lineasCalculadas = new List<LineaCalculada>();
            var productosADescontar = new List<(Producto Producto, int Cantidad)>();

            foreach (var linea in lineas)
            {
                var producto = await _context.Productos.FindAsync(linea.ProductoId);
                if (producto == null || !producto.Activo)
                {
                    resultado.Errores.Add("Uno de los productos del pedido ya no está disponible.");
                    continue;
                }

                if (linea.Cantidad > producto.Stock)
                {
                    _logger.LogWarning("Intento de vender {Cantidad} unidades de {Producto} con solo {Stock} en existencia.",
                        linea.Cantidad, producto.Nombre, producto.Stock);
                    resultado.Errores.Add($"No hay suficiente stock de \"{producto.Nombre}\" (disponible: {producto.Stock}).");
                    continue;
                }

                lineasCalculadas.Add(_calculadora.CalcularLinea(producto.Id, producto.Precio, linea.Cantidad, linea.Descuento, producto.ImpuestoPorc));
                productosADescontar.Add((producto, linea.Cantidad));
            }

            if (resultado.Errores.Count > 0 || lineasCalculadas.Count == 0)
            {
                if (lineasCalculadas.Count == 0 && resultado.Errores.Count == 0)
                {
                    resultado.Errores.Add("Agregue al menos un producto disponible al pedido.");
                }
                return resultado;
            }

            var totales = _calculadora.CalcularTotales(lineasCalculadas);

            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now,
                Subtotal = totales.Subtotal,
                Impuestos = totales.Impuestos,
                Total = totales.Total,
                Estado = "Pendiente"
            };

            foreach (var linea in lineasCalculadas)
            {
                pedido.Detalles.Add(new PedidoDetalle
                {
                    ProductoId = linea.ProductoId,
                    Cantidad = linea.Cantidad,
                    PrecioUnit = linea.PrecioUnit,
                    Descuento = linea.Descuento,
                    ImpuestoPorc = linea.ImpuestoPorc,
                    TotalLinea = linea.TotalLinea
                });
            }

            // se descuenta del inventario lo que se acaba de vender
            foreach (var (producto, cantidad) in productosADescontar)
            {
                producto.Stock -= cantidad;
            }

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido {PedidoId} creado para el cliente {ClienteId} por un total de {Total}.",
                pedido.Id, pedido.ClienteId, pedido.Total);

            resultado.Exitoso = true;
            resultado.Pedido = pedido;
            return resultado;
        }
    }
}
