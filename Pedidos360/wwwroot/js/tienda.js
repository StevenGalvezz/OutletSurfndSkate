// Busca productos en vivo mientras se escribe o se cambia la categoría,
// sin recargar la página: pide el mismo listado ya armado en el servidor
// y reemplaza la cuadrícula.
(function () {
    var buscador = document.getElementById("buscador-tienda");
    var filtroCategoria = document.getElementById("filtro-categoria");
    var contenedor = document.getElementById("grid-productos-contenedor");

    if (!buscador || !contenedor) {
        return;
    }

    var temporizador = null;

    function buscar() {
        var termino = buscador.value.trim();
        var categoriaId = filtroCategoria ? filtroCategoria.value : "";
        var parametros = "termino=" + encodeURIComponent(termino) + "&categoriaId=" + encodeURIComponent(categoriaId);

        fetch("/Tienda/Buscar?" + parametros)
            .then(function (respuesta) { return respuesta.text(); })
            .then(function (html) { contenedor.innerHTML = html; })
            .catch(function () {
                contenedor.innerHTML = "<p class=\"sin-resultados-tienda\">No se pudo hacer la búsqueda. Intente de nuevo.</p>";
            });
    }

    buscador.addEventListener("input", function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(buscar, 250);
    });

    if (filtroCategoria) {
        filtroCategoria.addEventListener("change", buscar);
    }
})();
