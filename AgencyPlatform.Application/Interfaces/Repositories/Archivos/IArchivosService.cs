using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories.Archivos
{
    public interface IArchivosService
    {
        Task<string> GuardarArchivoAsync(IFormFile archivo);
        Task<bool> EliminarArchivoAsync(string ruta);
        Task<string> ObtenerUrlCompleta(string rutaRelativa);
    }
}
