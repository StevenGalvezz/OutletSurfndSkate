using System.Text.Json;

namespace Pedidos360.Services
{
    public class CarritoService : ICarritoService
    {
        private const string ClaveSesion = "Carrito";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CarritoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Sesion => _httpContextAccessor.HttpContext!.Session;

        public List<ItemCarrito> ObtenerItems()
        {
            var json = Sesion.GetString(ClaveSesion);
            if (string.IsNullOrEmpty(json))
            {
                return new List<ItemCarrito>();
            }

            return JsonSerializer.Deserialize<List<ItemCarrito>>(json) ?? new List<ItemCarrito>();
        }

        public void Agregar(int productoId, int cantidad)
        {
            var items = ObtenerItems();
            var existente = items.FirstOrDefault(i => i.ProductoId == productoId);

            if (existente != null)
            {
                existente.Cantidad += cantidad;
            }
            else
            {
                items.Add(new ItemCarrito { ProductoId = productoId, Cantidad = cantidad });
            }

            Guardar(items);
        }

        public void Actualizar(int productoId, int cantidad)
        {
            var items = ObtenerItems();
            var existente = items.FirstOrDefault(i => i.ProductoId == productoId);
            if (existente == null)
            {
                return;
            }

            existente.Cantidad = cantidad;
            Guardar(items);
        }

        public void Quitar(int productoId)
        {
            var items = ObtenerItems();
            items.RemoveAll(i => i.ProductoId == productoId);
            Guardar(items);
        }

        public void Vaciar()
        {
            Sesion.Remove(ClaveSesion);
        }

        private void Guardar(List<ItemCarrito> items)
        {
            Sesion.SetString(ClaveSesion, JsonSerializer.Serialize(items));
        }
    }
}
