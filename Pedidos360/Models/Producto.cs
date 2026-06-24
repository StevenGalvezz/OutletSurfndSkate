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
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El porcentaje de impuesto es requerido.")]
        [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100 %.")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ImpuestoPorc { get; set; }

        [Required(ErrorMessage = "El stock es requerido.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La imagen del producto es obligatoria.")]
        public string ImagenUrl { get; set; } = null!;

        public bool Activo { get; set; } = true;
    }
}
