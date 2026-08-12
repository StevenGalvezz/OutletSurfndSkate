namespace Pedidos360.Services
{
    // Una línea tal como la manda quien arma el pedido (admin o carrito del cliente).
    public class LineaPedidoInput
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Descuento { get; set; }
    }

    public class ResultadoPedido
    {
        public bool Exitoso { get; set; }
        public Pedidos360.Models.Pedido? Pedido { get; set; }
        public List<string> Errores { get; set; } = new();
    }

    // Punto único para crear un pedido: lo usa tanto el panel del administrador
    // como el checkout del carrito del cliente, para no repetir la misma
    // validación y el mismo cálculo en dos lados.
    public interface IPedidoService
    {
        // estadoInicial: "Pendiente" para lo que arma el admin a mano (todavía
        // no hay pago registrado), "Procesando" para lo que ya pasó por el
        // checkout con el pago simulado aprobado.
        Task<ResultadoPedido> CrearPedidoAsync(int clienteId, string usuarioId, IReadOnlyList<LineaPedidoInput> lineas, string estadoInicial = "Pendiente");
    }
}
