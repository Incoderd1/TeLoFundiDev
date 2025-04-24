using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    [Table("solicitudes_registro_agencia", Schema = "plataforma")]
    public class solicitud_registro_agencia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [StringLength(255)]
        public string nombre { get; set; }

        [Required]
        [StringLength(255)]
        public string email { get; set; }

        [Required]
        [StringLength(255)]
        public string password_hash { get; set; }

        public string descripcion { get; set; }

        [StringLength(512)]
        public string logo_url { get; set; }

        [StringLength(255)]
        public string sitio_web { get; set; }

        public string direccion { get; set; }

        [StringLength(100)]
        public string ciudad { get; set; }

        [StringLength(100)]
        public string pais { get; set; }

        public DateTime fecha_solicitud { get; set; }

        public DateTime? fecha_respuesta { get; set; }

        [StringLength(50)]
        public string estado { get; set; } = "pendiente";

        public string? motivo_rechazo { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }
    }
}
