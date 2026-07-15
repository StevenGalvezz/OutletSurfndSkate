namespace Pedidos360.Services
{
    // Una línea del carrito: solo guardamos el producto y la cantidad.
    // El precio se vuelve a consultar cada vez que hace falta, nunca se
    // guarda en la sesión (así nunca queda desactualizado).
    public class ItemCarrito
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public interface ICarritoService
    {
        List<ItemCarrito> ObtenerItems();
        void Agregar(int productoId, int cantidad);
        void Actualizar(int productoId, int cantidad);
        void Quitar(int productoId);
        void Vaciar();
    }
}
