// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Botón de modo oscuro/claro: guarda la preferencia para que se mantenga al recargar.
(function () {
    var boton = document.getElementById("theme-toggle");
    if (!boton) {
        return;
    }

    function temaActual() {
        return document.documentElement.getAttribute("data-theme")
            || (window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");
    }

    function actualizarTexto() {
        boton.textContent = temaActual() === "dark" ? "Modo claro" : "Modo oscuro";
    }

    boton.addEventListener("click", function () {
        var nuevoTema = temaActual() === "dark" ? "light" : "dark";
        document.documentElement.setAttribute("data-theme", nuevoTema);
        localStorage.setItem("tema", nuevoTema);
        actualizarTexto();
    });

    actualizarTexto();
})();
