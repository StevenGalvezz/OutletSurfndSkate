using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Services;

namespace Pedidos360
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Sin RequireConfirmedAccount: el proyecto no tiene un correo real
            // configurado, así que un cliente que se registra puede entrar
            // de una vez en lugar de quedar esperando un correo que nunca llega.
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            // Reglas de negocio del pedido, separadas de los controladores.
            // Si algún día cambia la fórmula de impuestos o la forma de buscar
            // productos, se reemplaza la implementación aquí sin tocar el resto.
            builder.Services.AddScoped<ICalculadoraPedido, CalculadoraPedido>();
            builder.Services.AddScoped<IProductoBusquedaService, ProductoBusquedaService>();
            builder.Services.AddScoped<IPedidoService, PedidoService>();

            // El carrito de compras vive en la sesión del navegador.
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddScoped<ICarritoService, CarritoService>();

            // 1. Configuración de la Cultura es-CR personalizada para Costa Rica (Coma para miles, punto para decimales)
            var defaultCulture = new System.Globalization.CultureInfo("es-CR");
            defaultCulture.NumberFormat.CurrencyGroupSeparator = ",";    // Coma para los miles (ej: ₡100,000.00)
            defaultCulture.NumberFormat.CurrencyDecimalSeparator = ".";  // Punto para los decimales
            defaultCulture.NumberFormat.NumberGroupSeparator = ",";      // Aplica también para números normales
            defaultCulture.NumberFormat.NumberDecimalSeparator = ".";

            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(defaultCulture),
                SupportedCultures = new List<System.Globalization.CultureInfo> { defaultCulture },
                SupportedUICultures = new List<System.Globalization.CultureInfo> { defaultCulture }
            };

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = localizationOptions.DefaultRequestCulture;
                options.SupportedCultures = localizationOptions.SupportedCultures;
                options.SupportedUICultures = localizationOptions.SupportedUICultures;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Captura cualquier excepción no controlada que ocurra en la aplicación
            // y redirige al usuario a la página personalizada del Error 500.
            app.UseExceptionHandler("/Error/500");

            app.UseHttpsRedirection();
            app.UseRouting();

            // 2. Activación del Middleware de localización justo después de UseRouting
            app.UseRequestLocalization(localizationOptions);

            app.UseSession();

            app.UseAuthorization();

            // Si la aplicación devuelve un Error 404, redirige al usuario
            // a la página personalizada de "Página no encontrada".
            app.UseStatusCodePagesWithReExecute("/Error/404");


            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            // 3. Inicialización automática de la base de datos con los datos de prueba (SeedData)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                Pedidos360.Data.SeedData.Initialize(services);
            }

            app.Run();
        }
    }
}