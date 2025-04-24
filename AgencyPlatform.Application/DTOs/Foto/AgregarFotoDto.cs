using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Foto
{
    public class AgregarFotoDto
    {
        [Required]
        public IFormFile Archivo { get; set; }

        public bool EsPrincipal { get; set; }

        public int Orden { get; set; } = 0;
    }
}
