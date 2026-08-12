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
        private readonly IFacturaEmailService _facturaEmailService;

        public CarritoController(
            ApplicationDbContext context,
            ICarritoService carrito,
            ICalculadoraPedido calculadora,
            IPedidoService pedidoService,
            IFacturaEmailService facturaEmailService)
        {
            _context = context;
            _carrito = carrito;
            _calculadora = calculadora;
            _pedidoService = pedidoService;
            _facturaEmailService = facturaEmailService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await ConstruirCarritoViewModelAsync());
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
        public async Task<IActionResult> Actualizar(int productoId, int cantidad)
        {
            if (cantidad <= 0)
            {
                _carrito.Quitar(productoId);
                return RedirectToAction(nameof(Index));
            }

            // el stock real de la base manda, sin importar qué cantidad haya mandado el navegador
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto != null && cantidad > producto.Stock)
            {
                cantidad = producto.Stock;
            }

            _carrito.Actualizar(productoId, cantidad);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Quitar(int productoId)
        {
            _carrito.Quitar(productoId);
            return RedirectToAction(nameof(Index));
        }

        // GET: CARRITO/Checkout — datos de entrega + resumen de la bolsa, antes de confirmar.
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var carritoVm = await ConstruirCarritoViewModelAsync();
            if (carritoVm.Lineas.Count == 0)
            {
                TempData["ErrorCarrito"] = "Agregue al menos un producto antes de pasar al checkout.";
                return RedirectToAction(nameof(Index));
            }

            var miCliente = await ObtenerMiClienteAsync();
            if (miCliente == null)
            {
                TempData["ErrorCarrito"] = "No se encontró su ficha de cliente asociada a esta cuenta.";
                return RedirectToAction(nameof(Index));
            }

            var modelo = new CheckoutViewModel
            {
                Carrito = carritoVm,
                Entrega = new DatosEntregaInput
                {
                    Nombre = miCliente.Nombre,
                    Correo = miCliente.Correo,
                    Telefono = miCliente.Telefono ?? string.Empty,
                    Direccion = miCliente.Direccion
                }
            };

            return View(modelo);
        }

        // POST: CARRITO/Confirmar — cobra la tarjeta simulada, convierte el carrito
        // en un pedido real y manda la factura por correo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(DatosEntregaInput entrega, DatosPagoInput pago)
        {
            var carritoVm = await ConstruirCarritoViewModelAsync();

            if (carritoVm.Lineas.Count == 0)
            {
                TempData["ErrorCarrito"] = "El carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            // el pago se valida antes de tocar la base: una tarjeta rechazada
            // no debe crear un pedido ni descontar stock.
            var resultadoPago = ModelState.IsValid ? SimuladorPago.Procesar(pago) : null;
            if (!ModelState.IsValid || resultadoPago is { Aprobado: false })
            {
                if (resultadoPago is { Aprobado: false })
                {
                    ModelState.AddModelError(string.Empty, resultadoPago.Error!);
                }

                return View("Checkout", new CheckoutViewModel { Carrito = carritoVm, Entrega = entrega, Pago = pago });
            }

            var miCliente = await ObtenerMiClienteAsync();
            if (miCliente == null)
            {
                TempData["ErrorCarrito"] = "No se encontró su ficha de cliente asociada a esta cuenta.";
                return RedirectToAction(nameof(Index));
            }

            // los datos de entrega que se confirman acá también quedan guardados en su ficha
            miCliente.Nombre = entrega.Nombre;
            miCliente.Correo = entrega.Correo;
            miCliente.Telefono = entrega.Telefono;
            miCliente.Direccion = entrega.Direccion;

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var lineas = _carrito.ObtenerItems()
                .Select(i => new LineaPedidoInput { ProductoId = i.ProductoId, Cantidad = i.Cantidad, Descuento = 0 })
                .ToList();

            // el pago ya se aprobó arriba, así que el pedido no arranca en
            // "Pendiente" (eso es para lo que arma el admin sin cobrar todavía)
            var resultado = await _pedidoService.CrearPedidoAsync(miCliente.Id, usuarioId, lineas, estadoInicial: "Procesando");

            if (!resultado.Exitoso)
            {
                foreach (var error in resultado.Errores)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View("Checkout", new CheckoutViewModel { Carrito = carritoVm, Entrega = entrega, Pago = pago });
            }

            // los cambios en miCliente se guardan junto con el pedido: comparten el mismo contexto
            _carrito.Vaciar();

            try
            {
                await _facturaEmailService.EnviarFacturaAsync(resultado.Pedido!, resultadoPago!.CodigoAutorizacion, resultadoPago.UltimosDigitos);
                TempData["MensajePago"] = $"Pago aprobado (aut. {resultadoPago.CodigoAutorizacion}). Te enviamos la factura a {entrega.Correo}.";
            }
            catch (Exception)
            {
                // el pedido ya se creó y el pago "pasó": si el correo falla no se
                // revierte la compra, solo se avisa que la factura no salió.
                TempData["MensajePago"] = $"Pago aprobado (aut. {resultadoPago!.CodigoAutorizacion}), pero no pudimos enviarte la factura por correo.";
                TempData["MensajePagoError"] = true;
            }

            return RedirectToAction("Details", "Pedidos", new { id = resultado.Pedido!.Id });
        }

        private async Task<Cliente?> ObtenerMiClienteAsync()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }

        private async Task<CarritoViewModel> ConstruirCarritoViewModelAsync()
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

            return modelo;
        }
    }
}
