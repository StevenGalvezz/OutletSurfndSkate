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

                // 1. SEED DE CATEGORÍAS (Si no hay ninguna, las crea)
                if (!context.Categorias.Any())
                {
                    context.Categorias.AddRange(
                        new Categoria { Nombre = "General" },
                        new Categoria { Nombre = "Ropa Streetwear" }
                    );
                    context.SaveChanges(); // se guarda para generar los IDs en la BD
                }

                // Buscamos los IDs reales que la base de datos asignó para amarrar los productos
                var catGeneral = context.Categorias.FirstOrDefault(c => c.Nombre == "General")?.Id ?? 1;
                var catRopa = context.Categorias.FirstOrDefault(c => c.Nombre == "Ropa Streetwear")?.Id ?? 2;

                // 2. SEED DE PRODUCTOS (Si la tabla está vacía, mete los datos oficiales)
                if (!context.Productos.Any())
                {
                    context.Productos.AddRange(
                        new Producto { Nombre = "Pantalon Baggy Cargo Southpole", CategoriaId = catRopa, Precio = 18000, ImpuestoPorc = 13, Stock = 20, ImagenUrl = "/images/cargo.jpg", Activo = true },
                        new Producto { Nombre = "Camiseta Vintage Comfort Colors", CategoriaId = catRopa, Precio = 12000, ImpuestoPorc = 13, Stock = 40, ImagenUrl = "/images/tshirt.jpg", Activo = true }
                    );
                }

                // 3. SEED DE CLIENTES (Si la tabla está vacía, los mete)
                if (!context.Clientes.Any())
                {
                    context.Clientes.AddRange(
                        new Cliente { Nombre = "Distribuidora San Pedro S.A.", Cedula = "3-101-123456", Correo = "info@sanpedro.com", Telefono = "2222-1111", Direccion = "San José, Montes de Oca" },
                        new Cliente { Nombre = "Tienda El Parque S.A.", Cedula = "3-102-789012", Correo = "compras@elparque.com", Telefono = "2440-2222", Direccion = "Alajuela Centro" },
                        new Cliente { Nombre = "Juan Carlos Mendoza", Cedula = "1-1234-5678", Correo = "juan.mendoza@gmail.com", Telefono = "8888-9999", Direccion = "Heredia, San Francisco" }
                    );
                }

                context.SaveChanges(); // Guarda todos los productos y clientes finales
            }
        }
    }
}
