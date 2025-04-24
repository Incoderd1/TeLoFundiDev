using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    [Table("pagos_verificacion", Schema = "plataforma")]
    public class pago_verificacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int verificacion_id { get; set; }

        public int acompanante_id { get; set; }

        public int agencia_id { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal monto { get; set; }

        [StringLength(10)]
        public string moneda { get; set; } = "USD";

        [StringLength(50)]
        public string metodo_pago { get; set; }

        [StringLength(255)]
        public string referencia_pago { get; set; }

        [Required]
        [StringLength(50)]
        public string estado { get; set; } = "pendiente";

        public DateTime? fecha_pago { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        // Relaciones de navegación
        [ForeignKey("verificacion_id")]
        public virtual verificacione verificacion { get; set; }

        [ForeignKey("acompanante_id")]
        public virtual acompanante acompanante { get; set; }

        [ForeignKey("agencia_id")]
        public virtual agencia agencia { get; set; }
    }
}
