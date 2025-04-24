using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    [Table("movimientos_puntos_agencia", Schema = "plataforma")]
    public class movimientos_puntos_agencia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int agencia_id { get; set; }

        public int cantidad { get; set; }

        [Required]
        [StringLength(50)]
        public string tipo { get; set; }

        [Required]
        [StringLength(255)]
        public string concepto { get; set; }

        public int saldo_anterior { get; set; }

        public int saldo_nuevo { get; set; }

        public DateTime fecha { get; set; }

        public DateTime created_at { get; set; }

        [ForeignKey("agencia_id")]
        public virtual agencia agencia { get; set; }
    }
}
