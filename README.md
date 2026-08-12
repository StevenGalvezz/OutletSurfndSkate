# Pedidos360 — Outlet Surf&Skate

Sistema de pedidos para una tienda de ropa urbana/streetwear. Es una app ASP.NET Core MVC con Identity (login y roles), Entity Framework Core sobre SQLite, y un par de pantallas con AJAX (búsqueda de productos sin recargar la página). Incluye un proyecto de pruebas automatizadas con xUnit.

Dos proyectos en la solución:

- `Pedidos360/` — la aplicación web (MVC + Identity + EF Core).
- `Pedidos360.Tests/` — pruebas unitarias (xUnit) sobre la lógica de cálculo de pedidos y las validaciones de los modelos.

## Requisitos

- **.NET SDK 10.0** o más nuevo. Se puede confirmar con `dotnet --version`; si no aparece un `10.x`, bajarlo de https://dotnet.microsoft.com/download/dotnet/10.0 (instala el SDK, no solo el runtime).
- No hace falta instalar SQL Server, Docker ni nada más. La base de datos es un archivo SQLite que se crea solo la primera vez que se corre la app.

## Cómo compilar y correr

Desde la raíz del repo:

```bash
dotnet restore
dotnet build
```

Si esos dos comandos terminan en "Compilación correcta" con 0 errores, el proyecto compila. Para levantar la app:

```bash
dotnet run --project Pedidos360 --launch-profile http
```

(el `--launch-profile http` es para evitar el aviso del certificado HTTPS de desarrollo, que no viene instalado por defecto en una máquina nueva; si prefieren HTTPS, antes hay que correr una vez `dotnet dev-certs https --trust`).

La consola va a mostrar algo como `Now listening on: http://localhost:5079` — esa es la URL. La primera vez que arranca tarda un poco más de lo normal porque, además de compilar, aplica las migraciones de Entity Framework y siembra datos de prueba (`Pedidos360/Data/SeedData.cs`): categorías, un par de productos, clientes y dos usuarios ya listos para entrar:

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | `admin@outletsurfskate.com` | `Admin#2026` |
| Cliente | `cliente@outletsurfskate.com` | `Cliente#2026` |

No hay que correr `dotnet ef database update` a mano ni nada parecido — `Program.cs` llama `context.Database.Migrate()` al arrancar, así que la base y las tablas se crean solas.

## Cómo correr las pruebas

```bash
dotnet test
```

Deberían pasar las 13 pruebas de `Pedidos360.Tests` (cálculo de totales/impuestos de un pedido y validaciones de los modelos de Cliente y detalle de pedido).

## Abrir en Visual Studio

También se puede abrir directo `Pedidos360.slnx` en Visual Studio 2022 (17.13 o más nuevo, con soporte para `net10.0`), restaurar NuGet y compilar la solución (Ctrl+Shift+B) o correr con F5. El `.slnx` ya tiene los dos proyectos configurados para compilar juntos en Debug.

## Correo de factura (opcional, no bloquea nada)

Al confirmar una compra desde el carrito se intenta mandar la factura por correo (`Pedidos360/Services/SmtpEmailSender.cs`). Sin credenciales SMTP configuradas, la app no falla: solo deja un warning en el log y sigue funcionando normal — el pedido igual se crea. Si se quiere probar el envío real, hay que definir `Smtp:User` y `Smtp:Password` (ver `.env.example`), por ejemplo con `dotnet user-secrets`.

## Deploy en producción

La guía para actualizar/desplegar la instancia en Oracle Cloud (Docker + Nginx) está en [`DEPLOY.md`](DEPLOY.md). No hace falta para evaluar el proyecto localmente.
