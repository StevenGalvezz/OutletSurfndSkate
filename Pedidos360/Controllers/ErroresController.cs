using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Pedidos360.Controllers
{
    // Controlador encargado de mostrar las páginas de error personalizadas.
    // El [AllowAnonymous] va en cada acción pública (no en la clase) para
    // que no le gane por accidente al [Authorize] de Probar500.
    public class ErroresController : Controller
    {
        private readonly ILogger<ErroresController> _logger;

        public ErroresController(ILogger<ErroresController> logger)
        {
            _logger = logger;
        }

        // Muestra la página personalizada para el Error 404 (Página no encontrada).
        [AllowAnonymous]
        [Route("Error/404")]
        public IActionResult Error404()
        {
            _logger.LogWarning("Página no encontrada: {Ruta}", HttpContext.Request.Path);
            return View("NotFound");
        }

        // Muestra la página personalizada para el Error 500 (Error interno del servidor)
        // y deja registrada la excepción real que la provocó.
        [AllowAnonymous]
        [Route("Error/500")]
        public IActionResult Error500()
        {
            var excepcion = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (excepcion != null)
            {
                _logger.LogError(excepcion.Error, "Error 500 en {Ruta}", excepcion.Path);
            }

            return View("ServerError");
        }

        // Redirige al usuario a la página personalizada del Error 404.
        [AllowAnonymous]
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            _logger.LogWarning("Código de estado {Codigo} en {Ruta}", statusCode, HttpContext.Request.Path);
            return View("NotFound");
        }

        // Ruta de prueba para forzar un 500 y ver que el manejo de errores funciona.
        // Solo el administrador puede dispararla: no debe quedar abierta a cualquiera.
        [Route("Error/Probar500")]
        [Authorize(Roles = "Administrador")]
        public IActionResult Probar500()
        {
            int numero = 10;
            int divisor = 0;

            int resultado = numero / divisor;

            return View();
        }
    }
}