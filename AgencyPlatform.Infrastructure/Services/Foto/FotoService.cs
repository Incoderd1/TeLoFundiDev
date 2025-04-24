using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services.Foto;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Infrastructure.Services.Storage;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Foto
{
    public class FotoService : IFotoService
    {
        private readonly IFotoRepository _fotoRepository;
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IAgenciaRepository _agenciaRepository;
        private readonly IFileStorageService _fileStorage;
        private readonly IMapper _mapper;
        private readonly ILogger<FotoService> _logger;

        public FotoService(
            IFotoRepository fotoRepository,
            IAcompananteRepository acompananteRepository,
            IAgenciaRepository agenciaRepository,
            IFileStorageService fileStorage,
            IMapper mapper,
            ILogger<FotoService> logger)
        {
            _fotoRepository = fotoRepository;
            _acompananteRepository = acompananteRepository;
            _agenciaRepository = agenciaRepository;
            _fileStorage = fileStorage;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<FotoDto>> GetByAcompananteIdAsync(int acompananteId)
        {
            try
            {
                var fotos = await _fotoRepository.GetByAcompananteIdAsync(acompananteId);
                return _mapper.Map<List<FotoDto>>(fotos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fotos del acompañante {AcompananteId}", acompananteId);
                throw;
            }
        }

        public async Task<FotoDto> GetByIdAsync(int id)
        {
            try
            {
                var foto = await _fotoRepository.GetByIdAsync(id);
                return foto == null ? null : _mapper.Map<FotoDto>(foto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener foto con ID {FotoId}", id);
                throw;
            }
        }

        public async Task<FotoDto> SubirFotoAsync(SubirFotoDto dto, int usuarioId)
        {
            // 1. Validar existencia de acompañante
            var acompanante = await _acompananteRepository.GetByIdAsync(dto.AcompananteId);
            if (acompanante == null)
                throw new InvalidOperationException($"El acompañante con ID {dto.AcompananteId} no existe");

            // 2. Validar permisos
            if (acompanante.usuario_id != usuarioId && acompanante.agencia?.usuario_id != usuarioId)
                throw new UnauthorizedAccessException("No tienes permisos para subir fotos a este perfil");

            // 3. Guardar archivo en storage
            var urlArchivo = await GuardarArchivoAsync(dto.Foto, $"acompanantes/{dto.AcompananteId}");

            // 4. Si es foto principal, resetear las demás
            if (dto.EsPrincipal)
                await _fotoRepository.QuitarFotosPrincipalesAsync(dto.AcompananteId);

            // 5. Calcular orden si no viene en el DTO
            int orden = dto.Orden;
            if (orden == 0)
            {
                var maxOrden = await _fotoRepository.ContarFotosPorAcompananteAsync(dto.AcompananteId);
                orden = maxOrden + 1;
            }

            // 6. Crear entidad con verificación automática
            var nuevaFoto = new foto
            {
                acompanante_id = dto.AcompananteId,
                url = urlArchivo,
                es_principal = dto.EsPrincipal,
                orden = orden,

                verificada = true,                // ← marcada como verificada por defecto
                fecha_verificacion = DateTime.UtcNow,      // ← fecha de subida

                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // 7. Persistir en la base de datos
            await _fotoRepository.AddAsync(nuevaFoto);
            await _fotoRepository.SaveChangesAsync();

            // 8. Recuperar con todo el mapeo y devolver DTO
            var fotoCompleta = await _fotoRepository.GetByIdAsync(nuevaFoto.id);
            return _mapper.Map<FotoDto>(fotoCompleta);
        }

        public async Task<FotoDto> ActualizarFotoAsync(ActualizarFotoDto dto, int usuarioId)
        {
            try
            {
                var foto = await _fotoRepository.GetByIdAsync(dto.Id);
                if (foto == null)
                    throw new InvalidOperationException($"La foto con ID {dto.Id} no existe");

                var acompanante = await _acompananteRepository.GetByIdAsync(foto.acompanante_id);
                if (acompanante == null)
                    throw new InvalidOperationException("El acompañante asociado a esta foto no existe");

                if (acompanante.usuario_id != usuarioId && acompanante.agencia?.usuario_id != usuarioId)
                    throw new UnauthorizedAccessException("No tienes permisos para modificar esta foto");

                if (dto.EsPrincipal.HasValue && dto.EsPrincipal.Value && !foto.es_principal.GetValueOrDefault())
                    await _fotoRepository.QuitarFotosPrincipalesAsync(foto.acompanante_id);

                if (dto.EsPrincipal.HasValue) foto.es_principal = dto.EsPrincipal;
                if (dto.Orden.HasValue) foto.orden = dto.Orden;
                foto.updated_at = DateTime.UtcNow;

                await _fotoRepository.UpdateAsync(foto);
                await _fotoRepository.SaveChangesAsync();

                return _mapper.Map<FotoDto>(foto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar foto con ID {FotoId}", dto.Id);
                throw;
            }
        }

        public async Task<bool> EliminarFotoAsync(int id, int usuarioId)
        {
            try
            {
                var foto = await _fotoRepository.GetByIdAsync(id);
                if (foto == null) return false;

                var acompanante = await _acompananteRepository.GetByIdAsync(foto.acompanante_id);
                if (acompanante == null)
                    throw new InvalidOperationException("El acompañante asociado a esta foto no existe");

                if (acompanante.usuario_id != usuarioId && acompanante.agencia?.usuario_id != usuarioId)
                    throw new UnauthorizedAccessException("No tienes permisos para eliminar esta foto");

                if (!string.IsNullOrEmpty(foto.url))
                    _fileStorage.DeleteFile(foto.url);

                await _fotoRepository.DeleteAsync(foto);
                await _fotoRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar foto con ID {FotoId}", id);
                throw;
            }
        }

        public async Task<FotoDto> EstablecerFotoPrincipalAsync(int fotoId, int acompananteId, int usuarioId)
        {
            try
            {
                var foto = await _fotoRepository.GetByIdAsync(fotoId);
                if (foto == null)
                    throw new InvalidOperationException($"La foto con ID {fotoId} no existe");

                if (foto.acompanante_id != acompananteId)
                    throw new InvalidOperationException("La foto no pertenece al acompañante especificado");

                var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
                if (acompanante == null)
                    throw new InvalidOperationException($"El acompañante con ID {acompananteId} no existe");

                if (acompanante.usuario_id != usuarioId && acompanante.agencia?.usuario_id != usuarioId)
                    throw new UnauthorizedAccessException("No tienes permisos para modificar las fotos de este perfil");

                await _fotoRepository.QuitarFotosPrincipalesAsync(acompananteId);
                foto.es_principal = true;
                foto.updated_at = DateTime.UtcNow;

                await _fotoRepository.UpdateAsync(foto);
                await _fotoRepository.SaveChangesAsync();

                return _mapper.Map<FotoDto>(foto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer foto principal {FotoId} para acompañante {AcompananteId}", fotoId, acompananteId);
                throw;
            }
        }

        public async Task<string> GuardarArchivoAsync(IFormFile archivo, string carpeta)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    throw new ArgumentException("El archivo es inválido o está vacío");

                return await _fileStorage.SaveFileAsync(archivo, carpeta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo en carpeta {Carpeta}", carpeta);
                throw;
            }
        }
    }
}
