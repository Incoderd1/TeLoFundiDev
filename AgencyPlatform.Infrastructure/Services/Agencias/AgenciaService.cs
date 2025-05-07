using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Application.DTOs.Agencias.AgenciaDah;
using AgencyPlatform.Application.DTOs.Anuncios;
using AgencyPlatform.Application.DTOs.Estadisticas;
using AgencyPlatform.Application.DTOs.Solicitudes;
using AgencyPlatform.Application.DTOs.SolicitudesAgencia;
using AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia;
using AgencyPlatform.Application.DTOs.Verificacion;
using AgencyPlatform.Application.DTOs.Verificaciones;
using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using AgencyPlatform.Application.Interfaces.Services.Cliente;
using AgencyPlatform.Application.Interfaces.Services.EmailAgencia;
using AgencyPlatform.Application.Interfaces.Services.Notificaciones;
using AgencyPlatform.Application.Interfaces.Utils;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Core.Enums;
using AgencyPlatform.Infrastructure.Repositories;
using AgencyPlatform.Shared.EmailTemplates;
using AgencyPlatform.Shared.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Transactions;

namespace AgencyPlatform.Infrastructure.Services.Agencias
{
    public class AgenciaService : IAgenciaService
    {
        private readonly IAgenciaRepository _agenciaRepository;
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IVerificacionRepository _verificacionRepository;
        private readonly IAnuncioDestacadoRepository _anuncioRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ISolicitudAgenciaRepository _solicitudAgenciaRepository;
        private readonly IComisionRepository _comisionRepository;
        private readonly IUserRepository _usuarioRepository;
        private readonly IEmailSender _emailSender;
        private readonly INotificadorRealTime _notificador;
        private readonly ILogger<AgenciaService> _logger;
        private readonly IUserService _userService;
        private readonly ISolicitudRegistroAgenciaRepository _solicitudRegistroAgenciaRepository;
        private readonly IPagoVerificacionRepository _pagoVerificacionRepository;
        private readonly INotificacionService _notificacionService;
        private readonly IEmailProfesionalService _emailProfesionalService;
        private readonly ICuponClienteRepository _cuponRepository;
        private readonly IPaqueteCuponRepository _paqueteRepository;
        private readonly IClienteService _clienteService;
        private readonly IClienteRepository _clienteRepository;
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;


        private readonly List<TipoAnuncioCosto> _costos;

        public AgenciaService(
            IAgenciaRepository agenciaRepository,
            IAcompananteRepository acompananteRepository,
            IVerificacionRepository verificacionRepository,
            IAnuncioDestacadoRepository anuncioRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            ISolicitudAgenciaRepository solicitudAgenciaRepository,
            IComisionRepository comision,
            IUserRepository usuarioRepository,
            IEmailSender emailSender,
            INotificadorRealTime notificador,
            ILogger<AgenciaService> logger,
            IUserService userService,
            ISolicitudRegistroAgenciaRepository solicitudRegistroAgenciaRepository,
            IPagoVerificacionRepository pagoVerificacionRepository,
            INotificacionService notificacionService,
            IEmailProfesionalService emailProfesionalService,
            ICuponClienteRepository cuponRepository,
            IPaqueteCuponRepository paqueteRepository,
            IClienteService clienteService,
            IClienteRepository clienteRepository,
            IConfiguration configuration,
            IPaymentService paymentService)
        {
            _agenciaRepository = agenciaRepository;
            _acompananteRepository = acompananteRepository;
            _verificacionRepository = verificacionRepository;
            _anuncioRepository = anuncioRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _solicitudAgenciaRepository = solicitudAgenciaRepository;
            _comisionRepository = comision;
            _usuarioRepository = usuarioRepository;
            _emailSender = emailSender;
            _notificador = notificador;
            _logger = logger;
            _userService = userService;
            _solicitudRegistroAgenciaRepository = solicitudRegistroAgenciaRepository;
            _pagoVerificacionRepository = pagoVerificacionRepository;
            _notificacionService = notificacionService;
            _emailProfesionalService = emailProfesionalService;
            _cuponRepository = cuponRepository;
            _paqueteRepository = paqueteRepository;
            _clienteService = clienteService;
            _clienteRepository = clienteRepository;
            _configuration = configuration;
            _paymentService = paymentService;

            // Cargar los costos desde appsettings.json
            _costos = new List<TipoAnuncioCosto>();
            var costosConfig = _configuration.GetSection("AnuncioCostos").Get<List<TipoAnuncioCostoConfig>>();
            if (costosConfig == null || !costosConfig.Any())
            {
                throw new InvalidOperationException("No se encontraron costos de anuncios configurados en appsettings.json.");
            }

            foreach (var costo in costosConfig)
            {
                if (Enum.TryParse<TipoAnuncio>(costo.Tipo, true, out var tipoAnuncio))
                {
                    _costos.Add(new TipoAnuncioCosto
                    {
                        Tipo = tipoAnuncio,
                        Duracion = costo.Duracion,
                        Costo = costo.Costo
                    });
                }
                else
                {
                    _logger.LogWarning("Tipo de anuncio no válido en la configuración: {Tipo}", costo.Tipo);
                }
            }
        }

        public async Task<List<AgenciaDto>> GetAllAsync()
        {
            var entidades = await _agenciaRepository.GetAllAsync();
            return _mapper.Map<List<AgenciaDto>>(entidades);
        }

        public async Task<AgenciaDto?> GetByIdAsync(int id)
        {
            var entidad = await _agenciaRepository.GetByIdAsync(id);
            return entidad == null ? null : _mapper.Map<AgenciaDto>(entidad);
        }

        public async Task<AgenciaDto?> GetByUsuarioIdAsync(int usuarioId)
        {
            var entidad = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId);
            return entidad == null ? null : _mapper.Map<AgenciaDto>(entidad);
        }

        public async Task CrearAsync(CrearAgenciaDto nuevaAgenciaDto)
        {
            var nueva = _mapper.Map<agencia>(nuevaAgenciaDto);
            nueva.usuario_id = ObtenerUsuarioId();
            nueva.esta_verificada = false;

            await _agenciaRepository.AddAsync(nueva);
            await _agenciaRepository.SaveChangesAsync();
        }

        public async Task<agencia> CrearPendienteAsync(CrearSolicitudRegistroAgenciaDto dto)
        {
            var agencia = new agencia
            {
                nombre = dto.Nombre,
                email = dto.Email,
                descripcion = dto.Descripcion ?? string.Empty,
                logo_url = string.Empty,
                sitio_web = string.Empty,
                direccion = dto.Direccion ?? string.Empty,
                ciudad = dto.Ciudad ?? string.Empty,
                pais = dto.Pais ?? string.Empty,
                esta_verificada = false,
                fecha_verificacion = null,
                comision_porcentaje = null,
            };

            await _agenciaRepository.AddAsync(agencia);
            await _agenciaRepository.SaveChangesAsync();

            return agencia;
        }

        public async Task<int> SolicitarRegistroAgenciaAsync(CrearSolicitudRegistroAgenciaDto dto)
        {
            var solicitudAgencia = new solicitud_registro_agencia
            {
                nombre = dto.Nombre,
                email = dto.Email,
                password_hash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                descripcion = dto.Descripcion ?? string.Empty,
                ciudad = dto.Ciudad ?? string.Empty,
                pais = dto.Pais ?? string.Empty,
                direccion = dto.Direccion ?? string.Empty,
                fecha_solicitud = DateTime.UtcNow,
                estado = "pendiente",
                logo_url = string.Empty,
                sitio_web = string.Empty
            };

            await _solicitudRegistroAgenciaRepository.AddAsync(solicitudAgencia);
            await _solicitudRegistroAgenciaRepository.SaveChangesAsync();

            await _notificacionService.NotificarPorSignalR(0, $"Nueva solicitud de agencia (ID: {solicitudAgencia.id}) pendiente", "info");

            var admins = await _usuarioRepository.GetUsersByRoleAsync("admin");

            foreach (var admin in admins)
            {
                if (!string.IsNullOrEmpty(admin.email))
                {
                    await _notificacionService.NotificarPorEmail(
                        admin.email,
                        "Nueva solicitud de agencia",
                        $"Se ha recibido una nueva solicitud de agencia (ID: {solicitudAgencia.id})."
                    );
                }
            }

            return solicitudAgencia.id;
        }

