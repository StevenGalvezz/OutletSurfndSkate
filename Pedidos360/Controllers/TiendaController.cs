using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Models;

namespace Pedidos360.Controllers
{
    // Catálogo de compra para el cliente: solo lo que está activo y hay en existencia.
    [Authorize(Roles = "Cliente")]
    public class TiendaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TiendaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? termino)
        {
            ViewData["Termino"] = termino;
            return View(await BuscarProductosAsync(termino));
        }

        // GET: TIENDA/Buscar?termino=camiseta
        // Endpoint AJAX: devuelve solo la cuadrícula de productos ya renderizada,
        // para que la búsqueda funcione sin recargar la página.
        [HttpGet]
        public async Task<IActionResult> Buscar(string? termino)
        {
            return PartialView("_GridProductos", await BuscarProductosAsync(termino));
        }

        private async Task<List<Producto>> BuscarProductosAsync(string? termino)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0);

            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p => p.Nombre.Contains(termino));
            }

            return await query.OrderBy(p => p.Nombre).ToListAsync();
        }
    }
}
