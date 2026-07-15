using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;

namespace Pedidos360.Services
{
    public class ProductoBusquedaService : IProductoBusquedaService
    {
        private readonly ApplicationDbContext _context;

        public ProductoBusquedaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductoBusquedaResultado>> BuscarAsync(string? termino)
        {
            // solo productos activos y con existencia se pueden vender
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0);

            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p => p.Nombre.Contains(termino));
            }

            return await query
                .OrderBy(p => p.Nombre)
                .Take(15)
                .Select(p => new ProductoBusquedaResultado
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Categoria = p.Categoria!.Nombre,
                    Precio = p.Precio,
                    ImpuestoPorc = p.ImpuestoPorc,
                    Stock = p.Stock
                })
                .ToListAsync();
        }
    }
}
