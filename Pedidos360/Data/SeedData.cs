using Microsoft.EntityFrameworkCore;
using Pedidos360.Models;

namespace Pedidos360.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Asegura que la base de datos exista
                context.Database.EnsureCreated();

                // Si ya hay categorías, no hace nada
                if (context.Categorias.Any()) return;

                // Agregar categorías de prueba
                var categoriaDefault = new Categoria { Nombre = "General" };
                context.Categorias.Add(categoriaDefault);
                context.SaveChanges();

                // ============== Aquí meter luego los 10-20 productos de prueba automáticamente ===================
            }
        }
    }
}
