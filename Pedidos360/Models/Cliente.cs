using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La cédula física o jurídica es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder los 20 caracteres.")]
        public string Cedula { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(100)]
        public string Correo { get; set; } = null!;

        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres.")]
        public string? Telefono { get; set; } = null!;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(250, ErrorMessage = "La dirección no puede exceder los 250 caracteres.")]
        public string Direccion { get; set; } = null!;
    }
}
