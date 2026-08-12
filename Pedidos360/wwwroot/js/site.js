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
        document.documentElement.setAttribute("data-bs-theme", nuevoTema);
        localStorage.setItem("tema", nuevoTema);
        actualizarTexto();
    });

    actualizarTexto();
})();

// Botones +/- de cantidad (carrito y tienda): solo ajustan el número en pantalla,
// el formulario sigue mandando el valor final cuando se envía.
(function () {
    document.addEventListener("click", function (evento) {
        var boton = evento.target.closest(".btn-stepper");
        if (!boton) {
            return;
        }

        var input = document.getElementById(boton.dataset.target);
        if (!input) {
            return;
        }

        var paso = Number(boton.dataset.paso);
        var min = Number(input.min) || 1;
        var max = Number(input.max) || Infinity;
        var nuevoValor = Number(input.value) + paso;

        if (nuevoValor < min) nuevoValor = min;
        if (nuevoValor > max) nuevoValor = max;

        input.value = nuevoValor;
    });
})();
