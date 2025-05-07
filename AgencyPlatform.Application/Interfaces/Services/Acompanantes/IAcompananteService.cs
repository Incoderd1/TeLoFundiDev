using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.DTOs.Servicio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Acompanantes
{
    public interface IAcompananteService
    {
        // Métodos CRUD básicos
        Task<List<AcompananteDto>> GetAllAsync();
        Task<AcompananteDto> GetByIdAsync(int id);
        Task<AcompananteDto> GetByUsuarioIdAsync(int usuarioId);
        Task<int> CrearAsync(CrearAcompananteDto nuevoAcompanante, int usuarioId, string clientIp);
        Task ActualizarAsync(UpdateAcompananteDto acompananteActualizado, int usuarioId, int rolId, string clientIp);
        Task EliminarAsync(int id, int usuarioId, int rolId);

        // Gestión de fotos
        Task<int> AgregarFotoAsync(int acompananteId, AgregarFotoDto fotoDto, int usuarioId, int rolId);
        Task EliminarFotoAsync(int fotoId, int usuarioId, int rolId);
        Task EstablecerFotoPrincipalAsync(int acompananteId, int fotoId, int usuarioId, int rolId);

        // Gestión de servicios
        Task<int> AgregarServicioAsync(int acompananteId, AgregarServicioDto servicioDto, int usuarioId, int rolId);
        Task ActualizarServicioAsync(int servicioId, ActualizarServicioDto servicioDto, int usuarioId, int rolId);
        Task EliminarServicioAsync(int servicioId, int usuarioId, int rolId);

        // Gestión de categorías
        Task AgregarCategoriaAsync(int acompananteId, int categoriaId, int usuarioId, int rolId);
        Task EliminarCategoriaAsync(int acompananteId, int categoriaId, int usuarioId, int rolId);

        // Búsqueda y filtrado
        Task<List<AcompananteDto>> BuscarAsync(AcompananteFiltroDto filtro);
        Task<List<AcompananteDto>> GetDestacadosAsync();
        Task<List<AcompananteDto>> GetRecientesAsync(int cantidad = 10);
        Task<List<AcompananteDto>> GetPopularesAsync(int cantidad = 10);

        // Verificación
        Task<bool> EstaVerificadoAsync(int acompananteId);
        Task VerificarAcompananteAsync(int acompananteId, int agenciaId, int usuarioId, int rolId);
        Task RevocarVerificacionAsync(int acompananteId, int usuarioId, int rolId);

        // Métricas y estadísticas
        Task RegistrarVisitaAsync(int acompananteId, string ipVisitante, string userAgent, int? clienteId = null);
        Task RegistrarContactoAsync(int acompananteId, string tipoContacto, string ipContacto, int? clienteId = null);
        Task<AcompananteEstadisticasDto> GetEstadisticasAsync(int acompananteId, int usuarioId, int rolId);

        // Disponibilidad
        Task CambiarDisponibilidadAsync(int acompananteId, bool estaDisponible, int usuarioId, int rolId);

        // Métodos paginados
        Task<PaginatedResultDto<AcompananteDto>> GetAllPaginatedAsync(int pageNumber, int pageSize);
        Task<PaginatedResultDto<AcompananteResumen2Dto>> GetAllPaginatedResumenAsync(int pageNumber, int pageSize);
        Task<PaginatedResultDto<AcompananteDto>> GetRecientesPaginadosAsync(int pageNumber, int pageSize);
        Task<PaginatedResultDto<AcompananteDto>> GetPopularesPaginadosAsync(int pageNumber, int pageSize);
        Task<PaginatedResultDto<AcompananteDto>> GetDestacadosPaginadosAsync(int pageNumber, int pageSize);

        Task<PaginatedResultDto<AcompananteDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);


        Task<bool> ActualizarStripeAccountIdAsync(int acompananteId, string stripeAccountId);
        Task<bool> ActualizarEstatusCuentaPagoAsync(int acompananteId, string estado);


    }
}
