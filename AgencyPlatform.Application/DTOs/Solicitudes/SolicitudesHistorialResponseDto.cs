using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Solicitudes
{
    public class SolicitudesHistorialResponseDto
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<SolicitudHistorialDto> Items { get; set; } = new List<SolicitudHistorialDto>();
    }
}
