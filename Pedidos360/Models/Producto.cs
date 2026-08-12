using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos360.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La categoría es requerida.")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [Required(ErrorMessage = "El precio es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El porcentaje de impuesto es requerido.")]
        [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100 %.")]
        public decimal ImpuestoPorc { get; set; }

        [Required(ErrorMessage = "El stock es requerido.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        // La foto se guarda directo en la base de datos (BLOB), no como URL
        // externa: se sube un archivo desde ProductosController y se sirve de
        // vuelta con GET /Productos/Imagen/{id}. No lleva [Required] porque
        // Create la exige a mano (necesita validar el archivo, no solo el
        // campo) y Edit la deja opcional para no obligar a resubirla.
        public byte[]? ImagenData { get; set; }
        public string? ImagenContentType { get; set; }

        public bool Activo { get; set; } = true;
    }
}
