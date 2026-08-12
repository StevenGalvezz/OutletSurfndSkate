using Pedidos360.Models;

namespace Pedidos360.Services
{
    public class ResultadoPago
    {
        public bool Aprobado { get; set; }
        public string? Error { get; set; }

        // Solo para mostrar/loguear algo parecido a un comprobante real,
        // sin nunca guardar ni exponer el número completo de la tarjeta.
        public string UltimosDigitos { get; set; } = string.Empty;
        public string CodigoAutorizacion { get; set; } = string.Empty;
    }

    // Simula la aprobación de un pago con tarjeta: no se conecta a ninguna
    // pasarela real ni cobra nada. Solo valida que el número "parezca" una
    // tarjeta válida (algoritmo de Luhn, el mismo truco que usan las
    // tarjetas de prueba de Stripe/otros) y que el vencimiento no haya
    // pasado. El número y el CVV se descartan apenas se valida.
    public static class SimuladorPago
    {
        public static ResultadoPago Procesar(DatosPagoInput pago)
        {
            var digitos = new string(pago.NumeroTarjeta.Where(char.IsDigit).ToArray());

            if (!EsLuhnValido(digitos))
            {
                return new ResultadoPago { Aprobado = false, Error = "La tarjeta fue rechazada: el número no es válido." };
            }

            if (!VencimientoVigente(pago.Vencimiento))
            {
                return new ResultadoPago { Aprobado = false, Error = "La tarjeta está vencida." };
            }

            return new ResultadoPago
            {
                Aprobado = true,
                UltimosDigitos = digitos.Length >= 4 ? digitos[^4..] : digitos,
                CodigoAutorizacion = "SIM-" + Random.Shared.Next(100000, 999999)
            };
        }

        private static bool EsLuhnValido(string digitos)
        {
            if (digitos.Length < 13)
            {
                return false;
            }

            var suma = 0;
            var duplicar = false;
            for (var i = digitos.Length - 1; i >= 0; i--)
            {
                var d = digitos[i] - '0';
                if (duplicar)
                {
                    d *= 2;
                    if (d > 9)
                    {
                        d -= 9;
                    }
                }

                suma += d;
                duplicar = !duplicar;
            }

            return suma % 10 == 0;
        }

        private static bool VencimientoVigente(string vencimiento)
        {
            var partes = vencimiento.Split('/');
            if (partes.Length != 2 || !int.TryParse(partes[0], out var mes) || !int.TryParse(partes[1], out var anioCorto))
            {
                return false;
            }

            var anio = 2000 + anioCorto;
            var finDeMes = new DateTime(anio, mes, 1).AddMonths(1).AddDays(-1);
            return finDeMes >= DateTime.Today;
        }
    }
}
