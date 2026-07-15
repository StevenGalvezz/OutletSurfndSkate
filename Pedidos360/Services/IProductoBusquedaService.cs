namespace Pedidos360.Services
{
    // Lo que le devolvemos al buscador AJAX por cada producto encontrado.
    public class ProductoBusquedaResultado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Categoria { get; set; } = null!;
        public decimal Precio { get; set; }
        public decimal ImpuestoPorc { get; set; }
        public int Stock { get; set; }
    }

    // Busca productos disponibles para agregarlos a un pedido.
    public interface IProductoBusquedaService
    {
        Task<List<ProductoBusquedaResultado>> BuscarAsync(string? termino);
    }
}
