using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Pedidos360.Models;

namespace Pedidos360.Services
{
    public class FacturaEmailService : IFacturaEmailService
    {
        private static readonly CultureInfo Moneda = CultureInfo.GetCultureInfo("es-CR");

        private readonly IEmailSender _emailSender;
        private readonly ILogger<FacturaEmailService> _logger;

        public FacturaEmailService(IEmailSender emailSender, ILogger<FacturaEmailService> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task EnviarFacturaAsync(Pedido pedido, string codigoAutorizacion, string ultimosDigitosTarjeta)
        {
            var correo = pedido.Cliente?.Correo;
            if (string.IsNullOrWhiteSpace(correo))
            {
                _logger.LogWarning("El pedido {PedidoId} no tiene un correo de cliente al que mandar la factura.", pedido.Id);
                return;
            }

            var asunto = $"Factura de tu pedido #{pedido.Id} — Outlet Surf&Skate";
            var html = ConstruirHtml(pedido, codigoAutorizacion, ultimosDigitosTarjeta);
            await _emailSender.SendEmailAsync(correo, asunto, html);
        }

        private static string ConstruirHtml(Pedido pedido, string codigoAutorizacion, string ultimosDigitosTarjeta)
        {
            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Arial,sans-serif;max-width:600px;margin:0 auto;color:#17181b\">");

            sb.Append("<h2 style=\"text-transform:uppercase;letter-spacing:.05em\">Outlet Surf&amp;Skate</h2>");
            sb.Append($"<p>Gracias por tu compra, {Encode(pedido.Cliente?.Nombre)}. Tu pago fue aprobado y este es el comprobante de tu pedido.</p>");

            sb.Append("<table style=\"width:100%;border-collapse:collapse;margin:16px 0\">");
            sb.Append(FilaResumen("N.º de pedido", $"#{pedido.Id}"));
            sb.Append(FilaResumen("Fecha", pedido.Fecha.ToString("dd/MM/yyyy HH:mm")));
            sb.Append(FilaResumen("Autorización (simulada)", codigoAutorizacion));
            sb.Append(FilaResumen("Tarjeta", $"**** **** **** {ultimosDigitosTarjeta}"));
            sb.Append("</table>");

            sb.Append("<table style=\"width:100%;border-collapse:collapse;margin-bottom:16px\">");
            sb.Append("<thead><tr style=\"border-bottom:2px solid #17181b;text-align:left\">");
            sb.Append("<th style=\"padding:6px 4px\">Producto</th><th style=\"padding:6px 4px;text-align:center\">Cant.</th><th style=\"padding:6px 4px;text-align:right\">Total</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var linea in pedido.Detalles)
            {
                sb.Append("<tr style=\"border-bottom:1px solid #ddd\">");
                sb.Append($"<td style=\"padding:6px 4px\">{Encode(linea.Producto?.Nombre)}</td>");
                sb.Append($"<td style=\"padding:6px 4px;text-align:center\">{linea.Cantidad}</td>");
                sb.Append($"<td style=\"padding:6px 4px;text-align:right\">{linea.TotalLinea.ToString("C", Moneda)}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");

            sb.Append("<table style=\"width:100%;border-collapse:collapse;max-width:260px;margin-left:auto\">");
            sb.Append(FilaResumen("Subtotal", pedido.Subtotal.ToString("C", Moneda)));
            sb.Append(FilaResumen("Impuestos", pedido.Impuestos.ToString("C", Moneda)));
            sb.Append(FilaResumen("Total", pedido.Total.ToString("C", Moneda), negrita: true));
            sb.Append("</table>");

            sb.Append("<p style=\"margin-top:24px;font-size:12px;color:#6c6e72\">Este es un pago simulado con fines de práctica/demo — no se realizó ningún cargo real.</p>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private static string FilaResumen(string etiqueta, string valor, bool negrita = false)
        {
            var peso = negrita ? "font-weight:bold" : "";
            return $"<tr><td style=\"padding:2px 4px;color:#6c6e72\">{Encode(etiqueta)}</td><td style=\"padding:2px 4px;text-align:right;{peso}\">{Encode(valor)}</td></tr>";
        }

        private static string Encode(string? valor) => WebUtility.HtmlEncode(valor ?? string.Empty);
    }
}
