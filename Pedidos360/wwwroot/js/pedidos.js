// Arma el pedido en el navegador: busca productos por AJAX y va sumando
// los totales en pantalla a medida que se agregan o cambian líneas.
(function () {
    var buscador = document.getElementById("buscador");
    var resultadosDiv = document.getElementById("resultados-busqueda");
    var cuerpoTabla = document.getElementById("lineas-body");
    var form = document.getElementById("form-pedido");

    if (!buscador || !form) {
        return;
    }

    var lineas = []; // { productoId, nombre, precio, impuestoPorc, stock, cantidad, descuento }
    var temporizadorBusqueda = null;

    function formatoColones(numero) {
        var valor = (Math.round(numero * 100) / 100).toFixed(2);
        var partes = valor.split(".");
        partes[0] = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        return "₡" + partes.join(".");
    }

    function buscarProductos(termino) {
        fetch("/Pedidos/BuscarProductos?termino=" + encodeURIComponent(termino))
            .then(function (respuesta) { return respuesta.json(); })
            .then(mostrarResultados)
            .catch(function () {
                resultadosDiv.innerHTML = "<div class=\"sin-resultados\">No se pudo consultar el catálogo.</div>";
                resultadosDiv.classList.remove("d-none");
            });
    }

    function mostrarResultados(productos) {
        resultadosDiv.innerHTML = "";

        if (!productos.length) {
            resultadosDiv.innerHTML = "<div class=\"sin-resultados\">No hay productos que coincidan.</div>";
            resultadosDiv.classList.remove("d-none");
            return;
        }

        productos.forEach(function (producto) {
            var boton = document.createElement("button");
            boton.type = "button";
            boton.textContent = producto.nombre + " — " + formatoColones(producto.precio) + " (stock: " + producto.stock + ")";
            boton.addEventListener("click", function () {
                agregarProducto(producto);
                resultadosDiv.classList.add("d-none");
                buscador.value = "";
            });
            resultadosDiv.appendChild(boton);
        });

        resultadosDiv.classList.remove("d-none");
    }

    function agregarProducto(producto) {
        var existente = lineas.find(function (l) { return l.productoId === producto.id; });

        if (existente) {
            if (existente.cantidad < existente.stock) {
                existente.cantidad += 1;
            }
        } else {
            lineas.push({
                productoId: producto.id,
                nombre: producto.nombre,
                precio: producto.precio,
                impuestoPorc: producto.impuestoPorc,
                stock: producto.stock,
                cantidad: 1,
                descuento: 0
            });
        }

        renderizarLineas();
    }

    function quitarLinea(indice) {
        lineas.splice(indice, 1);
        renderizarLineas();
    }

    function calcularSubtotalLinea(linea) {
        var bruto = (linea.precio * linea.cantidad) - linea.descuento;
        return bruto < 0 ? 0 : bruto;
    }

    function renderizarLineas() {
        cuerpoTabla.innerHTML = "";

        if (lineas.length === 0) {
            cuerpoTabla.innerHTML = "<tr><td colspan=\"6\">Todavía no ha agregado productos.</td></tr>";
            actualizarTotales();
            return;
        }

        lineas.forEach(function (linea, indice) {
            var fila = document.createElement("tr");

            var subtotalLinea = calcularSubtotalLinea(linea);

            fila.innerHTML =
                "<td>" + linea.nombre +
                "<input type=\"hidden\" name=\"Detalles[" + indice + "].ProductoId\" value=\"" + linea.productoId + "\" /></td>" +
                "<td>" + formatoColones(linea.precio) + "</td>" +
                "<td><input type=\"number\" class=\"form-control input-cantidad\" name=\"Detalles[" + indice + "].Cantidad\" " +
                    "value=\"" + linea.cantidad + "\" min=\"1\" max=\"" + linea.stock + "\" step=\"1\" data-indice=\"" + indice + "\" /></td>" +
                "<td><input type=\"number\" class=\"form-control input-descuento\" name=\"Detalles[" + indice + "].Descuento\" " +
                    "value=\"" + linea.descuento + "\" min=\"0\" step=\"0.01\" data-indice=\"" + indice + "\" /></td>" +
                "<td class=\"subtotal-linea\">" + formatoColones(subtotalLinea) + "</td>" +
                "<td><button type=\"button\" class=\"btn btn-sm btn-danger btn-quitar\" data-indice=\"" + indice + "\">Quitar</button></td>";

            cuerpoTabla.appendChild(fila);
        });

        cuerpoTabla.querySelectorAll(".input-cantidad").forEach(function (input) {
            input.addEventListener("input", function () {
                var indice = Number(input.dataset.indice);
                var linea = lineas[indice];
                var cantidad = Math.floor(Number(input.value));

                if (!cantidad || cantidad < 1) {
                    cantidad = 1;
                }
                if (cantidad > linea.stock) {
                    cantidad = linea.stock;
                }

                input.value = cantidad;
                linea.cantidad = cantidad;
                actualizarFilaYTotales(indice);
            });
        });

        cuerpoTabla.querySelectorAll(".input-descuento").forEach(function (input) {
            input.addEventListener("input", function () {
                var indice = Number(input.dataset.indice);
                var descuento = Number(input.value);

                if (isNaN(descuento) || descuento < 0) {
                    descuento = 0;
                }

                input.value = descuento;
                lineas[indice].descuento = descuento;
                actualizarFilaYTotales(indice);
            });
        });

        cuerpoTabla.querySelectorAll(".btn-quitar").forEach(function (boton) {
            boton.addEventListener("click", function () {
                quitarLinea(Number(boton.dataset.indice));
            });
        });

        actualizarTotales();
    }

    function actualizarFilaYTotales(indice) {
        var fila = cuerpoTabla.children[indice];
        var subtotalLinea = calcularSubtotalLinea(lineas[indice]);
        fila.querySelector(".subtotal-linea").textContent = formatoColones(subtotalLinea);
        actualizarTotales();
    }

    function actualizarTotales() {
        var subtotal = 0;
        var impuestos = 0;

        lineas.forEach(function (linea) {
            var subtotalLinea = calcularSubtotalLinea(linea);
            subtotal += subtotalLinea;
            impuestos += subtotalLinea * (linea.impuestoPorc / 100);
        });

        document.getElementById("txt-subtotal").textContent = formatoColones(subtotal);
        document.getElementById("txt-impuestos").textContent = formatoColones(impuestos);
        document.getElementById("txt-total").textContent = formatoColones(subtotal + impuestos);
    }

    buscador.addEventListener("input", function () {
        var texto = buscador.value.trim();
        clearTimeout(temporizadorBusqueda);

        if (texto.length === 0) {
            resultadosDiv.classList.add("d-none");
            return;
        }

        temporizadorBusqueda = setTimeout(function () { buscarProductos(texto); }, 250);
    });

    document.addEventListener("click", function (evento) {
        if (!resultadosDiv.contains(evento.target) && evento.target !== buscador) {
            resultadosDiv.classList.add("d-none");
        }
    });

    form.addEventListener("submit", function (evento) {
        if (lineas.length === 0) {
            evento.preventDefault();
            alert("Agregue al menos un producto antes de guardar el pedido.");
        }
    });
})();
