// Busca productos en vivo mientras se escribe, sin recargar la página:
// pide el mismo listado ya armado en el servidor y reemplaza la cuadrícula.
(function () {
    var buscador = document.getElementById("buscador-tienda");
    var contenedor = document.getElementById("grid-productos-contenedor");

    if (!buscador || !contenedor) {
        return;
    }

    var temporizador = null;

    function buscar(termino) {
        fetch("/Tienda/Buscar?termino=" + encodeURIComponent(termino))
            .then(function (respuesta) { return respuesta.text(); })
            .then(function (html) { contenedor.innerHTML = html; })
            .catch(function () {
                contenedor.innerHTML = "<p class=\"sin-resultados-tienda\">No se pudo hacer la búsqueda. Intente de nuevo.</p>";
            });
    }

    buscador.addEventListener("input", function () {
        clearTimeout(temporizador);
        var termino = buscador.value.trim();
        temporizador = setTimeout(function () { buscar(termino); }, 250);
    });
})();
