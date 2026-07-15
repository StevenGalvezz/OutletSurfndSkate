# Checklist de calidad — Hito 2

Lista para repasar a mano antes de dar por lista la demo. Marcar cada punto probándolo en el navegador (o con `dotnet test` donde se indique).

## Registro y roles

- [ ] Registrarse como cliente nuevo (nombre, cédula, teléfono, dirección, correo, contraseña) y entrar de una vez, sin tener que confirmar correo.
- [ ] Revisar que ese cliente nuevo aparece en la lista de Clientes del Administrador con los mismos datos que escribió.
- [ ] Entrar como `admin@outletsurfskate.com` (clave `Admin#2026`) y confirmar que puede entrar a Pedidos, Clientes, Productos y Categorías.
- [ ] Entrar como `cliente@outletsurfskate.com` (clave `Cliente#2026`) y confirmar que solo ve Tienda, Carrito y Mis pedidos.
- [ ] Con la sesión de Cliente, intentar entrar a `/Productos`, `/Clientes` o `/Categorias` a mano por la URL → debe negar el acceso.
- [ ] Cerrar sesión y entrar a cualquier pantalla sin loguearse → debe mandar al login.

## Compra (Tienda + Carrito)

- [ ] En la Tienda, cada producto muestra su foto (o el cuadro de "sin imagen" si la URL no carga).
- [ ] Agregar un producto al carrito y ver que el contador del menú sube.
- [ ] En el carrito, cambiar la cantidad de una línea y ver que el subtotal de esa línea y el resumen se actualizan.
- [ ] Quitar una línea del carrito.
- [ ] Intentar agregar más unidades de las que hay en existencia → no debe dejar pasar de ese límite.
- [ ] Confirmar el pedido y revisar en Detalles que los totales coinciden con lo que se veía en el carrito.
- [ ] Revisar que el stock del producto bajó lo correspondiente después de confirmar.
- [ ] El carrito queda vacío después de confirmar.
- [ ] Ese cliente ve su pedido en "Mis pedidos", pero no puede ver el pedido de otro cliente cambiando el número en la URL.

## Panel del administrador

- [ ] El administrador puede armar un pedido manual desde `/Pedidos/Create` eligiendo cualquier cliente (por ejemplo, uno de los clientes de negocio que no tienen cuenta propia).
- [ ] El administrador ve todos los pedidos en `/Pedidos`, no solo los suyos.
- [ ] Crear/editar un producto sigue validando los campos numéricos y no acepta letras donde solo van números.

## Validaciones de formularios

- [ ] Cédula con letras (tanto en Clientes como en el registro) → rechazada.
- [ ] Correo sin arroba → rechazado.
- [ ] Dejar un campo obligatorio vacío → el formulario muestra el error antes de enviar, sin recargar.

## Errores y logging

- [ ] Entrar a una ruta inexistente → aparece la pantalla personalizada de 404.
- [ ] Como Administrador, entrar a `/Error/Probar500` → aparece la pantalla personalizada de 500 y queda una línea de error en la consola/log de la aplicación.
- [ ] Como Cliente, intentar entrar a `/Error/Probar500` → acceso denegado.

## Diseño

- [ ] Cambiar a modo oscuro con el botón del menú y recargar la página → el tema se mantiene.
- [ ] Las páginas de Login/Registro/Acceso denegado están en español y con el mismo estilo del resto del sitio.

## Pruebas automatizadas

- [ ] Ejecutar `dotnet test` desde la raíz del proyecto → todas las pruebas de `Pedidos360.Tests` deben pasar.
