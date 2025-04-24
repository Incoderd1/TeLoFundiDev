using AgencyPlatform.Application.DTOs.Foto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Foto
{
    public interface IFotoService
    {

        Task<List<FotoDto>> GetByAcompananteIdAsync(int acompananteId);
        Task<FotoDto> GetByIdAsync(int id);
        Task<FotoDto> SubirFotoAsync(SubirFotoDto dto, int usuarioId);
        Task<FotoDto> ActualizarFotoAsync(ActualizarFotoDto dto, int usuarioId);
        Task<bool> EliminarFotoAsync(int id, int usuarioId);
        Task<FotoDto> EstablecerFotoPrincipalAsync(int fotoId, int acompananteId, int usuarioId);
        Task<string> GuardarArchivoAsync(IFormFile archivo, string carpeta);
    }
}
