using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Models;
using Pedidos360.Services;

namespace Pedidos360.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICarritoService _carrito;
        private readonly ICalculadoraPedido _calculadora;
        private readonly IPedidoService _pedidoService;

        public CarritoController(
            ApplicationDbContext context,
            ICarritoService carrito,
            ICalculadoraPedido calculadora,
            IPedidoService pedidoService)
        {
            _context = context;
            _carrito = carrito;
            _calculadora = calculadora;
            _pedidoService = pedidoService;
        }

        public async Task<IActionResult> Index()
        {
            var modelo = new CarritoViewModel();
            var lineasCalculadas = new List<LineaCalculada>();

            foreach (var item in _carrito.ObtenerItems())
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null || !producto.Activo)
                {
                    continue;
                }

                // si el stock bajó desde que se agregó al carrito, se ajusta la cantidad mostrada
                var cantidad = Math.Min(item.Cantidad, producto.Stock);
                if (cantidad <= 0)
                {
                    continue;
                }

                var calculo = _calculadora.CalcularLinea(producto.Id, producto.Precio, cantidad, 0, producto.ImpuestoPorc);
                lineasCalculadas.Add(calculo);

                modelo.Lineas.Add(new LineaCarritoViewModel
                {
                    Producto = producto,
                    Cantidad = cantidad,
                    Subtotal = calculo.Subtotal,
                    MontoImpuesto = calculo.MontoImpuesto,
                    TotalLinea = calculo.TotalLinea
                });
            }

            var totales = _calculadora.CalcularTotales(lineasCalculadas);
            modelo.Subtotal = totales.Subtotal;
            modelo.Impuestos = totales.Impuestos;
            modelo.Total = totales.Total;

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null || !producto.Activo || producto.Stock <= 0)
            {
                TempData["ErrorCarrito"] = "Ese producto ya no está disponible.";
                return RedirectToAction("Index", "Tienda");
            }

            _carrito.Agregar(productoId, Math.Max(1, cantidad));
            return RedirectToAction("Index", "Tienda");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Actualizar(int productoId, int cantidad)
        {
            if (cantidad <= 0)
            {
                _carrito.Quitar(productoId);
            }
            else
            {
                _carrito.Actualizar(productoId, cantidad);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Quitar(int productoId)
        {
            _carrito.Quitar(productoId);
            return RedirectToAction(nameof(Index));
        }

        // POST: CARRITO/Confirmar — convierte el carrito en un pedido real.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar()
        {
            var items = _carrito.ObtenerItems();
            if (items.Count == 0)
            {
                TempData["ErrorCarrito"] = "El carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var miCliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (miCliente == null)
            {
                TempData["ErrorCarrito"] = "No se encontró su ficha de cliente asociada a esta cuenta.";
                return RedirectToAction(nameof(Index));
            }

            var lineas = items
                .Select(i => new LineaPedidoInput { ProductoId = i.ProductoId, Cantidad = i.Cantidad, Descuento = 0 })
                .ToList();

            var resultado = await _pedidoService.CrearPedidoAsync(miCliente.Id, usuarioId, lineas);

            if (!resultado.Exitoso)
            {
                TempData["ErrorCarrito"] = string.Join(" ", resultado.Errores);
                return RedirectToAction(nameof(Index));
            }

            _carrito.Vaciar();
            return RedirectToAction("Details", "Pedidos", new { id = resultado.Pedido!.Id });
        }
    }
}
