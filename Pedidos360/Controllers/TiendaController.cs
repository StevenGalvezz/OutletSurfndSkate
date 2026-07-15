using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;

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

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(productos);
        }
    }
}
