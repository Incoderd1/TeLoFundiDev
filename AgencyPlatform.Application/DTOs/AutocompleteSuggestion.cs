using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs
{
    public class AutocompleteSuggestion
    {
        /// <summary>
        /// Texto de la sugerencia
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Conteo de elementos asociados a esta sugerencia
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Tipo de sugerencia (Ciudad, Categoría, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// ID opcional para categorías, servicios, etc.
        /// </summary>
        public int? Id { get; set; }
    }
}