        private bool IsValidEmail(string email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public async Task<int> GetAgenciaIdByAcompananteIdAsync(int acompananteId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
            {
                _logger.LogWarning("Acompañante con ID: {AcompananteId} no encontrado", acompananteId);
                return 0;
            }
            return acompanante.agencia_id ?? 0;
        }

        public async Task<List<SolicitudRegistroAgenciaDto>> GetSolicitudesRegistroPendientesAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando GetSolicitudesRegistroPendientesAsync");

                if (!EsAdmin())
                    throw new UnauthorizedAccessException("Solo los administradores pueden ver solicitudes de registro de agencia.");

                _logger.LogInformation("Verificación de admin completada. Obteniendo solicitudes pendientes del repositorio");

                List<solicitud_registro_agencia> solicitudes;
                try
                {
                    solicitudes = await _solicitudRegistroAgenciaRepository.GetSolicitudesPendientesAsync();
                    _logger.LogInformation($"Obtenidas {solicitudes.Count} solicitudes pendientes");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener solicitudes pendientes del repositorio");
                    throw;
                }

                _logger.LogInformation("Iniciando mapeo manual de solicitudes a DTOs");
                var dtos = new List<SolicitudRegistroAgenciaDto>();

                foreach (var solicitud in solicitudes)
                {
                    try
                    {
                        _logger.LogInformation($"Mapeando solicitud ID: {solicitud.id}, Nombre: {solicitud.nombre}");
                        _logger.LogInformation($"Valores originales - LogoUrl: '{solicitud.logo_url}', SitioWeb: '{solicitud.sitio_web}'");

                        var dto = new SolicitudRegistroAgenciaDto
                        {
                            Id = solicitud.id,
                            Nombre = solicitud.nombre,
                            Email = solicitud.email,
                            Descripcion = solicitud.descripcion ?? string.Empty,
                            Ciudad = solicitud.ciudad ?? string.Empty,
                            Pais = solicitud.pais ?? string.Empty,
                            Direccion = solicitud.direccion ?? string.Empty,
                            LogoUrl = solicitud.logo_url ?? string.Empty,
                            SitioWeb = solicitud.sitio_web ?? string.Empty,
                            Estado = solicitud.estado,
                            FechaSolicitud = solicitud.fecha_solicitud,
                            FechaRespuesta = solicitud.fecha_respuesta
                        };

                        _logger.LogInformation($"Solicitud mapeada correctamente - ID: {dto.Id}");
                        dtos.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error al mapear solicitud ID: {solicitud.id}");
                    }
                }

                _logger.LogInformation($"Mapeo completado. Retornando {dtos.Count} DTOs");
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en GetSolicitudesRegistroPendientesAsync");
                throw;
            }
        }

        public async Task<bool> AprobarSolicitudRegistroAgenciaAsync(int solicitudId)
        {
            if (!EsAdmin())
                throw new UnauthorizedAccessException("Solo los administradores pueden aprobar solicitudes.");

            var solicitud = await _solicitudRegistroAgenciaRepository.GetByIdAsync(solicitudId);

            if (solicitud == null)
                throw new Exception("Solicitud no encontrada.");

            if (solicitud.estado != "pendiente")
                throw new Exception("Esta solicitud ya ha sido procesada.");

            var user = new usuario
            {
                email = solicitud.email,
                password_hash = solicitud.password_hash,
                rol_id = await _usuarioRepository.GetRoleIdByNameAsync("agencia"),
                esta_activo = true,
                fecha_registro = DateTime.UtcNow
            };

            await _usuarioRepository.AddAsync(user);
            await _usuarioRepository.SaveChangesAsync();

            var agencia = new agencia
            {
                usuario_id = user.id,
                nombre = solicitud.nombre,
                descripcion = solicitud.descripcion,
                logo_url = solicitud.logo_url,
                sitio_web = solicitud.sitio_web,
                direccion = solicitud.direccion,
                ciudad = solicitud.ciudad,
                pais = solicitud.pais,
                esta_verificada = false
            };

            await _agenciaRepository.AddAsync(agencia);

            solicitud.estado = "aprobada";
            solicitud.fecha_respuesta = DateTime.UtcNow;
            await _solicitudRegistroAgenciaRepository.UpdateAsync(solicitud);

            await _solicitudRegistroAgenciaRepository.SaveChangesAsync();
            await _agenciaRepository.SaveChangesAsync();

            await _emailProfesionalService.EnviarCorreoAprobacionAgencia(solicitud.email, solicitud.nombre);
            return true;
        }

        public async Task<bool> RechazarSolicitudRegistroAgenciaAsync(int solicitudId, string motivo)
        {
            if (!EsAdmin())
                throw new UnauthorizedAccessException("Solo los administradores pueden rechazar solicitudes.");

            var solicitud = await _solicitudRegistroAgenciaRepository.GetByIdAsync(solicitudId);

            if (solicitud == null)
                throw new Exception("Solicitud no encontrada.");

            if (solicitud.estado != "pendiente")
                throw new Exception("Esta solicitud ya ha sido procesada.");

            solicitud.estado = "rechazada";
            solicitud.fecha_respuesta = DateTime.UtcNow;
            solicitud.motivo_rechazo = motivo;

            await _solicitudRegistroAgenciaRepository.UpdateAsync(solicitud);
            await _solicitudRegistroAgenciaRepository.SaveChangesAsync();

            await _emailProfesionalService.EnviarCorreoRechazoAgencia(solicitud.email, solicitud.nombre, motivo);

            return true;
        }

        public async Task NotificarAdminsNuevaSolicitudAgencia(int solicitudId)
        {
            try
            {
                var admins = await _usuarioRepository.GetUsersByRoleAsync("admin");

                foreach (var admin in admins)
                {
                    await _notificador.NotificarUsuarioAsync(
                        admin.id,
                        $"Nueva solicitud de agencia (ID: {solicitudId}) pendiente de revisión",
                        "info"
                    );

                    var emailDestino = admin.email;
                    if (!string.IsNullOrWhiteSpace(emailDestino))
                    {
                        var asunto = "Nueva solicitud de registro de agencia";
                        var mensaje = $@"
                    <html>
                    <head>
                        <style>
                            h1 {{ font-size: 18px; color: #333; }}
                            p {{ font-size: 16px; color: #555; }}
                            .button {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-align: center; text-decoration: none; display: inline-block; }}
                        </style>
                    </head>
                    <body>
                        <h1>¡Hola, Administrador!</h1>
                        <p>Se ha recibido una nueva solicitud de registro de agencia. La solicitud tiene el ID: <strong>{solicitudId}</strong>.</p>
                        <p>Por favor, revisa el panel de administración para aprobar o rechazar la solicitud.</p>
                        <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                        <p>Saludos cordiales,</p>
                        <p><em>El equipo de AgencyPlatform</em></p>
                        <img src='data:image/png;base64,{GetBase64Image()}' alt='Logo' style='width:100px;height:auto;' />
                    </body>
                    </html>
                ";

                        await _emailProfesionalService.EnviarNotificacionAdministrador(
                            emailDestino,
                            "Nueva solicitud de registro de agencia",
                            $"Se ha recibido una nueva solicitud de registro de agencia. La solicitud tiene el ID: <strong>{solicitudId}</strong>. Por favor revisa el panel de administración."
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar a los administradores sobre la solicitud de agencia.");
            }
        }

        private string GetBase64Image()
        {
            return "iVBORw0KGgoAAAANSUhEUgAAAJQAAACUCAMAAABC4vDmAAAAYFBMVEX///8AAAD5+fnl5eVpaWnU1NTx8fFISEjs7Ow1NTWPj49eXl7Z2dmbm5v29vb8/PywsLAkJCS3t7cwMDB9fX2mpqYRERFRUVF0dHQaGhpCQkLf39+FhYVWVlbMzMy+vr5RG8qnAAADcUlEQVR4nO2b2ZKjIBRAIRo3RDTGLZr4/385gPaUW2aQCLFm7nnq6lyLU5dFFkEIAAAAAAAAAAAAAP4tmOkCbrW7E+oZVnIeWIMbupiUqnSchJVBUoyb4rqLVz5YmWtXd4yLnY94oeFcXVyMg53PBCEuiclcCSln5zNcCgfEYK50pRB7msuVthQymCt9KcQqU7n6QMpcrj6RQkllxuojKeTxp3F6Din+tkyGPxMj7UpHSrwFXMeTOE8DNagl9eIipS+p5BzDOTZXWlKoWEwZ6JFKmlIMvchMqj62UellaobDW9ixMz6QUgWkVAEpVUBKFZBSBaRUASlVQEoVkFIFpFRZSiXtuPZFQbso6dIGqyA7UgSToUAnx9E8NMLhEJnwIItSDPFFnJ8MTrOSOTwyF6Gej382OKxIIXTjBT4T6VRe50vMa4lFrpLnYqfFQkPveZFVzJ2aeBkbNzxXsdhn6af/Ni/FxG6/oLyug0Wu8GpHyoYU6kXR+SpP4jeRQVz28z0WK+NUIKT87XDRxsvFEZYNqTiTVfRI1sGy32GctbalAtHvItEHV7FyPz/iecxnuTIv1fI8ZbHsg/6immSeepnJbNrizEuJ3cLr2Afv89D72O+uonKtSmU/WeCjqDsPrYcdVyZy1ViViungxNCLLnqlQ1/jT3Fkt/rQ/oP0/3Q+pQFIqQJSSLEjWpa6BE5fRF0XFX3rvS3XqtSLTg9gSjKOnV+UCmi4/r4lXA7yVqWcaJz4kq5IOUU3Jq2MNoKtSF2G4zy/i5PfLZ0lcTRM8ehSwIpUK+Zy2O1XBV1e4hwbP+cTTytSt0ZU28a6QRCLamxsr2aGk9j07QDF5AqsmI5g5qXk7Dz406gZyJn65B9W9hLcjXXMFPmFhEUpnqk8Yn97uzAeZFWKOQrvOx5ktU0pMc/lSaTmgJQqIKUKSKkCUqqAlCogpQpIqQJSqpiR2nvxYkFwuJTYsv/w2+10dQzwMRqXeeYU2fGfpDPNa09TqoOddC+ITXl82FM22X+Vbkpt9KoYAAAAAAAAcBJY4r3hm1Zp6G/zTSn6bjb/bakcN6GbZW5FSEl8/HycQarz74UfUj+su7vbuZRmJ5CqSURrwtfflZtiv7sV5QmksltIqwetsornqa676AyZwoTnKPfdPMtLl+BHeAqpM/a+TX4BkQ4q4g8R7tMAAAAASUVORK5CYII=";
        }

        private async Task NotificarRechazoAgencia(string email, string nombreAgencia, string motivo)
        {
            try
            {
                await _emailSender.SendEmailAsync(
                    email,
                    "Tu solicitud de registro de agencia ha sido rechazada",
                    $"Lo sentimos, tu solicitud para registrar la agencia '{nombreAgencia}' ha sido rechazada. " +
                    $"Motivo: {motivo}" +
                    $"\n\nSi consideras que esto es un error o deseas obtener más información, por favor contacta a nuestro equipo de soporte."
                );

                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                if (usuario != null)
                {
                    await _notificador.NotificarUsuarioAsync(
                        usuario.id,
                        $"Tu solicitud de agencia '{nombreAgencia}' ha sido rechazada. Motivo: {motivo}",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de rechazo a {Email}", email);
            }
        }

        public async Task ActualizarAsync(UpdateAgenciaDto agenciaDto)
        {
            var usuarioId = ObtenerUsuarioId();
            var actual = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId);

            if (actual == null || actual.id != agenciaDto.Id)
                throw new UnauthorizedAccessException("No tienes permisos para editar esta agencia.");

            actual.nombre = agenciaDto.Nombre;
            actual.descripcion = agenciaDto.Descripcion;
            actual.logo_url = agenciaDto.LogoUrl;
            actual.sitio_web = agenciaDto.SitioWeb;
            actual.direccion = agenciaDto.Direccion;
            actual.ciudad = agenciaDto.Ciudad;
            actual.pais = agenciaDto.Pais;

            await _agenciaRepository.UpdateAsync(actual);
            await _agenciaRepository.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var entidad = await _agenciaRepository.GetByIdAsync(id);

            if (entidad == null)
                throw new Exception("Agencia no encontrada.");

            if (entidad.usuario_id != ObtenerUsuarioId() && !EsAdmin())
                throw new UnauthorizedAccessException("No tienes permisos para eliminar esta agencia.");

            await _agenciaRepository.DeleteAsync(entidad);
            await _agenciaRepository.SaveChangesAsync();
        }

        public async Task<List<AcompananteDto>> GetAcompanantesByAgenciaIdAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var acompanantes = await _agenciaRepository.GetAcompanantesByAgenciaIdAsync(agenciaId);
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task AgregarAcompananteAsync(int agenciaId, int acompananteId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);

            if (acompanante == null)
                throw new Exception("Acompañante no encontrado.");

            if (acompanante.agencia_id != null && acompanante.agencia_id != agenciaId)
                throw new Exception("El acompañante ya pertenece a otra agencia.");

            acompanante.agencia_id = agenciaId;

            if (acompanante.esta_verificado == true)
            {
                acompanante.esta_verificado = false;
                acompanante.fecha_verificacion = null;

                var verificacion = await _verificacionRepository.GetByAcompananteIdAsync(acompananteId);
                if (verificacion != null)
                {
                    await _verificacionRepository.DeleteAsync(verificacion);
                }
            }

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
        }

        public async Task RemoverAcompananteAsync(int agenciaId, int acompananteId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);

            if (acompanante == null)
                throw new Exception("Acompañante no encontrado.");

            if (acompanante.agencia_id != agenciaId)
                throw new Exception("El acompañante no pertenece a esta agencia.");

            acompanante.agencia_id = null;
            acompanante.esta_verificado = false;
            acompanante.fecha_verificacion = null;

            var verificacion = await _verificacionRepository.GetByAcompananteIdAsync(acompananteId);
            if (verificacion != null)
            {
                await _verificacionRepository.DeleteAsync(verificacion);
            }

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
        }

        public async Task<VerificacionDto> VerificarAcompananteAsync(int agenciaId, int acompananteId, VerificarAcompananteDto datosVerificacion)
        {
            _logger.LogDebug("Iniciando verificación de acompañante {AcompananteId} para agencia {AgenciaId}. MontoCobrado: {MontoCobrado}, Observaciones: {Observaciones}", acompananteId, agenciaId, datosVerificacion.MontoCobrado, datosVerificacion.Observaciones);

            await VerificarPermisosAgencia(agenciaId);

            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);

            if (agencia == null)
                throw new Exception("Agencia no encontrada.");

            if (acompanante == null)
                throw new Exception("Acompañante no encontrado.");

            if (acompanante.agencia_id != agenciaId)
                throw new Exception("El acompañante no pertenece a esta agencia.");

            if (acompanante.esta_verificado == true)
                throw new Exception("El acompañante ya está verificado.");

            if (agencia.esta_verificada != true)
                throw new Exception("La agencia debe estar verificada para poder verificar acompañantes.");

            if (datosVerificacion.MontoCobrado < 0)
                throw new ArgumentException("El monto cobrado no puede ser negativo.");

            bool yaPagoVerificacion = await _pagoVerificacionRepository.ExistenPagosCompletadosAsync(acompananteId);

            if (yaPagoVerificacion)
            {
                _logger.LogInformation("El acompañante {AcompananteId} ya tiene un pago de verificación completado. Estableciendo MontoCobrado a 0.", acompananteId);
                datosVerificacion.MontoCobrado = 0;
            }

            var verificacion = new verificacione
            {
                agencia_id = agenciaId,
                acompanante_id = acompananteId,
                fecha_verificacion = DateTime.UtcNow,
                monto_cobrado = datosVerificacion.MontoCobrado,
                estado = "pendiente",
                observaciones = datosVerificacion.Observaciones
            };

            await _verificacionRepository.AddAsync(verificacion);
            await _verificacionRepository.SaveChangesAsync();

            _logger.LogInformation("Verificación creada con ID {VerificacionId} para acompañante {AcompananteId}", verificacion.id, acompananteId);

            var nuevoPago = new pago_verificacion
            {
                verificacion_id = verificacion.id,
                acompanante_id = acompananteId,
                agencia_id = agenciaId,
                monto = datosVerificacion.MontoCobrado,
                estado = datosVerificacion.MontoCobrado > 0 ? "pendiente" : "completado",
                fecha_pago = datosVerificacion.MontoCobrado > 0 ? null : DateTime.UtcNow
            };

            await _pagoVerificacionRepository.AddAsync(nuevoPago);
            await _pagoVerificacionRepository.SaveChangesAsync();

            _logger.LogInformation("Pago de verificación creado con ID {PagoId}. Estado: {Estado}, Monto: {Monto}", nuevoPago.id, nuevoPago.estado, nuevoPago.monto);

            // Procesar el pago real si el monto es mayor a 0
            if (datosVerificacion.MontoCobrado > 0)
            {
                var cliente = await _clienteRepository.GetByIdAsync(agencia.usuario_id);
                if (cliente == null)
                    throw new Exception("Cliente no encontrado para la agencia.");

                // Procesar el pago real con Stripe
                var transaccion = await _paymentService.ProcesarPagoCliente(
                    clienteId: cliente.id,
                    acompananteId: acompananteId,
                    monto: datosVerificacion.MontoCobrado,
                    paymentMethodId: datosVerificacion.MetodoPago
                );

                nuevoPago.estado = transaccion.estado;
                nuevoPago.referencia_pago = transaccion.id_transaccion_externa;
                nuevoPago.fecha_pago = transaccion.fecha_procesamiento;

                await _pagoVerificacionRepository.UpdateAsync(nuevoPago);
                await _pagoVerificacionRepository.SaveChangesAsync();

                if (transaccion.estado == "completado")
                {
                    // Marcar el acompañante como verificado
                    acompanante.esta_verificado = true;
                    acompanante.fecha_verificacion = DateTime.UtcNow;
                    await _acompananteRepository.UpdateAsync(acompanante);
                    await _acompananteRepository.SaveChangesAsync();

                    // Notificar al acompañante y a la agencia
                    if (acompanante.usuario?.email != null)
                    {
                        _logger.LogDebug("Enviando correo de verificación al acompañante {AcompananteId} a {Email}", acompananteId, acompanante.usuario.email);
                        await _emailProfesionalService.EnviarCorreoVerificacionAcompanante(
                            acompanante.usuario.email,
                            acompanante.nombre_perfil,
                            agencia.nombre
                        );
                    }

                    await _emailSender.SendEmailAsync(
                        agencia.usuario.email,
                        "Pago de verificación completado",
                        $"El pago de verificación del acompañante {acompanante.nombre_perfil} ha sido completado. Monto: ${datosVerificacion.MontoCobrado}"
                    );

                    // Otorgar puntos a la agencia
                    await OtorgarPuntosAgenciaAsync(new OtorgarPuntosAgenciaDto
                    {
                        AgenciaId = agenciaId,
                        Cantidad = 50,
                        Concepto = $"Verificación de acompañante: {acompanante.nombre_perfil}"
                    });

                    await AplicarComisionPorVerificacionAsync(agenciaId);
                }
            }
            else
            {
                // Si no hay monto cobrado, marcar como verificado directamente
                acompanante.esta_verificado = true;
                acompanante.fecha_verificacion = DateTime.UtcNow;
                await _acompananteRepository.UpdateAsync(acompanante);
                await _acompananteRepository.SaveChangesAsync();

                if (acompanante.usuario?.email != null)
                {
                    _logger.LogDebug("Enviando correo de verificación al acompañante {AcompananteId} a {Email}", acompananteId, acompanante.usuario.email);
                    await _emailProfesionalService.EnviarCorreoVerificacionAcompanante(
                        acompanante.usuario.email,
                        acompanante.nombre_perfil,
                        agencia.nombre
                    );
                }
            }

            _logger.LogInformation("Verificación completada para acompañante {AcompananteId}. Estado del acompañante actualizado a verificado.", acompananteId);

            return _mapper.Map<VerificacionDto>(verificacion);
        }

        public async Task<List<AcompananteDto>> GetAcompanantesVerificadosAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var acompanantes = await _agenciaRepository.GetAcompanantesVerificadosByAgenciaIdAsync(agenciaId);
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task<List<AcompananteDto>> GetAcompanantesPendientesVerificacionAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var todosAcompanantes = await _agenciaRepository.GetAcompanantesByAgenciaIdAsync(agenciaId);
            var pendientes = todosAcompanantes.Where(a => a.esta_verificado != true).ToList();

            return _mapper.Map<List<AcompananteDto>>(pendientes);
        }

        public async Task<AnuncioDestacadoDto> CrearAnuncioDestacadoAsync(CrearAnuncioDestacadoDto anuncioDto)
        {
            var usuarioId = ObtenerUsuarioId();
            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new UnauthorizedAccessException("No tienes una agencia asociada.");
            var agenciaId = agencia.id;

            _logger.LogDebug("Usuario {UsuarioId} autenticado como agencia {AgenciaId}", usuarioId, agenciaId);

            var acompanante = await _acompananteRepository.GetByIdAsync(anuncioDto.AcompananteId)
                ?? throw new NotFoundException("Acompañante", anuncioDto.AcompananteId);
            if (acompanante.agencia_id != agenciaId)
                throw new UnauthorizedAccessException("El acompañante no pertenece a esta agencia.");
            if (acompanante.esta_verificado != true)
                throw new BusinessRuleViolationException("El acompañante debe estar verificado para crear un anuncio destacado.");

            _logger.LogDebug("Acompañante {AcompananteId} validado para agencia {AgenciaId}. Verificado: {EstaVerificado}", anuncioDto.AcompananteId, agenciaId, acompanante.esta_verificado);

            var fechaInicioUtc = anuncioDto.FechaInicio.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(anuncioDto.FechaInicio, DateTimeKind.Utc)
                : anuncioDto.FechaInicio.ToUniversalTime();
            var fechaFinUtc = anuncioDto.FechaFin.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(anuncioDto.FechaFin, DateTimeKind.Utc)
                : anuncioDto.FechaFin.ToUniversalTime();

            if (fechaFinUtc <= fechaInicioUtc)
                throw new BusinessRuleViolationException("La fecha de fin debe ser posterior a la fecha de inicio.");

            var duracionDias = (fechaFinUtc - fechaInicioUtc).Days;
            if (duracionDias <= 0)
                throw new BusinessRuleViolationException("La duración del anuncio debe ser mayor a 0 días.");

            _logger.LogDebug("Duración del anuncio calculada: {DuracionDias} días (Inicio: {FechaInicio}, Fin: {FechaFin})", duracionDias, fechaInicioUtc, fechaFinUtc);

            if (anuncioDto.Tipo == TipoAnuncio.Portada)
            {
                if (duracionDias > 1)
                    throw new BusinessRuleViolationException("Los anuncios de tipo Portada solo pueden durar 1 día.");

                var anunciosPortadaActivos = await _anuncioRepository.GetActivosPorTipoYFechasAsync(
                    "portada", fechaInicioUtc, fechaFinUtc);
                if (anunciosPortadaActivos.Count >= 5)
                    throw new BusinessRuleViolationException("No hay espacio disponible en el carrusel para este período.");

                _logger.LogDebug("Anuncios activos de tipo Portada en el período: {Cantidad}", anunciosPortadaActivos.Count);
            }

            if (duracionDias > 30)
                throw new BusinessRuleViolationException("La duración del anuncio no puede exceder 30 días.");

            var costosParaTipo = _costos.Where(c => c.Tipo == anuncioDto.Tipo).ToList();
            if (!costosParaTipo.Any())
                throw new BusinessRuleViolationException($"No hay costos configurados para el tipo de anuncio {anuncioDto.Tipo}.");

            decimal montoFinal;
            string duracionTipo;

            if (duracionDias <= 1)
            {
                duracionTipo = "día";
                montoFinal = costosParaTipo.FirstOrDefault(c => c.Duracion == "día")?.Costo
                    ?? throw new BusinessRuleViolationException($"No hay costo configurado para {anuncioDto.Tipo} con duración de 1 día.");
            }
            else if (duracionDias <= 7)
            {
                duracionTipo = "semana";
                montoFinal = costosParaTipo.FirstOrDefault(c => c.Duracion == "semana")?.Costo
                    ?? costosParaTipo.FirstOrDefault(c => c.Duracion == "día")?.Costo * duracionDias
                    ?? throw new BusinessRuleViolationException($"No hay costo configurado para {anuncioDto.Tipo} con duración de {duracionDias} días.");
            }
            else
            {
                duracionTipo = "mes";
                montoFinal = costosParaTipo.FirstOrDefault(c => c.Duracion == "mes")?.Costo
                    ?? costosParaTipo.FirstOrDefault(c => c.Duracion == "semana")?.Costo * (duracionDias / 7m)
                    ?? costosParaTipo.FirstOrDefault(c => c.Duracion == "día")?.Costo * duracionDias
                    ?? throw new BusinessRuleViolationException($"No hay costo configurado para {anuncioDto.Tipo} con duración de {duracionDias} días.");
            }

            _logger.LogDebug("Costo calculado para {TipoAnuncio} con duración {DuracionTipo} ({DuracionDias} días): {MontoFinal}", anuncioDto.Tipo, duracionTipo, duracionDias, montoFinal);

            var cliente = await _clienteRepository.GetByIdAsync(agencia.usuario_id);
            if (cliente == null)
                throw new Exception("Cliente no encontrado para la agencia.");

            // Procesar el pago real con Stripe
            var transaccion = await _paymentService.ProcesarPagoCliente(
                clienteId: cliente.id,
                acompananteId: anuncioDto.AcompananteId,
                monto: montoFinal,
                paymentMethodId: anuncioDto.MetodoPago
            );

            anuncios_destacado nuevoAnuncio;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                nuevoAnuncio = new anuncios_destacado
                {
                    acompanante_id = anuncioDto.AcompananteId,
                    fecha_inicio = fechaInicioUtc,
                    fecha_fin = fechaFinUtc,
                    tipo = anuncioDto.Tipo switch
                    {
                        TipoAnuncio.Portada => "portada",
                        TipoAnuncio.PremiumTop => "top",
                        TipoAnuncio.Destacado => "destacado",
                        _ => throw new BusinessRuleViolationException($"Tipo de anuncio no soportado: {anuncioDto.Tipo}")
                    },
                    monto_pagado = montoFinal,
                    cupon_id = null, // No se usa cupón
                    esta_activo = transaccion.estado == "completado",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow,
                    payment_reference = transaccion.id_transaccion_externa
                };

                await _anuncioRepository.AddAsync(nuevoAnuncio);
                await _anuncioRepository.SaveChangesAsync();
                if (acompanante.esta_verificado != true)
                    throw new BusinessRuleViolationException("El acompañante debe estar verificado para crear un anuncio destacado.");

                _logger.LogInformation("Anuncio destacado creado con ID {AnuncioId} para el acompañante {AcompananteId}, estado: {Estado}", nuevoAnuncio.id, nuevoAnuncio.acompanante_id);

                scope.Complete();
            }

            var anuncioDestacadoDto = _mapper.Map<AnuncioDestacadoDto>(nuevoAnuncio);
            return anuncioDestacadoDto;
        }

        public async Task<List<AnuncioDestacadoDto>> GetAnunciosByAgenciaAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var anuncios = await _agenciaRepository.GetAnunciosDestacadosByAgenciaIdAsync(agenciaId);
            return _mapper.Map<List<AnuncioDestacadoDto>>(anuncios);
        }

        public async Task<AgenciaEstadisticasDto> GetEstadisticasAgenciaAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var estadisticas = await _agenciaRepository.GetAgenciaAcompanantesViewByIdAsync(agenciaId);

            if (estadisticas == null)
                throw new Exception("No se encontraron estadísticas para esta agencia.");

            return new AgenciaEstadisticasDto
            {
                AgenciaId = estadisticas.agencia_id ?? 0,
                NombreAgencia = estadisticas.agencia_nombre ?? string.Empty,
                EstaVerificada = estadisticas.agencia_verificada ?? false,
                TotalAcompanantes = estadisticas.total_acompanantes ?? 0,
                AcompanantesVerificados = estadisticas.acompanantes_verificados ?? 0,
                AcompanantesDisponibles = estadisticas.acompanantes_disponibles ?? 0
            };
        }

        public async Task<ComisionesDto> GetComisionesByAgenciaAsync(int agenciaId, DateTime fechaInicio, DateTime fechaFin)
        {
            await VerificarPermisosAgencia(agenciaId);

            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);

            if (agencia == null)
                throw new Exception("Agencia no encontrada.");

            var verificaciones = await _verificacionRepository.GetByAgenciaIdAndPeriodoAsync(agenciaId, fechaInicio, fechaFin);

            decimal comisionPorcentaje = await _agenciaRepository.GetComisionPorcentajeByAgenciaIdAsync(agenciaId);
            decimal totalVerificaciones = verificaciones.Sum(v => v.monto_cobrado ?? 0);
            decimal comisionTotal = totalVerificaciones * (comisionPorcentaje / 100);

            return new ComisionesDto
            {
                AgenciaId = agenciaId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                PorcentajeComision = comisionPorcentaje,
                TotalVerificaciones = verificaciones.Count,
                MontoTotalVerificaciones = totalVerificaciones,
                ComisionTotal = comisionTotal
            };
        }

        public async Task<bool> VerificarAgenciaAsync(int agenciaId, bool verificada)
        {
            if (!EsAdmin())
                throw new UnauthorizedAccessException("Solo los administradores pueden verificar agencias.");

            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            if (agencia == null)
                throw new Exception($"Agencia con ID {agenciaId} no encontrada.");

            if (agencia.esta_verificada == verificada)
                return verificada;

            agencia.esta_verificada = verificada;

            if (verificada)
            {
                _logger.LogInformation($"Verificando agencia ID {agenciaId}");

                agencia.fecha_verificacion = DateTime.UtcNow;
                agencia.comision_porcentaje = 5.00m;

                try
                {
                    if (agencia.usuario?.email != null)
                    {
                        await _emailProfesionalService.EnviarCorreoVerificacionAgencia(
                            agencia.usuario.email,
                            agencia.nombre
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar notificación de verificación a la agencia {AgenciaId}", agenciaId);
                }
            }
            else
            {
                _logger.LogInformation($"Quitando verificación a agencia ID {agenciaId}");

                agencia.fecha_verificacion = null;

                var acompanantes = await _agenciaRepository.GetAcompanantesVerificadosByAgenciaIdAsync(agenciaId);

                _logger.LogInformation($"Quitando verificación a {acompanantes.Count} acompañantes de la agencia {agenciaId}");

                foreach (var acompanante in acompanantes)
                {
                    acompanante.esta_verificado = false;
                    acompanante.fecha_verificacion = null;
                    await _acompananteRepository.UpdateAsync(acompanante);
                }

                await _verificacionRepository.DeleteByAgenciaIdAsync(agenciaId);

                try
                {
                    if (agencia.usuario?.email != null)
                    {
                        await _emailProfesionalService.EnviarNotificacionAdministrador(
                            agencia.usuario.email,
                            "Verificación de agencia revocada",
                            $"Se ha revocado la verificación de tu agencia <strong>{agencia.nombre}</strong>. " +
                            "Si crees que esto es un error, por favor contacta a nuestro equipo de soporte."
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar notificación de revocación a la agencia {AgenciaId}", agenciaId);
                }
            }

            await _agenciaRepository.UpdateAsync(agencia);
            await _agenciaRepository.SaveChangesAsync();

            _logger.LogInformation($"Estado de verificación de la agencia {agenciaId} actualizado a: {verificada}");

            return verificada;
        }

        public async Task<List<AgenciaPendienteVerificacionDto>> GetAgenciasPendientesVerificacionAsync()
        {
            if (!EsAdmin())
                throw new UnauthorizedAccessException("Solo los administradores pueden ver agencias pendientes de verificación.");

            var agencias = await _agenciaRepository.GetAgenciasPendientesVerificacionAsync();
            return _mapper.Map<List<AgenciaPendienteVerificacionDto>>(agencias);
        }

        private int ObtenerUsuarioId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                throw new UnauthorizedAccessException("No se pudo identificar al usuario.");

            return userId;
        }

        private bool EsAdmin()
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole("admin") ?? false;
        }

        private async Task VerificarPermisosAgencia(int agenciaId)
        {
            if (EsAdmin())
                return;

            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);

            if (agencia == null)
                throw new Exception("Agencia no encontrada.");

            if (agencia.usuario_id != ObtenerUsuarioId())
                throw new UnauthorizedAccessException("No tienes permisos para acceder a esta agencia.");
        }

        private async Task AplicarComisionPorVerificacionAsync(int agenciaId)
        {
            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            if (agencia == null)
                return;

            var verificados = await _agenciaRepository.GetAcompanantesVerificadosByAgenciaIdAsync(agenciaId);
            int cantidadVerificados = verificados.Count;

            decimal nuevaComision = agencia.comision_porcentaje ?? 0;
            decimal descuentoVerificaciones = 0;

            if (cantidadVerificados >= 50)
            {
                nuevaComision = 12.00m;
                descuentoVerificaciones = 25.00m;
            }
            else if (cantidadVerificados >= 25)
            {
                nuevaComision = 10.00m;
                descuentoVerificaciones = 20.00m;
            }
            else if (cantidadVerificados >= 10)
            {
                nuevaComision = 8.00m;
                descuentoVerificaciones = 15.00m;
            }

            if (nuevaComision > (agencia.comision_porcentaje ?? 0))
            {
                await _agenciaRepository.UpdateComisionPorcentajeAsync(agenciaId, nuevaComision);

                try
                {
                    var emailAgencia = agencia.usuario?.email;
                    if (!string.IsNullOrWhiteSpace(emailAgencia))
                    {
                        await _emailSender.SendEmailAsync(
                            emailAgencia,
                            "¡Mejora en tu comisión y descuentos!",
                            $"Tu porcentaje de comisión ha aumentado a {nuevaComision}% y ahora recibes un {descuentoVerificaciones}% de descuento en futuras verificaciones."
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al notificar el cambio de comisión a la agencia {AgenciaId}", agenciaId);
                }
            }
        }

        public async Task<List<AgenciaDisponibleDto>> GetAgenciasDisponiblesAsync()
        {
            var agencias = await _agenciaRepository.GetAllAsync();

            agencias = agencias.Where(a => a.esta_verificada == true).ToList();

            return agencias.Select(a => new AgenciaDisponibleDto
            {
                Id = a.id,
                Nombre = a.nombre,
                Ciudad = a.ciudad,
                Pais = a.pais,
                EstaVerificada = a.esta_verificada ?? false
            }).ToList();
        }

        public async Task<List<SolicitudAgenciaDto>> GetSolicitudesPendientesAsync()
        {
            var usuarioId = ObtenerUsuarioId();

            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new UnauthorizedAccessException("No tienes una agencia asociada.");

            var solicitudes = await _agenciaRepository.GetSolicitudesPendientesPorAgenciaAsync(agencia.id);

            return solicitudes.Select(s => new SolicitudAgenciaDto
            {
                Id = s.Id,
                AcompananteId = s.AcompananteId,
                AgenciaId = s.AgenciaId,
                Estado = s.Estado,
                FechaSolicitud = s.FechaSolicitud,
                FechaRespuesta = s.FechaRespuesta
            }).ToList();
        }

        public async Task AprobarSolicitudAsync(int solicitudId)
        {
            _logger.LogDebug("Aprobando solicitud {SolicitudId}", solicitudId);

            var solicitud = await _agenciaRepository.GetSolicitudByIdAsync(solicitudId)
                ?? throw new Exception("Solicitud no encontrada.");

            await VerificarPermisosAgencia(solicitud.AgenciaId);

            if (solicitud.Estado != "pendiente")
                throw new Exception("La solicitud ya ha sido procesada.");

            solicitud.Estado = "aprobada";
            solicitud.FechaRespuesta = DateTime.UtcNow;

            var acompanante = await _acompananteRepository.GetByIdAsync(solicitud.AcompananteId)
                ?? throw new Exception("Acompañante no encontrado.");

            acompanante.agencia_id = solicitud.AgenciaId;

            await _agenciaRepository.UpdateSolicitudAsync(solicitud);
            await _acompananteRepository.UpdateAsync(acompanante);
            await _agenciaRepository.SaveChangesAsync();
            await _acompananteRepository.SaveChangesAsync();

            _logger.LogInformation("Solicitud {SolicitudId} aprobada. Acompañante {AcompananteId} asignado a agencia {AgenciaId}", solicitudId, solicitud.AcompananteId, solicitud.AgenciaId);

            await NotificarSolicitudAsync(
                solicitud: solicitud,
                estado: "aprobada",
                mensajeAcompananteEmail: "Tu solicitud ha sido aprobada",
                mensajeAcompananteSignalR: $"Tu solicitud a la agencia '{solicitud.Agencia?.nombre}' ha sido aprobada 🎉.",
                mensajeAgenciaEmail: "Solicitud aprobada exitosamente",
                mensajeAgenciaEmailBody: EmailTemplates.SolicitudAprobadaAgencia(
                    solicitud.Agencia.nombre,
                    solicitud.Acompanante?.nombre_perfil ?? "acompañante")
            );
        }

        public async Task RechazarSolicitudAsync(int solicitudId)
        {
            var usuarioId = ObtenerUsuarioId();

            _logger.LogDebug("Rechazando solicitud {SolicitudId} por usuario {UsuarioId}", solicitudId, usuarioId);

            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new UnauthorizedAccessException("No tienes una agencia asociada.");

            var solicitud = await _agenciaRepository.GetSolicitudByIdAsync(solicitudId)
                ?? throw new Exception("Solicitud no encontrada.");

            if (solicitud.AgenciaId != agencia.id)
                throw new UnauthorizedAccessException("No puedes rechazar esta solicitud.");

            if (solicitud.Estado != "pendiente")
                throw new Exception("Esta solicitud ya fue procesada.");

            solicitud.Estado = "rechazada";
            solicitud.FechaRespuesta = DateTime.UtcNow;

            await _agenciaRepository.SaveChangesAsync();

            _logger.LogInformation("Solicitud {SolicitudId} rechazada por agencia {AgenciaId}", solicitudId, agencia.id);

            await NotificarSolicitudAsync(
                solicitud: solicitud,
                estado: "rechazada",
                mensajeAcompananteEmail: "La agencia ha decidido no aceptar tu solicitud en este momento.",
                mensajeAcompananteSignalR: null,
                mensajeAgenciaEmail: "Solicitud rechazada correctamente",
                mensajeAgenciaEmailBody: $"Has rechazado la solicitud de <strong>{solicitud.Acompanante?.nombre_perfil ?? "un acompañante"}</strong> para unirse a tu agencia. La solicitud ha sido marcada como rechazada y el acompañante ha sido notificado."
            );
        }

        public async Task EnviarSolicitudAsync(int agenciaId)
        {
            var usuarioId = ObtenerUsuarioId();

            var acompanante = await _acompananteRepository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new Exception("Perfil de acompañante no encontrado.");

            var yaExiste = await _agenciaRepository.ExisteSolicitudPendienteAsync(acompanante.id, agenciaId);
            if (yaExiste)
                throw new Exception("Ya tienes una solicitud pendiente con esta agencia.");

            var nuevaSolicitud = new SolicitudAgencia
            {
                AcompananteId = acompanante.id,
                AgenciaId = agenciaId,
                Estado = "pendiente",
                FechaSolicitud = DateTime.UtcNow
            };

            await _agenciaRepository.CrearSolicitudAsync(nuevaSolicitud);
            await _agenciaRepository.SaveChangesAsync();

            try
            {
                var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
                var emailDestino = agencia?.usuario?.email;

                if (!string.IsNullOrWhiteSpace(emailDestino))
                {
                    var asunto = "Nueva solicitud recibida";
                    var mensaje = EmailTemplates.NuevaSolicitudRecibida(agencia.nombre, acompanante.nombre_perfil);
                    await _emailSender.SendEmailAsync(emailDestino, asunto, mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación por email a la agencia ID {AgenciaId}", agenciaId);
            }
        }

        public async Task<PerfilEstadisticasDto?> GetEstadisticasPerfilAsync(int acompananteId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId)
                ?? throw new Exception("Acompañante no encontrado.");

            var usuarioId = ObtenerUsuarioId();

            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new UnauthorizedAccessException("No tienes una agencia asociada.");

            if (acompanante.agencia_id != agencia.id)
                throw new UnauthorizedAccessException("Este perfil no pertenece a tu agencia.");

            return await _acompananteRepository.GetEstadisticasPerfilAsync(acompananteId);
        }

        public async Task<int> GetAgenciaIdByUsuarioIdAsync(int usuarioId)
        {
            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId);
            return agencia?.id ?? 0;
        }

        public async Task<AgenciaDashboardDto> GetDashboardAsync(int agenciaId)
        {
            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            if (agencia == null)
                throw new NotFoundException($"Agencia con id {agenciaId} no encontrada");

            var totalAcompanantes = await _acompananteRepository.CountByAgenciaIdAsync(agenciaId);
            var totalVerificados = await _acompananteRepository.CountVerificadosByAgenciaIdAsync(agenciaId);
            var pendientesVerificacion = totalAcompanantes - totalVerificados;
            var solicitudesPendientes = await _solicitudAgenciaRepository.CountPendientesByAgenciaIdAsync(agenciaId);
            var anunciosActivos = await _anuncioRepository.CountActivosByAgenciaIdAsync(agenciaId);

            var fechaInicio = DateTime.Now.AddMonths(-1);
            var fechaFin = DateTime.Now;
            var comisiones = await _comisionRepository.GetByAgenciaIdAndFechasAsync(agenciaId, fechaInicio, fechaFin);
            decimal comisionesTotal = comisiones.Sum(c => c.Monto);

            var puntosAgencia = agencia.puntos_acumulados;

            var acompanantesDestacados = await _acompananteRepository.GetDestacadosByAgenciaIdAsync(agenciaId, 5);
            var acompanantesResumen = acompanantesDestacados.Select(a => new AcompananteResumenDto
            {
                Id = a.id,
                NombrePerfil = a.nombre_perfil,
                TotalVisitas = a.visitas_perfils.Count,
                TotalContactos = a.contactos.Count
            }).ToList();

            return new AgenciaDashboardDto
            {
                TotalAcompanantes = totalAcompanantes,
                TotalVerificados = totalVerificados,
                PendientesVerificacion = pendientesVerificacion,
                SolicitudesPendientes = solicitudesPendientes,
                AnunciosActivos = anunciosActivos,
                ComisionesUltimoMes = comisionesTotal,
                PuntosAcumulados = puntosAgencia,
                AcompanantesDestacados = acompanantesResumen
            };
        }

        public async Task<AcompanantesIndependientesResponseDto> GetAcompanantesIndependientesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string filterBy = null,
            string sortBy = "Id",
            bool sortDesc = false)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var resultado = await _acompananteRepository.GetIndependientesAsync(
                pageNumber, pageSize, filterBy, sortBy, sortDesc);

            var items = new List<AcompananteIndependienteDto>();

            foreach (var acompanante in resultado.Items)
            {
                string fotoUrl = "";
                if (acompanante.fotos != null && acompanante.fotos.Any())
                {
                    var fotoPrincipal = acompanante.fotos.FirstOrDefault(f => f.es_principal == true);
                    fotoUrl = fotoPrincipal?.url ?? "";
                }

                items.Add(new AcompananteIndependienteDto
                {
                    Id = acompanante.id,
                    NombrePerfil = acompanante.nombre_perfil,
                    Genero = acompanante.genero,
                    Edad = acompanante.edad,
                    Ciudad = acompanante.ciudad,
                    Pais = acompanante.pais,
                    FotoUrl = fotoUrl,
                    TarifaBase = acompanante.tarifa_base,
                    Moneda = acompanante.moneda,
                    EstaVerificado = acompanante.esta_verificado
                });
            }

            return new AcompanantesIndependientesResponseDto
            {
                TotalItems = resultado.TotalItems,
                TotalPages = resultado.TotalPages,
                PageSize = resultado.PageSize,
                CurrentPage = resultado.CurrentPage,
                Items = items
            };
        }

        public async Task<SolicitudesHistorialResponseDto> GetHistorialSolicitudesAsync(
            int agenciaId,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string estado = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var resultado = await _solicitudAgenciaRepository.GetHistorialAsync(
                agenciaId, null, fechaDesde, fechaHasta, estado, pageNumber, pageSize);

            var items = new List<SolicitudHistorialDto>();

            foreach (var solicitud in resultado.Items)
            {
                string fotoUrl = "";
                if (solicitud.acompanante?.fotos != null && solicitud.acompanante.fotos.Any())
                {
                    var fotoPrincipal = solicitud.acompanante.fotos
                        .FirstOrDefault(f => f.es_principal == true);
                    fotoUrl = fotoPrincipal?.url ?? "";
                }

                items.Add(new SolicitudHistorialDto
                {
                    Id = solicitud.id,
                    AcompananteId = solicitud.acompanante_id,
                    NombreAcompanante = solicitud.acompanante?.nombre_perfil ?? "Desconocido",
                    FotoAcompanante = fotoUrl,
                    AgenciaId = solicitud.agencia_id,
                    NombreAgencia = solicitud.agencia?.nombre ?? "Desconocido",
                    FechaSolicitud = solicitud.fecha_solicitud,
                    FechaRespuesta = solicitud.fecha_respuesta,
                    Estado = solicitud.estado,
                    MotivoRechazo = solicitud.motivo_rechazo,
                    MotivoCancelacion = solicitud.motivo_cancelacion
                });
            }

            return new SolicitudesHistorialResponseDto
            {
                TotalItems = resultado.TotalItems,
                TotalPages = resultado.TotalPages,
                PageSize = resultado.PageSize,
                CurrentPage = resultado.CurrentPage,
                Items = items
            };
        }

        public async Task<List<VerificacionDto>> VerificarAcompanantesLoteAsync(VerificacionLoteDto dto)
        {
            await VerificarPermisosAgencia(dto.AgenciaId);

            if (dto.AcompananteIds == null || !dto.AcompananteIds.Any())
                throw new ArgumentException("Debe proporcionar al menos un acompañante para verificar.");

            var agencia = await _agenciaRepository.GetByIdAsync(dto.AgenciaId);
            if (agencia == null)
                throw new Exception("Agencia no encontrada.");

            if (agencia.esta_verificada != true)
                throw new Exception("La agencia debe estar verificada para poder verificar acompañantes.");

            decimal descuento = 0;
            if (dto.AcompananteIds.Count >= 10)
                descuento = 0.25m;
            else if (dto.AcompananteIds.Count >= 5)
                descuento = 0.15m;
            else if (dto.AcompananteIds.Count >= 3)
                descuento = 0.10m;

            decimal montoUnitarioConDescuento = dto.MontoCobradoUnitario * (1 - descuento);

            var cliente = await _clienteRepository.GetByIdAsync(agencia.usuario_id);
            if (cliente == null)
                throw new Exception("Cliente no encontrado para la agencia.");

            var resultados = new List<VerificacionDto>();

            decimal montoTotal = montoUnitarioConDescuento * dto.AcompananteIds.Count;

            // Procesar el pago total para el lote
            transaccion transaccionLote = null;
            if (montoTotal > 0)
            {
                // Usamos el primer acompañante como referencia para el pago del lote
                transaccionLote = await _paymentService.ProcesarPagoCliente(
                    clienteId: cliente.id,
                    acompananteId: dto.AcompananteIds.First(),
                    monto: montoTotal,
                    paymentMethodId: dto.MetodoPago
                );
            }

            foreach (var acompananteId in dto.AcompananteIds)
            {
                try
                {
                    var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);

                    if (acompanante == null)
                        continue;

                    if (acompanante.agencia_id != dto.AgenciaId)
                        continue;

                    if (acompanante.esta_verificado == true)
                        continue;

                    var verificacion = new verificacione
                    {
                        agencia_id = dto.AgenciaId,
                        acompanante_id = acompananteId,
                        fecha_verificacion = DateTime.UtcNow,
                        monto_cobrado = montoUnitarioConDescuento,
                        estado = transaccionLote != null ? transaccionLote.estado : "completado",
                        observaciones = dto.Observaciones + $" (Verificación en lote con {descuento * 100}% de descuento)"
                    };

                    await _verificacionRepository.AddAsync(verificacion);
                    await _verificacionRepository.SaveChangesAsync();

                    var nuevoPago = new pago_verificacion
                    {
                        verificacion_id = verificacion.id,
                        acompanante_id = acompananteId,
                        agencia_id = dto.AgenciaId,
                        monto = montoUnitarioConDescuento,
                        estado = transaccionLote != null ? transaccionLote.estado : "completado",
                        fecha_pago = transaccionLote != null ? transaccionLote.fecha_procesamiento : DateTime.UtcNow,
                        referencia_pago = transaccionLote?.id_transaccion_externa
                    };

                    await _pagoVerificacionRepository.AddAsync(nuevoPago);
                    await _pagoVerificacionRepository.SaveChangesAsync();

                    if (transaccionLote == null || transaccionLote.estado == "completado")
                    {
                        acompanante.esta_verificado = true;
                        acompanante.fecha_verificacion = DateTime.UtcNow;
                        await _acompananteRepository.UpdateAsync(acompanante);
                        await _acompananteRepository.SaveChangesAsync();

                        if (acompanante.usuario?.email != null)
                        {
                            await _emailProfesionalService.EnviarCorreoVerificacionAcompanante(
                                acompanante.usuario.email,
                                acompanante.nombre_perfil,
                                agencia.nombre
                            );
                        }
                    }

                    resultados.Add(_mapper.Map<VerificacionDto>(verificacion));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar acompañante ID {AcompananteId} en lote", acompananteId);
                }
            }

            if (transaccionLote != null && transaccionLote.estado == "completado")
            {
                await OtorgarPuntosAgenciaAsync(new OtorgarPuntosAgenciaDto
                {
                    AgenciaId = dto.AgenciaId,
                    Cantidad = 50 * dto.AcompananteIds.Count,
                    Concepto = $"Verificación de {dto.AcompananteIds.Count} acompañantes en lote"
                });

                await AplicarComisionPorVerificacionAsync(dto.AgenciaId);
            }

            return resultados;
        }

        public async Task CancelarSolicitudAsync(int solicitudId, int usuarioId, string motivo)
        {
            var solicitud = await _solicitudAgenciaRepository.GetByIdAsync(solicitudId);
            if (solicitud == null)
                throw new NotFoundException($"Solicitud con ID {solicitudId} no encontrada");

            if (solicitud.estado != "pendiente")
                throw new InvalidOperationException("Solo se pueden cancelar solicitudes en estado pendiente");

            bool tienePermiso = false;

            var roles = await _usuarioRepository.GetRolesAsync(usuarioId);
            if (roles.Contains("admin"))
            {
                tienePermiso = true;
            }
            else
            {
                var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId);
                if (agencia != null && solicitud.agencia_id == agencia.id)
                {
                    tienePermiso = true;
                }
                else
                {
                    var acompanante = await _acompananteRepository.GetByUsuarioIdAsync(usuarioId);
                    if (acompanante != null && solicitud.acompanante_id == acompanante.id)
                    {
                        tienePermiso = true;
                    }
                }
            }

            if (!tienePermiso)
                throw new UnauthorizedAccessException("No tienes permisos para cancelar esta solicitud");

            solicitud.estado = "cancelada";
            solicitud.motivo_cancelacion = motivo;
            solicitud.fecha_respuesta = DateTime.UtcNow;

            await _solicitudAgenciaRepository.UpdateAsync(solicitud);
            await _solicitudAgenciaRepository.SaveChangesAsync();

            try
            {
                if (solicitud.agencia?.usuario?.email != null)
                {
                    await _emailSender.SendEmailAsync(
                        solicitud.agencia.usuario.email,
                        "Solicitud Cancelada",
                        $"La solicitud de {solicitud.acompanante?.nombre_perfil ?? "un acompañante"} ha sido cancelada.\n\nMotivo: {motivo}");
                }

                if (solicitud.acompanante?.usuario?.email != null)
                {
                    await _emailSender.SendEmailAsync(
                        solicitud.acompanante.usuario.email,
                        "Tu solicitud ha sido cancelada",
                        $"Tu solicitud a {solicitud.agencia?.nombre ?? "una agencia"} ha sido cancelada.\n\nMotivo: {motivo}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar correos de notificación para solicitud cancelada ID {SolicitudId}", solicitud.id);
            }

            try
            {
                if (solicitud.agencia?.usuario_id > 0)
                {
                    await _notificador.NotificarUsuarioAsync(
                        solicitud.agencia.usuario_id,
                        $"La solicitud del acompañante '{solicitud.acompanante?.nombre_perfil ?? "desconocido"}' fue cancelada. ❌",
                        "error"
                    );
                }

                if (solicitud.acompanante?.usuario_id > 0)
                {
                    await _notificador.NotificarUsuarioAsync(
                        solicitud.acompanante.usuario_id,
                        $"Tu solicitud a la agencia '{solicitud.agencia?.nombre ?? "desconocida"}' ha sido cancelada. ❌",
                        "error"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar notificación SignalR para solicitud cancelada ID {SolicitudId}", solicitud.id);
            }
        }

        public async Task CompletarPerfilAgenciaAsync(CompletarPerfilAgenciaDto dto)
        {
            var usuarioId = ObtenerUsuarioId();

            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new Exception("No tienes una agencia registrada.");

            if (string.IsNullOrWhiteSpace(dto.LogoUrl) || string.IsNullOrWhiteSpace(dto.SitioWeb))
            {
                throw new Exception("El logo y el sitio web son obligatorios para completar el perfil.");
            }

            agencia.logo_url = dto.LogoUrl;
            agencia.sitio_web = dto.SitioWeb;

            await _agenciaRepository.UpdateAsync(agencia);
            await _agenciaRepository.SaveChangesAsync();
        }

        public async Task<PuntosAgenciaDto> GetPuntosAgenciaAsync(int agenciaId)
        {
            await VerificarPermisosAgencia(agenciaId);

            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            if (agencia == null)
                throw new Exception("Agencia no encontrada.");

            var movimientos = await _agenciaRepository.GetUltimosMovimientosPuntosAsync(agenciaId, 10);

            return new PuntosAgenciaDto
            {
                AgenciaId = agenciaId,
                PuntosDisponibles = agencia.puntos_acumulados - agencia.puntos_gastados,
                PuntosGastados = agencia.puntos_gastados,
                UltimosMovimientos = _mapper.Map<List<MovimientoPuntosDto>>(movimientos)
            };
        }

        public async Task<int> OtorgarPuntosAgenciaAsync(OtorgarPuntosAgenciaDto dto)
        {
            var agencia = await _agenciaRepository.GetByIdAsync(dto.AgenciaId);
            if (agencia == null)
                throw new Exception("Agencia no encontrada.");

            int saldoAnterior = agencia.puntos_acumulados;
            agencia.puntos_acumulados += dto.Cantidad;

            var movimiento = new movimientos_puntos_agencia
            {
                agencia_id = dto.AgenciaId,
                cantidad = dto.Cantidad,
                tipo = "ingreso",
                concepto = dto.Concepto,
                saldo_anterior = saldoAnterior,
                saldo_nuevo = agencia.puntos_acumulados,
                fecha = DateTime.UtcNow
            };

            await _agenciaRepository.AddMovimientoPuntosAsync(movimiento);
            await _agenciaRepository.UpdateAsync(agencia);
            await _agenciaRepository.SaveChangesAsync();

            return agencia.puntos_acumulados;
        }

        public async Task<bool> GastarPuntosAgenciaAsync(int agenciaId, int puntos, string concepto)
        {
            await VerificarPermisosAgencia(agenciaId);

            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            if (agencia == null)
                throw new Exception("Agencia no encontrada.");

            int puntosDisponibles = agencia.puntos_acumulados - agencia.puntos_gastados;
            if (puntosDisponibles < puntos)
                throw new Exception($"Puntos insuficientes. Disponibles: {puntosDisponibles}, Solicitados: {puntos}");

            int saldoAnterior = agencia.puntos_gastados;
            agencia.puntos_gastados += puntos;

            var movimiento = new movimientos_puntos_agencia
            {
                agencia_id = agenciaId,
                cantidad = puntos,
                tipo = "gasto",
                concepto = concepto,
                saldo_anterior = saldoAnterior,
                saldo_nuevo = agencia.puntos_gastados,
                fecha = DateTime.UtcNow
            };

            await _agenciaRepository.AddMovimientoPuntosAsync(movimiento);
            await _agenciaRepository.UpdateAsync(agencia);
            await _agenciaRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> InvitarAcompananteAsync(int acompananteId)
        {
            var usuarioId = ObtenerUsuarioId();

            var agencia = await _agenciaRepository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new UnauthorizedAccessException("No tienes una agencia asociada.");

            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId)
                ?? throw new Exception("Acompañante no encontrado.");

            if (acompanante.agencia_id.HasValue)
                throw new Exception("El acompañante ya pertenece a una agencia.");

            var yaExiste = await _agenciaRepository.ExisteSolicitudPendienteAsync(acompanante.id, agencia.id);
            if (yaExiste)
                throw new Exception("Ya existe una solicitud pendiente para este acompañante.");

            var nuevaSolicitud = new SolicitudAgencia
            {
                AcompananteId = acompananteId,
                AgenciaId = agencia.id,
                Estado = "pendiente",
                FechaSolicitud = DateTime.UtcNow
            };

            await _agenciaRepository.CrearSolicitudAsync(nuevaSolicitud);
            await _agenciaRepository.SaveChangesAsync();

            try
            {
                var emailDestino = acompanante.usuario?.email;
                if (!string.IsNullOrWhiteSpace(emailDestino))
                {
                    await _emailProfesionalService.EnviarCorreoInvitacionAgencia(emailDestino, acompanante.nombre_perfil, agencia.nombre);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de invitación al acompañante ID {AcompananteId}", acompananteId);
            }

            try
            {
                await _notificador.NotificarUsuarioAsync(
                    acompanante.usuario_id,
                    $"Has recibido una invitación de la agencia '{agencia.nombre}' para unirte a su equipo.",
                    "info"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación SignalR al acompañante ID {AcompananteId}", acompananteId);
            }

            return true;
        }




        public async Task<bool> ConfirmarPagoVerificacionAsync(int pagoId, string referenciaPago)
        {
            var pago = await _pagoVerificacionRepository.GetByIdAsync(pagoId);
            if (pago == null)
                throw new NotFoundException("Pago de verificación", pagoId);

            if (pago.estado == "completado")
                throw new BusinessRuleViolationException("Este pago ya ha sido completado anteriormente");

            pago.estado = "completado";
            pago.fecha_pago = DateTime.UtcNow;
            pago.referencia_pago = referenciaPago;

            await _pagoVerificacionRepository.UpdateAsync(pago);
            await _pagoVerificacionRepository.SaveChangesAsync();

            var acompanante = await _acompananteRepository.GetByIdAsync(pago.acompanante_id);
            var usuario = acompanante?.usuario;

            if (usuario != null && !string.IsNullOrEmpty(usuario.email))
            {
                string nombreUsuario = "Usuario";

                if (acompanante != null && !string.IsNullOrEmpty(acompanante.nombre_perfil))
                {
                    nombreUsuario = acompanante.nombre_perfil;
                }

                await _emailProfesionalService.EnviarConfirmacionPago(
                    usuario.email,
                    nombreUsuario,
                    "Verificación de perfil",
                    pago.monto,
                    pago.id.ToString()
                );
            }

            try
            {
                var agencia = await _agenciaRepository.GetByIdAsync(pago.agencia_id);
                if (agencia?.usuario?.email != null)
                {
                    var asunto = "Pago de verificación completado";
                    var mensaje = $"El pago de verificación del acompañante {acompanante?.nombre_perfil ?? "desconocido"} ha sido completado. Monto: ${pago.monto}";
                    await _emailSender.SendEmailAsync(agencia.usuario.email, asunto, mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar a la agencia sobre el pago completado. PagoId: {PagoId}", pagoId);
            }

            _logger.LogInformation("Pago de verificación ID {PagoId} confirmado correctamente", pagoId);
            return true;
        }
        public async Task<AnuncioDestacadoDto> GetAnuncioByReferenceIdAsync(string referenceId)
        {
            var anuncio = await _anuncioRepository.GetByReferenceIdAsync(referenceId);
            return anuncio != null ? _mapper.Map<AnuncioDestacadoDto>(anuncio) : null;
        }

        public async Task UpdateAnuncioAsync(AnuncioDestacadoDto anuncio)
        {
            var anuncioEntity = await _anuncioRepository.GetByIdAsync(anuncio.Id);
            if (anuncioEntity != null)
            {
                anuncioEntity.esta_activo = anuncio.EstaActivo;
                anuncioEntity.updated_at = DateTime.UtcNow;
                await _anuncioRepository.UpdateAsync(anuncioEntity);
                await _anuncioRepository.SaveChangesAsync();
                _logger.LogInformation("Anuncio {AnuncioId} actualizado: EstaActivo={EstaActivo}", anuncio.Id, anuncio.EstaActivo);
            }
            else
            {
                _logger.LogWarning("No se encontró anuncio con ID {AnuncioId} para actualizar", anuncio.Id);
                throw new NotFoundException("Anuncio", anuncio.Id);
            }
        }

        public async Task<SolicitudAgenciaDto> GetSolicitudByIdAsync(int solicitudId)
        {
            _logger.LogInformation("Obteniendo solicitud con ID: {SolicitudId}", solicitudId);
            var solicitud = await _agenciaRepository.GetSolicitudByIdAsync(solicitudId);
            if (solicitud == null)
            {
                _logger.LogWarning("Solicitud con ID: {SolicitudId} no encontrada", solicitudId);
                return null;
            }
            _logger.LogInformation("Solicitud con ID: {SolicitudId} obtenida correctamente", solicitudId);
            return _mapper.Map<SolicitudAgenciaDto>(solicitud);
        }

        private async Task NotificarSolicitudAsync(
            SolicitudAgencia solicitud,
            string estado,
            string mensajeAcompananteEmail,
            string mensajeAcompananteSignalR,
            string mensajeAgenciaEmail,
        string mensajeAgenciaEmailBody)
        {
            // Notificación al acompañante (email)
            try
            {
                var emailDestinoAcompanante = solicitud.Acompanante?.usuario?.email;
                if (!string.IsNullOrWhiteSpace(emailDestinoAcompanante))
                {
                    if (estado == "aprobada")
                    {
                        await _emailProfesionalService.EnviarCorreoSolicitudAprobada(
                            emailDestinoAcompanante,
                            solicitud.Acompanante.nombre_perfil,
                            solicitud.Agencia?.nombre ?? "una agencia"
                        );
                    }
                    else if (estado == "rechazada")
                    {
                        await _emailProfesionalService.EnviarCorreoSolicitudRechazada(
                            emailDestinoAcompanante,
                            solicitud.Acompanante?.nombre_perfil ?? "Estimado usuario",
                            solicitud.Agencia?.nombre ?? "una agencia",
                            mensajeAcompananteEmail
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar email al acompañante ID {AcompananteId}", solicitud.AcompananteId);
            }

            // Notificación a la agencia (email)
            try
            {
                var emailDestinoAgencia = solicitud.Agencia?.usuario?.email;
                if (!string.IsNullOrWhiteSpace(emailDestinoAgencia))
                {
                    if (estado == "aprobada")
                    {
                        await _emailSender.SendEmailAsync(
                            emailDestinoAgencia,
                            mensajeAgenciaEmail,
                            mensajeAgenciaEmailBody
                        );
                    }
                    else if (estado == "rechazada")
                    {
                        await _emailProfesionalService.EnviarNotificacionAdministrador(
                            emailDestinoAgencia,
                            mensajeAgenciaEmail,
                            mensajeAgenciaEmailBody
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar email a la agencia ID {AgenciaId}", solicitud.AgenciaId);
            }

            // Notificación al acompañante (SignalR)
            if (!string.IsNullOrEmpty(mensajeAcompananteSignalR))
            {
                try
                {
                    await _notificador.NotificarUsuarioAsync(
                        solicitud.Acompanante.usuario_id,
                        mensajeAcompananteSignalR
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al enviar notificación SignalR al usuario ID {UsuarioId}", solicitud.Acompanante.usuario_id);
                }
            }


        }


        public async Task<bool> EsPropietarioAgenciaAsync(int agenciaId, int usuarioId)
        {
            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            return agencia != null && agencia.usuario_id == usuarioId;
        }

    }

}