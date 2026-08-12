using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Index(string? termino, int? categoriaId)
        {
            ViewData["Termino"] = termino;
            ViewData["CategoriaId"] = categoriaId;
            ViewData["Categorias"] = await ObtenerCategoriasConStockAsync(categoriaId);
            return View(await BuscarProductosAsync(termino, categoriaId));
        }

        // GET: TIENDA/Buscar?termino=camiseta&categoriaId=2
        // Endpoint AJAX: devuelve solo la cuadrícula de productos ya renderizada,
        // para que la búsqueda funcione sin recargar la página.
        [HttpGet]
        public async Task<IActionResult> Buscar(string? termino, int? categoriaId)
        {
            return PartialView("_GridProductos", await BuscarProductosAsync(termino, categoriaId));
        }

        private async Task<List<Producto>> BuscarProductosAsync(string? termino, int? categoriaId)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0);

            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p => p.Nombre.Contains(termino));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            return await query.OrderBy(p => p.Nombre).ToListAsync();
        }

        // Solo categorías con algo comprable: no tiene sentido ofrecer un
        // filtro que siempre va a devolver la bolsa vacía.
        private async Task<SelectList> ObtenerCategoriasConStockAsync(int? categoriaId)
        {
            var categorias = await _context.Categorias
                .Where(c => c.Productos.Any(p => p.Activo && p.Stock > 0))
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return new SelectList(categorias, "Id", "Nombre", categoriaId);
        }
    }
}
