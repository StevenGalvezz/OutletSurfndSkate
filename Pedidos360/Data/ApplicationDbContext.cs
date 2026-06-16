using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Models;

namespace Pedidos360.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Nuevas tablas en la Base de Datos
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidosDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mantiene la configuración interna de ASP.NET Core Identity (Roles y Usuarios)
            base.OnModelCreating(modelBuilder);

            // Configuraciones de precisión para campos Decimales (Evita truncados en BD)
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Producto>()
                .Property(p => p.ImpuestoPorc)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Subtotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Impuestos)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PedidoDetalle>()
                .Property(pd => pd.PrecioUnit)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PedidoDetalle>()
                .Property(pd => pd.Descuento)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PedidoDetalle>()
                .Property(pd => pd.ImpuestoPorc)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<PedidoDetalle>()
                .Property(pd => pd.TotalLinea)
                .HasColumnType("decimal(18,2)");
        }
    }
}