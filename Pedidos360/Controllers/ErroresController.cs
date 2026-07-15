using Microsoft.AspNetCore.Mvc;

namespace Pedidos360.Controllers
{
    // Controlador encargado de mostrar las páginas de error personalizadas.
    public class ErroresController : Controller
    {
        // Muestra la página personalizada para el Error 404 (Página no encontrada).
        [Route("Error/404")]
        public IActionResult Error404()
        {
            return View("NotFound");
        }

        // Muestra la página personalizada para el Error 500 (Error interno del servidor).
        [Route("Error/500")]
        public IActionResult Error500()
        {
            return View("ServerError");
        }

        // Redirige al usuario a la página personalizada del Error 404.
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {     
                
            return View("NotFound");
        }

        // Método temporal para comprobar el funcionamiento del Error 500.
        [Route("Error/Probar500")]
        public IActionResult Probar500()
        {
            int numero = 10;
            int divisor = 0;

            int resultado = numero / divisor;

            return View();
        }
    }
}