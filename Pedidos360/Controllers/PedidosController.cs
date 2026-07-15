using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Models;
using Pedidos360.Services;

namespace Pedidos360.Controllers
{
    // El administrador ve y arma cualquier pedido (por ejemplo, uno que
    // entró por teléfono); el cliente solo ve los suyos y no puede armar
    // uno a nombre de otro.
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPedidoService _pedidoService;
        private readonly IProductoBusquedaService _busqueda;

        public PedidosController(
            ApplicationDbContext context,
            IPedidoService pedidoService,
            IProductoBusquedaService busqueda)
        {
            _context = context;
            _pedidoService = pedidoService;
            _busqueda = busqueda;
        }

        // GET: PEDIDOS
        public async Task<IActionResult> Index()
        {
            var query = _context.Pedidos.Include(p => p.Cliente).AsQueryable();

            if (User.IsInRole("Cliente"))
            {
                var miClienteId = await ObtenerClienteIdDelUsuarioAsync();
                query = query.Where(p => p.ClienteId == miClienteId);
            }

            var pedidos = await query.OrderByDescending(p => p.Fecha).ToListAsync();
            return View(pedidos);
        }

        // GET: PEDIDOS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Cliente"))
            {
                var miClienteId = await ObtenerClienteIdDelUsuarioAsync();
                if (pedido.ClienteId != miClienteId)
                {
                    return NotFound();
                }
            }

            return View(pedido);
        }

        // GET: PEDIDOS/Create
        // Solo el administrador arma pedidos manualmente (ej. por teléfono);
        // el cliente compra desde la Tienda con su carrito.
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nombre), "Id", "Nombre");
            return View(new PedidoCreateViewModel());
        }

        // GET: PEDIDOS/BuscarProductos?termino=camiseta
        // Endpoint AJAX que alimenta la barra de búsqueda del formulario.
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> BuscarProductos(string? termino)
        {
            var resultados = await _busqueda.BuscarAsync(termino);
            return Json(resultados);
        }

        // POST: PEDIDOS/Create
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nombre), "Id", "Nombre", model.ClienteId);
                return View(model);
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var lineas = model.Detalles
                .Select(d => new LineaPedidoInput { ProductoId = d.ProductoId, Cantidad = d.Cantidad, Descuento = d.Descuento })
                .ToList();

            var resultado = await _pedidoService.CrearPedidoAsync(model.ClienteId, usuarioId, lineas);

            if (!resultado.Exitoso)
            {
                foreach (var error in resultado.Errores)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nombre), "Id", "Nombre", model.ClienteId);
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = resultado.Pedido!.Id });
        }

        private async Task<int?> ObtenerClienteIdDelUsuarioAsync()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            return cliente?.Id;
        }
    }
}
