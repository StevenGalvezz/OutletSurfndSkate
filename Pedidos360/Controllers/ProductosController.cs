using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Models;

// El catálogo administrativo (con precios de costo, stock, etc.) es
// solo para el administrador; el cliente compra desde la Tienda.
[Authorize(Roles = "Administrador")]
public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PRODUCTOS
    public async Task<IActionResult> Index()
    {
        var productosConCategoria = await _context.Productos.Include(p => p.Categoria).ToListAsync();
        return View(productosConCategoria);
    }

    // GET: PRODUCTOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (producto == null)
        {
            return NotFound();
        }

        return View(producto);
    }

    // GET: PRODUCTOS/Imagen/5 — sirve la foto guardada en la base como
    // archivo binario. [AllowAnonymous] porque la Tienda y el Carrito
    // (que no piden login para navegar) también la muestran; si el
    // producto no tiene foto cargada, el 404 hace que el <img> de la vista
    // caiga solo al ícono de "sin imagen" (ver _ImagenProducto.cshtml).
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Imagen(int id)
    {
        var producto = await _context.Productos
            .Select(p => new { p.Id, p.ImagenData, p.ImagenContentType })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto?.ImagenData == null || producto.ImagenData.Length == 0)
        {
            return NotFound();
        }

        return File(producto.ImagenData, producto.ImagenContentType ?? "application/octet-stream");
    }

    // GET: PRODUCTOS/Create
    public IActionResult Create()
    {
        ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre");
        return View();
    }

    // POST: PRODUCTOS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nombre,CategoriaId,Precio,ImpuestoPorc,Stock,Activo")] Producto producto, IFormFile? imagen)
    {
        if (imagen is not { Length: > 0 })
        {
            ModelState.AddModelError(string.Empty, "Debe subir una foto del producto.");
        }
        else if (!EsImagenValida(imagen, out var errorImagen))
        {
            ModelState.AddModelError(string.Empty, errorImagen);
        }

        if (ModelState.IsValid)
        {
            (producto.ImagenData, producto.ImagenContentType) = await LeerImagenAsync(imagen!);

            _context.Add(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
        return View(producto);
    }

    // GET: PRODUCTOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
        return View(producto);
    }

    // POST: PRODUCTOS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nombre,CategoriaId,Precio,ImpuestoPorc,Stock,Activo")] Producto producto, IFormFile? imagen)
    {
        if (id != producto.Id)
        {
            return NotFound();
        }

        if (imagen is { Length: > 0 } && !EsImagenValida(imagen, out var errorImagen))
        {
            ModelState.AddModelError(string.Empty, errorImagen);
        }

        if (ModelState.IsValid)
        {
            // Se trae la fila actual de la base y se le pisan solo los campos
            // del formulario. Usar _context.Update(producto) acá marcaría
            // ImagenData/ImagenContentType como modificados con sus valores
            // por defecto (null), porque el model binder nunca los llena
            // -- y borraría la foto cada vez que se edita el producto sin
            // subir una nueva.
            var productoDb = await _context.Productos.FindAsync(id);
            if (productoDb == null)
            {
                return NotFound();
            }

            productoDb.Nombre = producto.Nombre;
            productoDb.CategoriaId = producto.CategoriaId;
            productoDb.Precio = producto.Precio;
            productoDb.ImpuestoPorc = producto.ImpuestoPorc;
            productoDb.Stock = producto.Stock;
            productoDb.Activo = producto.Activo;

            if (imagen is { Length: > 0 })
            {
                (productoDb.ImagenData, productoDb.ImagenContentType) = await LeerImagenAsync(imagen);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
        return View(producto);
    }

    // GET: PRODUCTOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (producto == null)
        {
            return NotFound();
        }

        return View(producto);
    }

    // POST: PRODUCTOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductoExists(int? id)
    {
        return _context.Productos.Any(e => e.Id == id);
    }

    private static readonly string[] TiposDeImagenPermitidos = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private const long TamanoMaximoImagenBytes = 3 * 1024 * 1024; // 3 MB alcanza de sobra para la foto de un producto

    private static bool EsImagenValida(IFormFile imagen, out string error)
    {
        if (!TiposDeImagenPermitidos.Contains(imagen.ContentType))
        {
            error = "La foto debe ser JPG, PNG, WEBP o GIF.";
            return false;
        }

        if (imagen.Length > TamanoMaximoImagenBytes)
        {
            error = "La foto no puede pesar más de 3 MB.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static async Task<(byte[] Data, string ContentType)> LeerImagenAsync(IFormFile imagen)
    {
        using var stream = new MemoryStream();
        await imagen.CopyToAsync(stream);
        return (stream.ToArray(), imagen.ContentType);
    }
}
