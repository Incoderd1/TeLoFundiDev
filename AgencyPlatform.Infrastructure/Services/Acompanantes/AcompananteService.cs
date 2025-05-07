using AgencyPlatform.Application;
using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.DTOs.Servicio;
using AgencyPlatform.Application.DTOs.Visitas;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Repositories.Archivos;
using AgencyPlatform.Application.Interfaces.Services.Acompanantes;
using AgencyPlatform.Application.Interfaces.Services.Geocalizacion;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AgencyPlatform.Infrastructure.Services.Acompanantes
{
    public class AcompananteService : IAcompananteService
    {
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IFotoRepository _fotoRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IArchivosService _archivosService;
        private readonly IVisitaPerfilRepository _visitaPerfilRepository;
        private readonly IContactoRepository _contactoRepository;
        private readonly IMapper _mapper;
        private readonly IAgenciaRepository _agenciaRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AcompananteService> _logger;
        private readonly IGeocodingService _geocodingService;

        private const string CACHE_KEY_POPULARES = "AcompanantesPopulares";
        private const string CACHE_KEY_DESTACADOS = "AcompanantesDestacados";
        private const string CACHE_KEY_RECIENTES = "AcompanantesRecientes";
        private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(15);

        public AcompananteService(
            IAcompananteRepository acompananteRepository,
            IFotoRepository fotoRepository,
            IServicioRepository servicioRepository,
            IUsuarioRepository usuarioRepository,
            IArchivosService archivosService,
            IVisitaPerfilRepository visitaPerfilRepository,
            IContactoRepository contactoRepository,
            IMapper mapper,
            IAgenciaRepository agenciaRepository,
            IMemoryCache cache,
            ILogger<AcompananteService> logger,
            IGeocodingService geocodingService)
        {
            _acompananteRepository = acompananteRepository;
            _fotoRepository = fotoRepository;
            _servicioRepository = servicioRepository;
            _usuarioRepository = usuarioRepository;
            _archivosService = archivosService;
            _visitaPerfilRepository = visitaPerfilRepository;
            _contactoRepository = contactoRepository;
            _mapper = mapper;
            _agenciaRepository = agenciaRepository;
            _cache = cache;
            _logger = logger;
            _geocodingService = geocodingService;
        }

        public async Task<List<AcompananteDto>> GetAllAsync()
        {
            _logger.LogInformation("Obteniendo todos los acompañantes.");
            var acompanantes = await _acompananteRepository.GetAllAsync();
            _logger.LogInformation("Se obtuvieron {Count} acompañantes.", acompanantes.Count);
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task<AcompananteDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo acompañante con ID {AcompananteId}.", id);
            var acompanante = await _acompananteRepository.GetByIdAsync(id);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante con ID {AcompananteId} no encontrado.", id);
                return null;
            }
            _logger.LogInformation("Acompañante con ID {AcompananteId} obtenido correctamente.", id);
            return _mapper.Map<AcompananteDto>(acompanante);
        }
       
        public async Task<AcompananteDto> GetByUsuarioIdAsync(int usuarioId)
        {
            _logger.LogInformation("Obteniendo acompañante para usuario {UsuarioId}.", usuarioId);
            var acompanante = await _acompananteRepository.GetByUsuarioIdAsync(usuarioId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante para usuario {UsuarioId} no encontrado.", usuarioId);
                return null;
            }
            _logger.LogInformation("Acompañante para usuario {UsuarioId} obtenido correctamente.", usuarioId);
            return _mapper.Map<AcompananteDto>(acompanante);
        }

        public async Task<int> CrearAsync(CrearAcompananteDto nuevoAcompanante, int usuarioId, string clientIp)
        {
            _logger.LogInformation("Creando/Actualizando acompañante para usuario {UsuarioId} con IP {ClientIp}.", usuarioId, clientIp);

            // Validaciones
            if (nuevoAcompanante.Edad < 18)
            {
                _logger.LogWarning("Intento de crear acompañante con edad inválida: {Edad}.", nuevoAcompanante.Edad);
                throw new ArgumentException("La edad debe ser mayor o igual a 18.");
            }

            // Verificar que el usuario exista
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario {UsuarioId} no encontrado.", usuarioId);
                throw new InvalidOperationException("El usuario no existe");
            }

            // Obtener ubicación automáticamente basada en la IP del cliente
            double? latitud = null;
            double? longitud = null;
            string ciudad = "Desconocido";
            string pais = "Desconocido";
            string direccionCompleta = "Desconocido";

            if (!string.IsNullOrWhiteSpace(clientIp) && clientIp != "Unknown")
            {
                _logger.LogInformation("Obteniendo ubicación para IP {ClientIp} usando ip-api.com.", clientIp);
                var location = await _geocodingService.GetLocationFromIpAsync(clientIp);
                if (location.HasValue)
                {
                    latitud = location.Value.latitude;
                    longitud = location.Value.longitude;
                    ciudad = location.Value.city ?? "Desconocido";
                    pais = location.Value.country ?? "Desconocido";
                    direccionCompleta = string.Join(", ", new[] { ciudad, pais }.Where(s => !string.IsNullOrWhiteSpace(s) && s != "Desconocido"));
                    _logger.LogInformation("Ubicación obtenida exitosamente para IP {ClientIp}: Ciudad={Ciudad}, País={Pais}, Latitud={Latitud}, Longitud={Longitud}.", clientIp, ciudad, pais, latitud, longitud);
                }
                else
                {
                    _logger.LogWarning("No se pudo obtener la ubicación para IP {ClientIp} desde ip-api.com.", clientIp);
                }
            }
            else
            {
                _logger.LogWarning("IP del cliente no válida: {ClientIp}. No se puede obtener la ubicación.", clientIp);
            }

            // IMPORTANTE: Consultar el acompañante existente una sola vez y antes de iniciar operaciones
            var existingAcompanante = await _acompananteRepository.GetByUsuarioIdAsync(usuarioId);
            int acompananteId;

            if (existingAcompanante != null)
            {
                _logger.LogInformation("Acompañante existente encontrado para usuario {UsuarioId}. Actualizando perfil.", usuarioId);

                // Actualizar el perfil existente
                existingAcompanante.agencia_id = nuevoAcompanante.AgenciaId;
                existingAcompanante.nombre_perfil = nuevoAcompanante.NombrePerfil;
                existingAcompanante.genero = nuevoAcompanante.Genero;
                existingAcompanante.edad = nuevoAcompanante.Edad;
                existingAcompanante.descripcion = nuevoAcompanante.Descripcion;
                existingAcompanante.altura = nuevoAcompanante.Altura;
                existingAcompanante.peso = nuevoAcompanante.Peso;
                existingAcompanante.ciudad = ciudad;
                existingAcompanante.pais = pais;
                existingAcompanante.idiomas = nuevoAcompanante.Idiomas;
                existingAcompanante.disponibilidad = nuevoAcompanante.Disponibilidad;
                existingAcompanante.tarifa_base = nuevoAcompanante.TarifaBase;
                existingAcompanante.moneda = nuevoAcompanante.Moneda ?? "USD";
                existingAcompanante.telefono = nuevoAcompanante.Telefono;
                existingAcompanante.whatsapp = nuevoAcompanante.WhatsApp;
                existingAcompanante.email_contacto = nuevoAcompanante.EmailContacto;
                existingAcompanante.mostrar_telefono = true;
                existingAcompanante.mostrar_whatsapp = true;
                existingAcompanante.mostrar_email = true;
                existingAcompanante.latitud = latitud;
                existingAcompanante.longitud = longitud;
                existingAcompanante.direccion_completa = direccionCompleta;
                existingAcompanante.updated_at = DateTime.UtcNow;
                existingAcompanante.esta_disponible = true;

                // Inicializar campos Stripe si son nulos para evitar el error
                if (existingAcompanante.stripe_account_id == null)
                    existingAcompanante.stripe_account_id = "";

                await _acompananteRepository.UpdateAsync(existingAcompanante);

                acompananteId = existingAcompanante.id;

                // Agregar categorías si se proporcionaron
                if (nuevoAcompanante.CategoriaIds != null && nuevoAcompanante.CategoriaIds.Any())
                {
                    _logger.LogInformation("Agregando {Count} categorías al acompañante {AcompananteId}.", nuevoAcompanante.CategoriaIds.Count, existingAcompanante.id);
                    foreach (var categoriaId in nuevoAcompanante.CategoriaIds)
                    {
                        if (!await _acompananteRepository.TieneCategoriaAsync(existingAcompanante.id, categoriaId))
                        {
                            await _acompananteRepository.AgregarCategoriaSinGuardarAsync(existingAcompanante.id, categoriaId);
                            _logger.LogDebug("Categoría {CategoriaId} agregada al acompañante {AcompananteId}.", categoriaId, existingAcompanante.id);
                        }
                    }
                }
            }
            else
            {
                _logger.LogInformation("Creando nuevo acompañante para usuario {UsuarioId}.", usuarioId);

                // Crear el nuevo acompañante
                var acompanante = new acompanante
                {
                    usuario_id = usuarioId,
                    agencia_id = nuevoAcompanante.AgenciaId,
                    nombre_perfil = nuevoAcompanante.NombrePerfil,
                    genero = nuevoAcompanante.Genero,
                    edad = nuevoAcompanante.Edad,
                    descripcion = nuevoAcompanante.Descripcion,
                    altura = nuevoAcompanante.Altura,
                    peso = nuevoAcompanante.Peso,
                    ciudad = ciudad,
                    pais = pais,
                    idiomas = nuevoAcompanante.Idiomas,
                    disponibilidad = nuevoAcompanante.Disponibilidad,
                    tarifa_base = nuevoAcompanante.TarifaBase,
                    moneda = nuevoAcompanante.Moneda ?? "USD",
                    telefono = nuevoAcompanante.Telefono,
                    whatsapp = nuevoAcompanante.WhatsApp,
                    email_contacto = nuevoAcompanante.EmailContacto,
                    mostrar_telefono = true,
                    mostrar_whatsapp = true,
                    mostrar_email = true,
                    latitud = latitud,
                    longitud = longitud,
                    direccion_completa = direccionCompleta,
                    esta_verificado = false,
                    esta_disponible = true,
                    created_at = DateTime.UtcNow,
                    score_actividad = 0,
                    // Inicializar campos de Stripe con valores por defecto
                    stripe_account_id = "",
                    stripe_payouts_enabled = false,
                    stripe_charges_enabled = false,
                    stripe_onboarding_completed = false
                };

                await _acompananteRepository.AddAsync(acompanante);
                // Necesitamos guardar aquí para obtener el ID
                await _acompananteRepository.SaveChangesAsync();

                acompananteId = acompanante.id;

                // Agregar categorías si se proporcionaron
                if (nuevoAcompanante.CategoriaIds != null && nuevoAcompanante.CategoriaIds.Any())
                {
                    _logger.LogInformation("Agregando {Count} categorías al nuevo acompañante {AcompananteId}.", nuevoAcompanante.CategoriaIds.Count, acompananteId);
                    foreach (var categoriaId in nuevoAcompanante.CategoriaIds)
                    {
                        await _acompananteRepository.AgregarCategoriaSinGuardarAsync(acompananteId, categoriaId);
                        _logger.LogDebug("Categoría {CategoriaId} agregada al acompañante {AcompananteId}.", categoriaId, acompananteId);
                    }
                }
            }

            // Guardar todos los cambios pendientes de una sola vez
            await _acompananteRepository.SaveChangesAsync();

            _logger.LogInformation("Acompañante creado/actualizado para usuario {UsuarioId}. AcompañanteId: {AcompananteId}.", usuarioId, acompananteId);
            return acompananteId;
        }
        public async Task ActualizarAsync(UpdateAcompananteDto acompananteActualizado, int usuarioId, int rolId, string clientIp)
        {
            _logger.LogInformation("Actualizando acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId} y IP {ClientIp}.", acompananteActualizado.Id, usuarioId, rolId, clientIp);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteActualizado.Id);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteActualizado.Id);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para actualizar el acompañante {AcompananteId}.", usuarioId, acompananteActualizado.Id);
                throw new UnauthorizedAccessException("No tienes permisos para actualizar este perfil");
            }

            // Validaciones
            if (acompananteActualizado.Edad.HasValue && acompananteActualizado.Edad < 18)
            {
                _logger.LogWarning("Intento de actualizar acompañante {AcompananteId} con edad inválida: {Edad}.", acompananteActualizado.Id, acompananteActualizado.Edad);
                throw new ArgumentException("La edad debe ser mayor o igual a 18.");
            }

            // Actualizar propiedades
            if (!string.IsNullOrEmpty(acompananteActualizado.NombrePerfil))
                acompanante.nombre_perfil = acompananteActualizado.NombrePerfil;
            if (!string.IsNullOrEmpty(acompananteActualizado.Genero))
                acompanante.genero = acompananteActualizado.Genero;
            if (acompananteActualizado.Edad.HasValue)
                acompanante.edad = acompananteActualizado.Edad;
            if (acompananteActualizado.Descripcion != null)
                acompanante.descripcion = acompananteActualizado.Descripcion;
            if (acompananteActualizado.Altura.HasValue)
                acompanante.altura = acompananteActualizado.Altura;
            if (acompananteActualizado.Peso.HasValue)
                acompanante.peso = acompananteActualizado.Peso;
            if (!string.IsNullOrEmpty(acompananteActualizado.Idiomas))
                acompanante.idiomas = acompananteActualizado.Idiomas;
            if (!string.IsNullOrEmpty(acompananteActualizado.Disponibilidad))
                acompanante.disponibilidad = acompananteActualizado.Disponibilidad;
            if (acompananteActualizado.TarifaBase.HasValue)
                acompanante.tarifa_base = acompananteActualizado.TarifaBase;
            if (!string.IsNullOrEmpty(acompananteActualizado.Moneda))
                acompanante.moneda = acompananteActualizado.Moneda;
            if (acompananteActualizado.EstaDisponible.HasValue)
                acompanante.esta_disponible = acompananteActualizado.EstaDisponible;

            // Solo administradores y agencias pueden cambiar la agencia
            if (acompananteActualizado.AgenciaId.HasValue && (rolId == 1 || rolId == 2))
            {
                _logger.LogInformation("Cambiando agencia del acompañante {AcompananteId} a {AgenciaId}.", acompananteActualizado.Id, acompananteActualizado.AgenciaId);
                acompanante.agencia_id = acompananteActualizado.AgenciaId;
                // Al cambiar de agencia, se pierde la verificación
                if (acompanante.agencia_id != acompananteActualizado.AgenciaId)
                {
                    acompanante.esta_verificado = false;
                    acompanante.fecha_verificacion = null;
                    _logger.LogInformation("Verificación revocada para el acompañante {AcompananteId} debido a cambio de agencia.", acompananteActualizado.Id);
                }
            }

            // Actualizar ubicación automáticamente basada en la IP del cliente
            double? latitud = null;
            double? longitud = null;
            string ciudad = "Desconocido";
            string pais = "Desconocido";
            string direccionCompleta = "Desconocido";

            if (!string.IsNullOrWhiteSpace(clientIp) && clientIp != "Unknown")
            {
                _logger.LogInformation("Obteniendo ubicación para IP {ClientIp} usando ip-api.com.", clientIp);
                var location = await _geocodingService.GetLocationFromIpAsync(clientIp);
                if (location.HasValue)
                {
                    latitud = location.Value.latitude;
                    longitud = location.Value.longitude;
                    ciudad = location.Value.city ?? "Desconocido";
                    pais = location.Value.country ?? "Desconocido";
                    direccionCompleta = string.Join(", ", new[] { ciudad, pais }.Where(s => !string.IsNullOrWhiteSpace(s) && s != "Desconocido"));
                    _logger.LogInformation("Ubicación obtenida exitosamente para IP {ClientIp}: Ciudad={Ciudad}, País={Pais}, Latitud={Latitud}, Longitud={Longitud}.", clientIp, ciudad, pais, latitud, longitud);
                }
                else
                {
                    _logger.LogWarning("No se pudo obtener la ubicación para IP {ClientIp} desde ip-api.com.", clientIp);
                }
            }
            else
            {
                _logger.LogWarning("IP del cliente no válida: {ClientIp}. No se puede obtener la ubicación.", clientIp);
            }

            // Siempre actualizar la ubicación
            acompanante.latitud = latitud;
            acompanante.longitud = longitud;
            acompanante.ciudad = ciudad;
            acompanante.pais = pais;
            acompanante.direccion_completa = direccionCompleta;

            acompanante.updated_at = DateTime.UtcNow;
            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
            _logger.LogInformation("Acompañante {AcompananteId} actualizado correctamente.", acompananteActualizado.Id);
        }

        public async Task EliminarAsync(int id, int usuarioId, int rolId)
        {
            _logger.LogInformation("Eliminando acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", id, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(id);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", id);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para eliminar el acompañante {AcompananteId}.", usuarioId, id);
                throw new UnauthorizedAccessException("No tienes permisos para eliminar este perfil");
            }

            // Eliminar fotos asociadas
            _logger.LogInformation("Eliminando fotos asociadas al acompañante {AcompananteId}.", id);
            var fotos = await _fotoRepository.GetByAcompananteIdAsync(id);
            foreach (var foto in fotos)
            {
                await _archivosService.EliminarArchivoAsync(foto.url);
                await _fotoRepository.DeleteAsync(foto);
                _logger.LogDebug("Foto {FotoId} eliminada para acompañante {AcompananteId}.", foto.id, id);
            }

            // Eliminar servicios asociados
            _logger.LogInformation("Eliminando servicios asociados al acompañante {AcompananteId}.", id);
            var servicios = await _servicioRepository.GetByAcompananteIdAsync(id);
            foreach (var servicio in servicios)
            {
                await _servicioRepository.DeleteAsync(servicio);
                _logger.LogDebug("Servicio {ServicioId} eliminado para acompañante {AcompananteId}.", servicio.id, id);
            }

            // Eliminar categorías asociadas
            _logger.LogInformation("Eliminando categorías asociadas al acompañante {AcompananteId}.", id);
            var categorias = await _acompananteRepository.GetCategoriasByAcompananteIdAsync(id);
            foreach (var categoria in categorias)
            {
                await _acompananteRepository.EliminarCategoriaAsync(id, categoria.categoria_id);
                _logger.LogDebug("Categoría {CategoriaId} eliminada para acompañante {AcompananteId}.", categoria.categoria_id, id);
            }

            // Finalmente eliminar el acompañante
            await _acompananteRepository.DeleteAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
            _logger.LogInformation("Acompañante {AcompananteId} eliminado correctamente.", id);
        }

        public async Task<int> AgregarFotoAsync(int acompananteId, AgregarFotoDto fotoDto, int usuarioId, int rolId)
        {
            _logger.LogInformation("Agregando foto para acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", acompananteId, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para agregar foto al acompañante {AcompananteId}.", usuarioId, acompananteId);
                throw new UnauthorizedAccessException("No tienes permisos para agregar fotos a este perfil");
            }

            // Guardar el archivo
            var url = await _archivosService.GuardarArchivoAsync(fotoDto.Archivo);
            _logger.LogDebug("Archivo subido con URL: {Url}.", url);

            // Si es la primera foto o se marca como principal
            bool esPrincipal = fotoDto.EsPrincipal;
            if (!esPrincipal)
            {
                var fotos = await _fotoRepository.GetByAcompananteIdAsync(acompananteId);
                if (fotos.Count == 0)
                {
                    esPrincipal = true;
                    _logger.LogInformation("Primera foto agregada para acompañante {AcompananteId}. Marcada como principal.", acompananteId);
                }
            }

            // Si es principal, quitar el estado principal de las demás fotos
            if (esPrincipal)
            {
                await _fotoRepository.QuitarFotosPrincipalesAsync(acompananteId);
                _logger.LogDebug("Estado principal quitado de las otras fotos del acompañante {AcompananteId}.", acompananteId);
            }

            // Crear la foto en la base de datos
            var foto = new foto
            {
                acompanante_id = acompananteId,
                url = url,
                es_principal = esPrincipal,
                orden = fotoDto.Orden,
                created_at = DateTime.UtcNow
            };

            await _fotoRepository.AddAsync(foto);
            await _fotoRepository.SaveChangesAsync();
            _logger.LogInformation("Foto {FotoId} agregada para acompañante {AcompananteId}. Principal: {EsPrincipal}.", foto.id, acompananteId, esPrincipal);

            return foto.id;
        }

        public async Task EliminarFotoAsync(int fotoId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Eliminando foto {FotoId} por usuario {UsuarioId} con rol {RolId}.", fotoId, usuarioId, rolId);

            var foto = await _fotoRepository.GetByIdAsync(fotoId);
            if (foto == null)
            {
                _logger.LogWarning("Foto {FotoId} no encontrada.", fotoId);
                throw new InvalidOperationException("Foto no encontrada");
            }

            var acompanante = await _acompananteRepository.GetByIdAsync(foto.acompanante_id);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado para foto {FotoId}.", foto.acompanante_id, fotoId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para eliminar foto {FotoId} del acompañante {AcompananteId}.", usuarioId, fotoId, foto.acompanante_id);
                throw new UnauthorizedAccessException("No tienes permisos para eliminar fotos de este perfil");
            }

            // Si es la foto principal, no permitir eliminar si es la única
            if (acompanante.esta_verificado == true)
            {
                var fotos = await _fotoRepository.GetByAcompananteIdAsync(foto.acompanante_id);
                if (fotos.Count <= 1)
                {
                    _logger.LogWarning("No se puede eliminar la única foto del perfil verificado {AcompananteId}.", foto.acompanante_id);
                    throw new InvalidOperationException("No se puede eliminar la única foto del perfil");
                }
            }

            // Eliminar archivo físico
            await _archivosService.EliminarArchivoAsync(foto.url);
            _logger.LogDebug("Archivo físico eliminado para foto {FotoId}.", fotoId);

            // Eliminar de la base de datos
            await _fotoRepository.DeleteAsync(foto);
            await _fotoRepository.SaveChangesAsync();
            _logger.LogInformation("Foto {FotoId} eliminada para acompañante {AcompananteId}.", fotoId, foto.acompanante_id);

            // Si era la principal, establecer otra como principal
            if (acompanante.esta_verificado == true)
            {
                var nuevasPrincipales = await _fotoRepository.GetByAcompananteIdAsync(foto.acompanante_id);
                if (nuevasPrincipales.Any())
                {
                    var primera = nuevasPrincipales.First();
                    primera.es_principal = true;
                    await _fotoRepository.UpdateAsync(primera);
                    await _fotoRepository.SaveChangesAsync();
                    _logger.LogInformation("Nueva foto principal {NuevaFotoId} establecida para acompañante {AcompananteId}.", primera.id, foto.acompanante_id);
                }
            }
        }

        public async Task EstablecerFotoPrincipalAsync(int acompananteId, int fotoId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Estableciendo foto {FotoId} como principal para acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", fotoId, acompananteId, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para modificar el perfil de acompañante {AcompananteId}.", usuarioId, acompananteId);
                throw new UnauthorizedAccessException("No tienes permisos para modificar este perfil");
            }

            var foto = await _fotoRepository.GetByIdAsync(fotoId);
            if (foto == null || foto.acompanante_id != acompananteId)
            {
                _logger.LogWarning("Foto {FotoId} no encontrada o no pertenece al acompañante {AcompananteId}.", fotoId, acompananteId);
                throw new InvalidOperationException("Foto no encontrada o no pertenece al acompañante");
            }

            // Quitar el estado principal de las demás fotos
            await _fotoRepository.QuitarFotosPrincipalesAsync(acompananteId);
            _logger.LogDebug("Estado principal quitado de las otras fotos del acompañante {AcompananteId}.", acompananteId);

            // Establecer esta foto como principal
            foto.es_principal = true;
            await _fotoRepository.UpdateAsync(foto);
            await _fotoRepository.SaveChangesAsync();
            _logger.LogInformation("Foto {FotoId} establecida como principal para acompañante {AcompananteId}.", fotoId, acompananteId);
        }

        public async Task<int> AgregarServicioAsync(int acompananteId, AgregarServicioDto servicioDto, int usuarioId, int rolId)
        {
            _logger.LogInformation(" Агregando servicio para acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", acompananteId, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para agregar servicios al acompañante {AcompananteId}.", usuarioId, acompananteId);
                throw new UnauthorizedAccessException("No tienes permisos para agregar servicios a este perfil");
            }

            // Validaciones
            if (servicioDto.Precio < 0)
            {
                _logger.LogWarning("Intento de agregar servicio con precio inválido: {Precio}.", servicioDto.Precio);
                throw new ArgumentException("El precio no puede ser negativo.");
            }

            // Crear el servicio
            var servicio = new servicio
            {
                acompanante_id = acompananteId,
                nombre = servicioDto.Nombre,
                descripcion = servicioDto.Descripcion,
                precio = servicioDto.Precio,
                duracion_minutos = servicioDto.DuracionMinutos,
                created_at = DateTime.UtcNow
            };

            await _servicioRepository.AddAsync(servicio);
            await _servicioRepository.SaveChangesAsync();
            _logger.LogInformation("Servicio {ServicioId} agregado para acompañante {AcompananteId}.", servicio.id, acompananteId);

            return servicio.id;
        }

        public async Task ActualizarServicioAsync(int servicioId, ActualizarServicioDto servicioDto, int usuarioId, int rolId)
        {
            _logger.LogInformation("Actualizando servicio {ServicioId} por usuario {UsuarioId} con rol {RolId}.", servicioId, usuarioId, rolId);

            var servicio = await _servicioRepository.GetByIdAsync(servicioId);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio {ServicioId} no encontrado.", servicioId);
                throw new InvalidOperationException("Servicio no encontrado");
            }

            var acompanante = await _acompananteRepository.GetByIdAsync(servicio.acompanante_id);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado para servicio {ServicioId}.", servicio.acompanante_id, servicioId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para modificar servicios del acompañante {AcompananteId}.", usuarioId, servicio.acompanante_id);
                throw new UnauthorizedAccessException("No tienes permisos para modificar servicios de este perfil");
            }

            // Validaciones
            if (servicioDto.Precio < 0)
            {
                _logger.LogWarning("Intento de actualizar servicio {ServicioId} con precio inválido: {Precio}.", servicioId, servicioDto.Precio);
                throw new ArgumentException("El precio no puede ser negativo.");
            }

            // Actualizar el servicio
            servicio.nombre = servicioDto.Nombre;
            servicio.descripcion = servicioDto.Descripcion;
            servicio.precio = servicioDto.Precio;
            servicio.duracion_minutos = servicioDto.DuracionMinutos;
            servicio.updated_at = DateTime.UtcNow;

            await _servicioRepository.UpdateAsync(servicio);
            await _servicioRepository.SaveChangesAsync();
            _logger.LogInformation("Servicio {ServicioId} actualizado para acompañante {AcompananteId}.", servicioId, servicio.acompanante_id);
        }

        public async Task EliminarServicioAsync(int servicioId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Eliminando servicio {ServicioId} por usuario {UsuarioId} con rol {RolId}.", servicioId, usuarioId, rolId);

            var servicio = await _servicioRepository.GetByIdAsync(servicioId);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio {ServicioId} no encontrado.", servicioId);
                throw new InvalidOperationException("Servicio no encontrado");
            }

            var acompanante = await _acompananteRepository.GetByIdAsync(servicio.acompanante_id);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado para servicio {ServicioId}.", servicio.acompanante_id, servicioId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para eliminar servicios del acompañante {AcompananteId}.", usuarioId, servicio.acompanante_id);
                throw new UnauthorizedAccessException("No tienes permisos para eliminar servicios de este perfil");
            }

            await _servicioRepository.DeleteAsync(servicio);
            await _servicioRepository.SaveChangesAsync();
            _logger.LogInformation("Servicio {ServicioId} eliminado para acompañante {AcompananteId}.", servicioId, servicio.acompanante_id);
        }

        public async Task AgregarCategoriaAsync(int acompananteId, int categoriaId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Agregando categoría {CategoriaId} al acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", categoriaId, acompananteId, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para modificar categorías del acompañante {AcompananteId}.", usuarioId, acompananteId);
                throw new UnauthorizedAccessException("No tienes permisos para modificar categorías de este perfil");
            }

            // Verificar si ya tiene la categoría
            bool tieneCat = await _acompananteRepository.TieneCategoriaAsync(acompananteId, categoriaId);
            if (tieneCat)
            {
                _logger.LogWarning("El acompañante {AcompananteId} ya tiene asignada la categoría {CategoriaId}.", acompananteId, categoriaId);
                throw new InvalidOperationException("El acompañante ya tiene asignada esta categoría");
            }

            await _acompananteRepository.AgregarCategoriaAsync(acompananteId, categoriaId);
            _logger.LogInformation("Categoría {CategoriaId} agregada al acompañante {AcompananteId}.", categoriaId, acompananteId);
        }

        public async Task EliminarCategoriaAsync(int acompananteId, int categoriaId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Eliminando categoría {CategoriaId} del acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", categoriaId, acompananteId, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para modificar categorías del acompañante {AcompananteId}.", usuarioId, acompananteId);
                throw new UnauthorizedAccessException("No tienes permisos para modificar categorías de este perfil");
            }

            // Verificar si tiene la categoría
            bool tieneCat = await _acompananteRepository.TieneCategoriaAsync(acompananteId, categoriaId);
            if (!tieneCat)
            {
                _logger.LogWarning("El acompañante {AcompananteId} no tiene asignada la categoría {CategoriaId}.", acompananteId, categoriaId);
                throw new InvalidOperationException("El acompañante no tiene asignada esta categoría");
            }

            await _acompananteRepository.EliminarCategoriaAsync(acompananteId, categoriaId);
            _logger.LogInformation("Categoría {CategoriaId} eliminada del acompañante {AcompananteId}.", categoriaId, acompananteId);
        }

        public async Task<List<AcompananteDto>> BuscarAsync(AcompananteFiltroDto filtro)
        {
            _logger.LogInformation("Buscando acompañantes con filtros: Búsqueda={Busqueda}, Ciudad={Ciudad}, País={País}, Género={Genero}, EdadMinima={EdadMinima}, EdadMaxima={EdadMaxima}, TarifaMinima={TarifaMinima}, TarifaMaxima={TarifaMaxima}, SoloVerificados={SoloVerificados}, SoloDisponibles={SoloDisponibles}, Categorías={Categorias}, OrdenarPor={OrdenarPor}, Página={Pagina}, ElementosPorPagina={ElementosPorPagina}.",
                filtro.Busqueda, filtro.Ciudad, filtro.Pais, filtro.Genero, filtro.EdadMinima, filtro.EdadMaxima, filtro.TarifaMinima, filtro.TarifaMaxima, filtro.SoloVerificados, filtro.SoloDisponibles, string.Join(",", filtro.CategoriaIds ?? new List<int>()), filtro.OrdenarPor, filtro.Pagina, filtro.ElementosPorPagina);

            var acompanantes = await _acompananteRepository.BuscarAsync(
                filtro.Busqueda,
                filtro.Ciudad,
                filtro.Pais,
                filtro.Genero,
                filtro.EdadMinima,
                filtro.EdadMaxima,
                filtro.TarifaMinima,
                filtro.TarifaMaxima,
                filtro.SoloVerificados,
                filtro.SoloDisponibles,
                filtro.CategoriaIds,
                filtro.OrdenarPor,
                filtro.Pagina,
                filtro.ElementosPorPagina);

            _logger.LogInformation("Búsqueda completada. Se encontraron {Count} acompañantes.", acompanantes.Count);
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task<List<AcompananteDto>> GetDestacadosAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY_DESTACADOS, out List<AcompananteDto> cachedDestacados))
            {
                _logger.LogInformation("Obteniendo acompañantes destacados desde caché.");
                return cachedDestacados;
            }

            _logger.LogInformation("Obteniendo acompañantes destacados desde la base de datos.");
            var acompanantes = await _acompananteRepository.GetDestacadosAsync();
            var result = _mapper.Map<List<AcompananteDto>>(acompanantes);

            _cache.Set(CACHE_KEY_DESTACADOS, result, CACHE_DURATION);
            _logger.LogInformation("Se encontraron {Count} acompañantes destacados y se almacenaron en caché.", result.Count);
            return result;
        }

        public async Task<List<AcompananteDto>> GetRecientesAsync(int cantidad = 10)
        {
            string cacheKey = $"{CACHE_KEY_RECIENTES}_{cantidad}";
            if (_cache.TryGetValue(cacheKey, out List<AcompananteDto> cachedRecientes))
            {
                _logger.LogInformation("Obteniendo acompañantes recientes desde caché. Cantidad: {Cantidad}.", cantidad);
                return cachedRecientes;
            }

            _logger.LogInformation("Obteniendo acompañantes recientes desde la base de datos. Cantidad: {Cantidad}.", cantidad);
            var acompanantes = await _acompananteRepository.GetRecientesAsync(cantidad);
            var result = _mapper.Map<List<AcompananteDto>>(acompanantes);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Se encontraron {Count} acompañantes recientes y se almacenaron en caché.", result.Count);
            return result;
        }

        public async Task<List<AcompananteDto>> GetPopularesAsync(int cantidad = 10)
        {
            string cacheKey = $"{CACHE_KEY_POPULARES}_{cantidad}";
            if (_cache.TryGetValue(cacheKey, out List<AcompananteDto> cachedPopulares))
            {
                _logger.LogInformation("Obteniendo acompañantes populares desde caché. Cantidad: {Cantidad}.", cantidad);
                return cachedPopulares;
            }

            _logger.LogInformation("Obteniendo acompañantes populares desde la base de datos. Cantidad: {Cantidad}.", cantidad);
            var acompanantes = await _acompananteRepository.GetPopularesAsync(cantidad);
            var result = _mapper.Map<List<AcompananteDto>>(acompanantes);

            _cache.Set(cacheKey, result, CACHE_DURATION);
            _logger.LogInformation("Se encontraron {Count} acompañantes populares y se almacenaron en caché.", result.Count);
            return result;
        }

        public async Task<bool> EstaVerificadoAsync(int acompananteId)
        {
            _logger.LogInformation("Verificando estado de verificación para acompañante {AcompananteId}.", acompananteId);
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }
            bool verificado = acompanante.esta_verificado == true;
            _logger.LogInformation("Acompañante {AcompananteId} está verificado: {Verificado}.", acompananteId, verificado);
            return verificado;
        }

        public async Task VerificarAcompananteAsync(int acompananteId, int agenciaId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Verificando acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId} y agencia {AgenciaId}.", acompananteId, usuarioId, rolId, agenciaId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Validar que el acompañante no esté ya verificado
            if (acompanante.esta_verificado == true)
            {
                _logger.LogWarning("El acompañante {AcompananteId} ya está verificado.", acompananteId);
                throw new InvalidOperationException("El acompañante ya está verificado");
            }

            // Validar que el acompañante pertenezca a una agencia
            if (!acompanante.agencia_id.HasValue)
            {
                _logger.LogWarning("El acompañante {AcompananteId} no pertenece a ninguna agencia.", acompananteId);
                throw new InvalidOperationException("El acompañante debe pertenecer a una agencia para ser verificado");
            }

            // Solo agencias o administradores pueden verificar
            if (rolId != 1 && rolId != 2)
            {
                _logger.LogWarning("Usuario {UsuarioId} con rol {RolId} no tiene permisos para verificar acompañantes.", usuarioId, rolId);
                throw new UnauthorizedAccessException("No tienes permisos para verificar acompañantes");
            }

            // Si es agencia, solo puede verificar acompañantes de su agencia
            if (rolId == 2 && acompanante.agencia_id != agenciaId)
            {
                _logger.LogWarning("Usuario {UsuarioId} (agencia {AgenciaId}) no tiene permisos para verificar acompañante {AcompananteId} (agencia {AcompananteAgenciaId}).", usuarioId, agenciaId, acompananteId, acompanante.agencia_id);
                throw new UnauthorizedAccessException("Solo puedes verificar acompañantes de tu agencia");
            }

            // Verificar el acompañante
            acompanante.esta_verificado = true;
            acompanante.fecha_verificacion = DateTime.UtcNow;
            acompanante.updated_at = DateTime.UtcNow;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
            _logger.LogInformation("Acompañante {AcompananteId} verificado correctamente.", acompananteId);
        }

        public async Task RevocarVerificacionAsync(int acompananteId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Revocando verificación de acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", acompananteId, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Solo agencias o administradores pueden revocar verificación
            if (rolId != 1 && rolId != 2)
            {
                _logger.LogWarning("Usuario {UsuarioId} con rol {RolId} no tiene permisos para revocar verificación.", usuarioId, rolId);
                throw new UnauthorizedAccessException("No tienes permisos para revocar verificación");
            }

            // Si es agencia, solo puede modificar acompañantes de su agencia
            if (rolId == 2 && acompanante.agencia_id != usuarioId)
            {
                _logger.LogWarning("Usuario {UsuarioId} (agencia) no tiene permisos para revocar verificación del acompañante {AcompananteId} (agencia {AcompananteAgenciaId}).", usuarioId, acompananteId, acompanante.agencia_id);
                throw new UnauthorizedAccessException("Solo puedes modificar acompañantes de tu agencia");
            }

            // Revocar verificación
            acompanante.esta_verificado = false;
            acompanante.fecha_verificacion = null;
            acompanante.updated_at = DateTime.UtcNow;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
            _logger.LogInformation("Verificación revocada para acompañante {AcompananteId}.", acompananteId);
        }

        public async Task RegistrarVisitaAsync(int acompananteId, string ipVisitante, string userAgent, int? clienteId = null)
        {
            _logger.LogInformation("Registrando visita para acompañante {AcompananteId}. IP: {IpVisitante}, ClienteId: {ClienteId}.", acompananteId, ipVisitante, clienteId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Crear registro de visita
            var visita = new visitas_perfil
            {
                acompanante_id = acompananteId,
                cliente_id = clienteId,
                ip_visitante = ipVisitante,
                user_agent = userAgent,
                fecha_visita = DateTime.UtcNow,
                duracion_segundos = 0,
                created_at = DateTime.UtcNow
            };

            await _visitaPerfilRepository.AddAsync(visita);
            await _visitaPerfilRepository.SaveChangesAsync();
            _logger.LogInformation("Visita registrada para acompañante {AcompananteId}. VisitaId: {VisitaId}.", acompananteId, visita.id);

            // Actualizar score de actividad
            long nuevoScore = await CalcularScoreActividadAsync(acompananteId);
            await _acompananteRepository.ActualizarScoreActividadAsync(acompananteId, nuevoScore);
            _logger.LogDebug("Score de actividad actualizado para acompañante {AcompananteId}: {NuevoScore}.", acompananteId, nuevoScore);
        }

        public async Task RegistrarContactoAsync(int acompananteId, string tipoContacto, string ipContacto, int? clienteId = null)
        {
            _logger.LogInformation("Registrando contacto para acompañante {AcompananteId}. Tipo: {TipoContacto}, IP: {IpContacto}, ClienteId: {ClienteId}.", acompananteId, tipoContacto, ipContacto, clienteId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Validar tipo de contacto
            if (!new[] { "telefono", "whatsapp", "email" }.Contains(tipoContacto.ToLower()))
            {
                _logger.LogWarning("Tipo de contacto inválido: {TipoContacto}.", tipoContacto);
                throw new InvalidOperationException("Tipo de contacto inválido");
            }

            // Crear registro de contacto
            var contacto = new contacto
            {
                acompanante_id = acompananteId,
                cliente_id = clienteId,
                tipo_contacto = tipoContacto.ToLower(),
                fecha_contacto = DateTime.UtcNow,
                esta_registrado = clienteId.HasValue,
                ip_contacto = ipContacto,
                created_at = DateTime.UtcNow
            };

            await _contactoRepository.AddAsync(contacto);
            await _contactoRepository.SaveChangesAsync();
            _logger.LogInformation("Contacto registrado para acompañante {AcompananteId}. ContactoId: {ContactoId}.", acompananteId, contacto.id);

            // Actualizar score de actividad
            long nuevoScore = await CalcularScoreActividadAsync(acompananteId);
            await _acompananteRepository.ActualizarScoreActividadAsync(acompananteId, nuevoScore);
            _logger.LogDebug("Score de actividad actualizado para acompañante {AcompananteId}: {NuevoScore}.", acompananteId, nuevoScore);
        }

        public async Task<AcompananteEstadisticasDto> GetEstadisticasAsync(int acompananteId, int usuarioId, int rolId)
        {
            _logger.LogInformation("Obteniendo estadísticas para acompañante {AcompananteId} por usuario {UsuarioId} con rol {RolId}.", acompananteId, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEstadisticas(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para ver estadísticas del acompañante {AcompananteId}.", usuarioId, acompananteId);
                throw new UnauthorizedAccessException("No tienes permisos para ver estadísticas de este perfil");
            }

            // Obtener datos de contactos y visitas
            var totalVisitas = await _visitaPerfilRepository.GetTotalByAcompananteIdAsync(acompananteId);
            var totalContactos = await _contactoRepository.GetTotalByAcompananteIdAsync(acompananteId);
            var contactosPorTipo = await _contactoRepository.GetContactosPorTipoAsync(acompananteId);
            var visitasPorDia = await _visitaPerfilRepository.GetVisitasPorDiaAsync(acompananteId, 30);

            var estadisticas = new AcompananteEstadisticasDto
            {
                Id = acompanante.id,
                NombrePerfil = acompanante.nombre_perfil,
                TotalVisitas = totalVisitas,
                TotalContactos = totalContactos,
                ScoreActividad = acompanante.score_actividad ?? 0,
                ContactosPorTipo = contactosPorTipo.ToDictionary(c => c.Key, c => c.Value),
                VisitasPorDia = visitasPorDia.Select(v => new VisitaDiaDto
                {
                    Fecha = v.Key,
                    CantidadVisitas = v.Value
                }).ToList()
            };

            _logger.LogInformation("Estadísticas obtenidas para acompañante {AcompananteId}. TotalVisitas: {TotalVisitas}, TotalContactos: {TotalContactos}.", acompananteId, totalVisitas, totalContactos);
            return estadisticas;
        }

        public async Task CambiarDisponibilidadAsync(int acompananteId, bool estaDisponible, int usuarioId, int rolId)
        {
            _logger.LogInformation("Cambiando disponibilidad del acompañante {AcompananteId} a {EstaDisponible} por usuario {UsuarioId} con rol {RolId}.", acompananteId, estaDisponible, usuarioId, rolId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante {AcompananteId} no encontrado.", acompananteId);
                throw new InvalidOperationException("Acompañante no encontrado");
            }

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene permisos para modificar el perfil de acompañante {AcompananteId}.", usuarioId, acompananteId);
                throw new UnauthorizedAccessException("No tienes permisos para modificar este perfil");
            }

            acompanante.esta_disponible = estaDisponible;
            acompanante.updated_at = DateTime.UtcNow;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
            _logger.LogInformation("Disponibilidad del acompañante {AcompananteId} actualizada a {EstaDisponible}.", acompananteId, estaDisponible);
        }

        public async Task<PaginatedResultDto<AcompananteDto>> GetAllPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            _logger.LogInformation("Obteniendo acompañantes paginados. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);

            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetAllPaginatedAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Se obtuvieron {Count} acompañantes paginados. Total: {TotalItems}.", result.Items.Count, total);
            return result;
        }

        public async Task<PaginatedResultDto<AcompananteResumen2Dto>> GetAllPaginatedResumenAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            _logger.LogInformation("Obteniendo acompañantes paginados. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);

            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetAllPaginatedAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteResumen2Dto>
            {
                Items = _mapper.Map<List<AcompananteResumen2Dto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Se obtuvieron {Count} acompañantes paginados. Total: {TotalItems}.", result.Items.Count, total);
            return result;
        }
        public async Task<PaginatedResultDto<AcompananteDto>> GetRecientesPaginadosAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            string cacheKey = $"RecientesPaginados_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out PaginatedResultDto<AcompananteDto> cachedResult))
            {
                _logger.LogInformation("Obteniendo acompañantes recientes paginados desde caché. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);
                return cachedResult;
            }

            _logger.LogInformation("Obteniendo acompañantes recientes paginados desde la base de datos. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);
            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetRecientesPaginadosAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Se obtuvieron {Count} acompañantes recientes paginados. Total: {TotalItems}. Almacenados en caché.", result.Items.Count, total);
            return result;
        }

        public async Task<PaginatedResultDto<AcompananteDto>> GetPopularesPaginadosAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            string cacheKey = $"PopularesPaginados_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out PaginatedResultDto<AcompananteDto> cachedResult))
            {
                _logger.LogInformation("Obteniendo acompañantes populares paginados desde caché. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);
                return cachedResult;
            }

            _logger.LogInformation("Obteniendo acompañantes populares paginados desde la base de datos. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);
            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetPopularesPaginadosAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _cache.Set(cacheKey, result, CACHE_DURATION);
            _logger.LogInformation("Se obtuvieron {Count} acompañantes populares paginados. Total: {TotalItems}. Almacenados en caché.", result.Items.Count, total);
            return result;
        }

        public async Task<PaginatedResultDto<AcompananteDto>> GetDestacadosPaginadosAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            string cacheKey = $"DestacadosPaginados_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out PaginatedResultDto<AcompananteDto> cachedResult))
            {
                _logger.LogInformation("Obteniendo acompañantes destacados paginados desde caché. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);
                return cachedResult;
            }

            _logger.LogInformation("Obteniendo acompañantes destacados paginados desde la base de datos. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);
            var total = await _acompananteRepository.CountDestacadosAsync();
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetDestacadosPaginadosAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _cache.Set(cacheKey, result, CACHE_DURATION);
            _logger.LogInformation("Se obtuvieron {Count} acompañantes destacados paginados. Total: {TotalItems}. Almacenados en caché.", result.Items.Count, total);
            return result;
        }

        // Métodos privados auxiliares
        private bool TienePermisosEdicion(acompanante acompanante, int usuarioId, int rolId)
        {
            if (rolId == 1) return true; // Admin
            if (acompanante.usuario_id == usuarioId) return true; // Propio acompañante
            if (rolId == 2 && acompanante.agencia_id.HasValue && acompanante.agencia_id == usuarioId) return true; // Agencia
            return false;
        }

        private bool TienePermisosEstadisticas(acompanante acompanante, int usuarioId, int rolId)
        {
            return TienePermisosEdicion(acompanante, usuarioId, rolId);
        }

        private async Task<long> CalcularScoreActividadAsync(int acompananteId)
        {
            const int PESO_VISITA = 1;
            const int PESO_CONTACTO = 5;
            var fechaInicio = DateTime.UtcNow.AddDays(-30);

            int visitas = await _visitaPerfilRepository.GetTotalDesdeAsync(acompananteId, fechaInicio);
            int contactos = await _contactoRepository.GetTotalDesdeAsync(acompananteId, fechaInicio);

            long score = (visitas * PESO_VISITA) + (contactos * PESO_CONTACTO);
            _logger.LogDebug("Score calculado para acompañante {AcompananteId}: Visitas={Visitas}, Contactos={Contactos}, Score={Score}.", acompananteId, visitas, contactos, score);
            return score;
        }

        public async Task<PaginatedResultDto<AcompananteDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            _logger.LogInformation("Obteniendo todos los acompañantes paginados. Página: {PageNumber}, Tamaño: {PageSize}.", pageNumber, pageSize);

            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetAllPaginatedAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Se obtuvieron {Count} acompañantes. Total: {TotalItems}.", result.Items.Count, total);
            return result;
        }


        public async Task<bool> ActualizarStripeAccountIdAsync(int acompananteId, string stripeAccountId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                return false;

            acompanante.stripe_account_id = stripeAccountId;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActualizarEstatusCuentaPagoAsync(int acompananteId, string estado)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                return false;

            // Actualizar estado basado en la respuesta de Stripe
            acompanante.stripe_onboarding_completed = estado == "completed";

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();

            return true;
        }
    }
}