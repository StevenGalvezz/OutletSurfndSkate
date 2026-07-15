using Microsoft.AspNetCore.Identity;
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
                // Aplica las migraciones pendientes (crea la base si no existe)
                context.Database.Migrate();

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
                // Estos son cuentas de negocio que carga el administrador a mano,
                // por eso no tienen UsuarioId: no inician sesión, solo se facturan.
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

            // 4. SEED DE ROLES Y USUARIOS DE PRUEBA (Administrador y Cliente)
            SeedRolesYUsuarios(serviceProvider);
        }

        private static void SeedRolesYUsuarios(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var rol in new[] { "Administrador", "Cliente" })
            {
                if (!roleManager.RoleExistsAsync(rol).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole(rol)).GetAwaiter().GetResult();
                }
            }

            CrearUsuarioDemo(serviceProvider, userManager, "admin@outletsurfskate.com", "Admin#2026", "Administrador");
            CrearUsuarioDemo(serviceProvider, userManager, "cliente@outletsurfskate.com", "Cliente#2026", "Cliente");
        }

        private static void CrearUsuarioDemo(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, string correo, string clave, string rol)
        {
            var usuario = userManager.FindByEmailAsync(correo).GetAwaiter().GetResult();
            if (usuario != null)
            {
                return;
            }

            usuario = new IdentityUser
            {
                UserName = correo,
                Email = correo,
                EmailConfirmed = true // usuario de prueba: se salta la confirmación por correo
            };

            var resultado = userManager.CreateAsync(usuario, clave).GetAwaiter().GetResult();
            if (!resultado.Succeeded)
            {
                return;
            }

            userManager.AddToRoleAsync(usuario, rol).GetAwaiter().GetResult();

            // Al usuario Cliente de prueba se le crea su ficha, tal como haría
            // el registro real, para que ya tenga con qué comprar.
            if (rol == "Cliente")
            {
                using var context = new ApplicationDbContext(
                    serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

                context.Clientes.Add(new Cliente
                {
                    Nombre = "Cliente de Prueba",
                    Cedula = "1-0000-0000",
                    Correo = correo,
                    Telefono = "8000-0000",
                    Direccion = "San José, Costa Rica",
                    UsuarioId = usuario.Id
                });
                context.SaveChanges();
            }
        }
    }
}
