using Pedidos360.Models;

namespace Pedidos360.Services
{
    // Punto único para mandar la factura de un pedido por correo. Separado
    // del envío en sí (IEmailSender) para no mezclar "cómo se manda un
    // correo" con "cómo se ve la factura".
    public interface IFacturaEmailService
    {
        Task EnviarFacturaAsync(Pedido pedido, string codigoAutorizacion, string ultimosDigitosTarjeta);
    }
}
