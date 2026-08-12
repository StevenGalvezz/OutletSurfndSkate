using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos360.Models
{
    public class PedidoDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

       
        [ForeignKey("PedidoId")]
        public virtual Pedido? Pedido { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual global::Pedidos360.Models.Producto? Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mínimo 1.")]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnit { get; set; }

        [Required]
        public decimal Descuento { get; set; }

        [Required]
        public decimal ImpuestoPorc { get; set; }

        [Required]
        public decimal TotalLinea { get; set; }
    }
}