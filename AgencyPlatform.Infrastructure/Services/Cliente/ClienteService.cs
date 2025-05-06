using AgencyPlatform.Application.Configuration;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.DTOs.Sorteos;
using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.Cliente;
using AgencyPlatform.Application.Interfaces.Services.ClienteCache;
using AgencyPlatform.Application.Interfaces.Services.EmailAgencia;
using AgencyPlatform.Application.Interfaces.Services.Recommendation;
using AgencyPlatform.Application.Validators;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Core.Exceptions;
using AgencyPlatform.Infrastructure.Services.EmailProfecional;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace AgencyPlatform.Infrastructure.Services.Cliente
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteService> _logger;

        private readonly ICuponClienteRepository _cuponRepository;

        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IContactoRepository _contactoRepository;
        private readonly IVisitaRepository _visitaRepository;
        private readonly ISorteoRepository _sorteoRepository;
        private readonly IParticipanteSorteoRepository _participanteRepository;
        private readonly IPaqueteCuponRepository _paqueteRepository;
        private readonly ICompraRepository _compraRepository;

        private readonly IMembresiaVipRepository _membresiaRepository;
        private readonly ISuscripcionVipRepository _suscripcionRepository;

        private readonly IPaymentService _paymentService;

        private readonly ClienteSettings _clienteSettings;

        private readonly IPuntosService _puntosService;

        private readonly IClienteCacheService _cacheService;
        private readonly IValidator<RegistroClienteDto> _registroValidator;
        private readonly IRecommendationService _recommendationService;
        private readonly IEmailService _emailService;
        private readonly IEmailProfesionalService _emailProfesionalService;








        public ClienteService(
                    IClienteRepository clienteRepository,
                    IUserRepository userRepository,
                    IMapper mapper,
                    ILogger<ClienteService> logger,
                    ICuponClienteRepository cuponRepository,
                    IContactoRepository contactorepositorio,
                    IAcompananteRepository acompananteRepository,
                    IVisitaRepository visitaRepository,
                    ISorteoRepository sorteoRepository,
                    IParticipanteSorteoRepository participanteRepository,
                    IPaqueteCuponRepository paqueteRepository,
                    ICompraRepository compraRepository,
                    IMembresiaVipRepository membresiaRepository,
                    ISuscripcionVipRepository suscripcionRepository,
                    IPaymentService paymentService,
                     ClienteSettings clienteSettings,
                   
                     IPuntosService puntosService,
                     IClienteCacheService cacheService,
                      IValidator<RegistroClienteDto> registroValidator,
                      IRecommendationService recommendationService,
                      IEmailService emailService, IEmailProfesionalService emailProfesionalService)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cuponRepository = cuponRepository ?? throw new ArgumentNullException(nameof(cuponRepository));
            _contactoRepository = contactorepositorio ?? throw new ArgumentNullException(nameof(contactorepositorio));
            _acompananteRepository = acompananteRepository ?? throw new ArgumentNullException(nameof(acompananteRepository));
            _visitaRepository = visitaRepository ?? throw new ArgumentNullException(nameof(visitaRepository));
            _sorteoRepository = sorteoRepository ?? throw new ArgumentNullException(nameof(sorteoRepository));
            _participanteRepository = participanteRepository ?? throw new ArgumentNullException(nameof(participanteRepository));
            _paqueteRepository = paqueteRepository ?? throw new ArgumentNullException(nameof(paqueteRepository));
            _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
            _membresiaRepository = membresiaRepository ?? throw new ArgumentNullException(nameof(membresiaRepository));
            _suscripcionRepository = suscripcionRepository ?? throw new ArgumentNullException(nameof(suscripcionRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _clienteSettings = clienteSettings ?? throw new ArgumentNullException(nameof(clienteSettings));
            _puntosService = puntosService ?? throw new ArgumentNullException(nameof(puntosService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _registroValidator = registroValidator ?? throw new ArgumentNullException(nameof(registroValidator));
            _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _emailProfesionalService = emailProfesionalService ?? throw new ArgumentNullException(nameof(emailProfesionalService));





        }

        // Añadir esta clase al inicio de la clase ClienteService (dentro de la clase, pero fuera de los métodos)



        public async Task<ClienteDto> RegistrarClienteAsync(RegistroClienteDto dto)
        {
            _logger.LogInformation("Iniciando registro de nuevo cliente con email: {Email}", dto.Email);

            // 1) Validación y normalización usando el validador dedicado
            _registroValidator.Validate(dto);

            // Guardamos email y nickname para el correo de bienvenida
            var toEmail = dto.Email;
            var nickname = dto.Nickname;

            // Ejecutamos la transacción de creación
            var clienteDto = await ExecuteInTransactionAsync(async () =>
            {
                // Validar que el email no exista
                var usuarioExistente = await _userRepository.GetByEmailAsync(dto.Email);
                if (usuarioExistente != null)
                {
                    _logger.LogWarning("Intento de registro con email ya existente: {Email}", dto.Email);
                    throw new DuplicateEntityException("Usuario", "email", dto.Email);
                }

                // Validar que el nickname no exista (si se proporciona)
                if (!string.IsNullOrEmpty(dto.Nickname))
                {
                    var existeNickname = await _clienteRepository.ExisteNicknameAsync(dto.Nickname);
                    if (existeNickname)
                    {
                        _logger.LogWarning("Intento de registro con nickname ya existente: {Nickname}", dto.Nickname);
                        throw new DuplicateEntityException("Cliente", "nickname", dto.Nickname);
                    }
                }

                // Crear usuario
                int rolClienteId = await _userRepository.GetRoleIdByNameAsync("cliente");
                if (rolClienteId == 0)
                {
                    _logger.LogError("No se encontró el rol 'cliente' durante el registro de usuario");
                    throw new NotFoundException("Rol", "cliente");
                }

                var nuevoUsuario = new usuario
                {
                    email = dto.Email,
                    password_hash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    rol_id = rolClienteId,
                    fecha_registro = DateTime.UtcNow,
                    esta_activo = true,
                    provider = "local",
                    password_required = true
                };
                await _userRepository.AddAsync(nuevoUsuario);
                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("Usuario creado correctamente. Id: {UsuarioId}, Email: {Email}", nuevoUsuario.id, nuevoUsuario.email);

                // Crear cliente
                var nuevoCliente = new cliente
                {
                    usuario_id = nuevoUsuario.id,
                    nickname = dto.Nickname,
                    es_vip = false,
                    puntos_acumulados = 0
                };
                await _clienteRepository.AddAsync(nuevoCliente);
                await _clienteRepository.SaveChangesAsync();
                _logger.LogInformation("Cliente creado correctamente. Id: {ClienteId}, Nickname: {Nickname}", nuevoCliente.id, nuevoCliente.nickname);

                // Otorgar puntos por registro si existe la acción
                try
                {
                    await _puntosService.OtorgarPuntosPorAccionAsync(nuevoCliente.id, "registro");
                    _logger.LogInformation("Puntos por registro otorgados al cliente {ClienteId}", nuevoCliente.id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al otorgar puntos por registro al cliente {ClienteId}", nuevoCliente.id);
                }

                _logger.LogInformation("Registro de cliente completado exitosamente. ClienteId: {ClienteId}", nuevoCliente.id);

                return _mapper.Map<ClienteDto>(nuevoCliente);
            });

            // Envío de correo de bienvenida (no altera el flujo si falla)
            try
            {
                var subject = "¡Bienvenido a AgencyPlatform!";
                var body = $"Hola {(string.IsNullOrEmpty(nickname) ? "" : nickname + ",\n\n")}¡Gracias por unirte a TeLoFundi🔥! " +
                           "Esperamos que disfrutes de nuestra plataforma.";
                await _emailService.SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de bienvenida a {Email}", toEmail);
            }

            return clienteDto;
        }
        public async Task<ClienteDto> GetByIdAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
                throw new NotFoundException("Cliente", id);

            return _mapper.Map<ClienteDto>(cliente);
        }
        public async Task<ClienteDto> GetByUsuarioIdAsync(int usuarioId)
        {
            var cliente = await _clienteRepository.GetByUsuarioIdAsync(usuarioId);
            if (cliente == null)
                throw new NotFoundException("Cliente", $"Usuario ID {usuarioId}");

            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<ClienteDetailDto> GetClienteDetailAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
                throw new NotFoundException("Cliente", id);

            return _mapper.Map<ClienteDetailDto>(cliente);
        }

        public async Task<ClienteDashboardDto> GetDashboardAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var dashboard = new ClienteDashboardDto
            {
                Cliente = _mapper.Map<ClienteDto>(cliente),
                PuntosDisponibles = cliente.puntos_acumulados ?? 0,
                CuponesDisponibles = await _cuponRepository.CountDisponiblesByClienteIdAsync(clienteId),
                TieneMembresiVip = cliente.es_vip ?? false,
                VencimientoVip = cliente.fecha_fin_vip
            };

            // Cargar perfiles visitados y recomendados usando el servicio de recomendación
            dashboard.PerfilesVisitadosRecentemente = await _recommendationService.GetPerfilesVisitadosRecientementeAsync(clienteId, 5);
            dashboard.PerfilesRecomendados = await _recommendationService.GetPerfilesRecomendadosAsync(clienteId, 5);

            return dashboard;
        }


        public async Task<List<MovimientoPuntosDto>> ObtenerHistorialPuntosAsync(int clienteId, int cantidad = 10)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            // Delegar la obtención del historial al servicio de puntos
            return await _puntosService.ObtenerHistorialPuntosAsync(clienteId, cantidad);
        }

        public async Task<List<CuponClienteDto>> ObtenerCuponesDisponiblesAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            // Usar el servicio de caché para obtener los cupones
            // La función lambda solo se ejecuta si los datos no están en caché
            return await _cacheService.GetCuponesDisponiblesAsync(clienteId, async () =>
            {
                _logger.LogDebug("Consultando cupones disponibles en base de datos. Cliente: {ClienteId}", clienteId);
                var cupones = await _cuponRepository.GetDisponiblesByClienteIdAsync(clienteId);
                var result = _mapper.Map<List<CuponClienteDto>>(cupones);
                return result;
            });
        }
        public async Task<bool> UsarCuponAsync(int clienteId, string codigo)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var cupon = await _cuponRepository.GetByCodigoAsync(codigo);
            if (cupon == null)
                throw new NotFoundException("Cupón", codigo);

            if (cupon.cliente_id != clienteId)
                throw new AccessDeniedException("No puedes usar un cupón que no te pertenece");

            if (cupon.esta_usado == true)
                throw new BusinessRuleViolationException("Este cupón ya ha sido utilizado");

            if (cupon.fecha_expiracion < DateTime.UtcNow)
                throw new BusinessRuleViolationException("Este cupón ha expirado");

            // Marcar como usado
            cupon.esta_usado = true;
            cupon.fecha_uso = DateTime.UtcNow;

            await _cuponRepository.UpdateAsync(cupon);
            await _cuponRepository.SaveChangesAsync();

            // Invalidar caché de cupones usando el servicio de caché
            _cacheService.InvalidarCacheCliente(clienteId);
            _logger.LogDebug("Caché de cupones invalidada para cliente {ClienteId} tras usar cupón", clienteId);

            return true;
        }

       

        public async Task<bool> RegistrarContactoPerfilAsync(int? clienteId, int acompananteId, string tipoContacto, string ipContacto)
        {
            // Verificar que el acompañante existe
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new NotFoundException("Acompañante", acompananteId);

            // Crear registro de contacto
            var contacto = new contacto
            {
                acompanante_id = acompananteId,
                cliente_id = clienteId, // Puede ser null (usuarios no registrados)
                tipo_contacto = tipoContacto,
                esta_registrado = clienteId.HasValue,
                ip_contacto = ipContacto,
                fecha_contacto = DateTime.UtcNow
            };

            await _contactoRepository.AddAsync(contacto);
            await _contactoRepository.SaveChangesAsync();

            // Si hay cliente registrado, otorgar puntos
            if (clienteId.HasValue)
            {
                try
                {
                    await _puntosService.OtorgarPuntosPorAccionAsync(clienteId.Value, "contacto_perfil");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al otorgar puntos por contacto al perfil {PerfilId} al cliente {ClienteId}", acompananteId, clienteId);
                    // Continuamos aunque no se puedan otorgar puntos
                }

                // Invalidar cachés relacionadas con el cliente
                _cacheService.InvalidarCacheCliente(clienteId.Value);
            }

            // También invalidar perfiles populares ya que puede cambiar el ranking
            _cacheService.InvalidarCachePerfilesPopulares();

            return true;
        }


        // Implementar método para registrar visitas
        public async Task RegistrarVisitaPerfilAsync(int? clienteId, int acompananteId, string ipVisitante, string userAgent)
        {
            // Verificar que el acompañante existe
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new NotFoundException("Acompañante", acompananteId);

            // Crear registro de visita
            var visita = new visitas_perfil
            {
                acompanante_id = acompananteId,
                cliente_id = clienteId, // Puede ser null (usuarios no registrados)
                ip_visitante = ipVisitante,
                user_agent = userAgent,
                fecha_visita = DateTime.UtcNow
            };

            await _visitaRepository.AddAsync(visita);
            await _visitaRepository.SaveChangesAsync();

            // Si hay cliente registrado, otorgar puntos
            if (clienteId.HasValue)
            {
                try
                {
                    await _puntosService.OtorgarPuntosPorAccionAsync(clienteId.Value, "visita_perfil");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al otorgar puntos por visita al perfil {PerfilId} al cliente {ClienteId}", acompananteId, clienteId);
                    // Continuamos aunque no se puedan otorgar puntos
                }

                // Invalidar cachés relacionadas con el cliente
                _cacheService.InvalidarCacheCliente(clienteId.Value);
            }

            // Invalidar caché de perfiles populares usando el servicio especializado
            _cacheService.InvalidarCachePerfilesPopulares();
        }
        public async Task<bool> ParticiparEnSorteoAsync(int clienteId, int sorteoId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var sorteo = await _sorteoRepository.GetByIdAsync(sorteoId);
            if (sorteo == null)
                throw new NotFoundException("Sorteo", sorteoId);

            if (sorteo.esta_activo != true)
                throw new BusinessRuleViolationException("El sorteo no está activo");

            var now = DateTime.UtcNow;
            if (now < sorteo.fecha_inicio || now > sorteo.fecha_fin)
                throw new BusinessRuleViolationException("El sorteo no está en curso");

            // Verificar si ya está participando
            var participacion = await _participanteRepository.GetByClienteYSorteoAsync(clienteId, sorteoId);
            if (participacion != null)
                throw new DuplicateEntityException("Participación", "cliente_sorteo", $"{clienteId}_{sorteoId}");

            // Crear participación
            var nuevaParticipacion = new participantes_sorteo
            {
                sorteo_id = sorteoId,
                cliente_id = clienteId,
                fecha_participacion = now,
                es_ganador = false
            };

            await _participanteRepository.AddAsync(nuevaParticipacion);
            await _participanteRepository.SaveChangesAsync();

            return true;
        }

        public async Task<List<SorteoDto>> GetSorteosParticipandoAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var participaciones = await _participanteRepository.GetByClienteIdAsync(clienteId);
            var sorteos = new List<SorteoDto>();

            foreach (var participacion in participaciones)
            {
                var sorteo = await _sorteoRepository.GetByIdAsync(participacion.sorteo_id);
                if (sorteo != null)
                {
                    var sorteoDto = _mapper.Map<SorteoDto>(sorteo);
                    sorteoDto.EstoyParticipando = true;
                    sorteoDto.TotalParticipantes = await _participanteRepository.CountBySorteoIdAsync(sorteo.id);
                    sorteos.Add(sorteoDto);
                }
            }

            return sorteos;
        }


        public async Task<CompraDto> ComprarPaqueteAsync(int clienteId, ComprarPaqueteDto dto)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                if (cliente == null)
                    throw new NotFoundException("Cliente", clienteId);

                var paquete = await _paqueteRepository.GetByIdAsync(dto.PaqueteId);
                if (paquete == null)
                    throw new NotFoundException("Paquete", dto.PaqueteId);

                // ⚠️ Comparación explícita con bool?
                if (paquete.activo != true)
                    throw new BusinessRuleViolationException("El paquete no está disponible para compra");

                // Confirmación de pago en Stripe
                if (dto.MetodoPago.Equals("stripe", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(dto.PaymentIntentId))
                {
                    var ok = await _paymentService.ConfirmPayment(dto.PaymentIntentId);
                    if (!ok)
                        throw new ExternalServiceException("Stripe", "El pago no ha sido completado correctamente");
                }

                var compra = new compras_paquete
                {
                    cliente_id = clienteId,
                    paquete_id = dto.PaqueteId,
                    fecha_compra = DateTime.UtcNow,
                    monto_pagado = paquete.precio,
                    metodo_pago = dto.MetodoPago,
                    referencia_pago = dto.PaymentIntentId ?? dto.ReferenciaPago,
                    estado = "completado"
                };

                await _compraRepository.AddAsync(compra);
                await _compraRepository.SaveChangesAsync();


                try
                {
                    if (paquete.puntos_otorgados > 0)
                    {
                        await _puntosService.OtorgarPuntosManualesAsync(
                            clienteId,
                            paquete.puntos_otorgados,
                            $"Compra de paquete: {paquete.nombre}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al otorgar puntos por compra de paquete al cliente {ClienteId}", clienteId);
                }

                // Generar cupones
                var cupones = await GenerarCuponesPorCompraAsync(compra.id);

                // Después de generar cupones
                if (paquete.incluye_sorteo == true)
                {
                    var sorteosActivos = await _sorteoRepository.GetActivosAsync();
                    var sorteoActivo = sorteosActivos.FirstOrDefault(); // Toma el primero si hay varios

                    if (sorteoActivo != null)
                    {
                        await _participanteRepository.AddAsync(new participantes_sorteo
                        {
                            cliente_id = clienteId,
                            sorteo_id = sorteoActivo.id,
                            fecha_participacion = DateTime.UtcNow,
                            es_ganador = false
                        });
                        await _participanteRepository.SaveChangesAsync();
                    }
                }

                // Invalidar caché de cupones usando el servicio de caché
                _cacheService.InvalidarCacheCliente(clienteId);

                // Enviar correo de confirmación usando el servicio profesional
                if (cliente.usuario?.email != null)
                {
                    var cuponesCodigos = cupones.Select(c => c.codigo).ToList();
                    await _emailProfesionalService.EnviarConfirmacionCompraPaquete(
                        cliente.usuario.email,
                        cliente.nickname ?? "Cliente",
                        paquete.nombre,
                        compra.monto_pagado,
                        paquete.puntos_otorgados,
                        cuponesCodigos
                    );
                }

                return _mapper.Map<CompraDto>(compra);
            });
        }
        private async Task<List<cupones_cliente>> GenerarCuponesPorCompraAsync(int compraId)
        {
            var compra = await _compraRepository.GetByIdAsync(compraId);
            if (compra == null)
                throw new NotFoundException("Compra", compraId);

            var paqueteDetalles = await _paqueteRepository.GetDetallesByPaqueteIdAsync(compra.paquete_id);
            if (paqueteDetalles == null || !paqueteDetalles.Any())
                return new List<cupones_cliente>(); // Retorna lista vacía si no hay detalles

            var cuponesGenerados = new List<cupones_cliente>();

            foreach (var detalle in paqueteDetalles)
            {
                for (int i = 0; i < detalle.cantidad; i++)
                {
                    // Generar código único
                    string codigo = $"CP-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

                    // Crear cupón
                    var cupon = new cupones_cliente
                    {
                        cliente_id = compra.cliente_id,
                        tipo_cupon_id = detalle.tipo_cupon_id,
                        codigo = codigo,
                        fecha_creacion = DateTime.UtcNow,
                        fecha_expiracion = DateTime.UtcNow.AddMonths(_clienteSettings.CuponesValidezMeses),
                        esta_usado = false
                    };

                    await _cuponRepository.AddAsync(cupon);
                    cuponesGenerados.Add(cupon);
                }
            }

            await _cuponRepository.SaveChangesAsync();
            return cuponesGenerados;
        }
        public async Task<List<CompraDto>> GetHistorialComprasAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var compras = await _compraRepository.GetByClienteIdAsync(clienteId);
            return _mapper.Map<List<CompraDto>>(compras);
        }

        public async Task<bool> SuscribirseVipAsync(int clienteId, SuscribirseVipDto dto)
        {
            // 1) Validaciones previas y creación en Stripe
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var membresia = await _membresiaRepository.GetByIdAsync(dto.MembresiaId);
            if (membresia == null)
                throw new NotFoundException("Membresía VIP", dto.MembresiaId);

            string stripeSubscriptionId = null;
            if (dto.MetodoPago.Equals("stripe", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(dto.PaymentMethodId))
            {
                stripeSubscriptionId = await _paymentService.CreateSubscription(
                    clienteId,
                    dto.MembresiaId,
                    dto.PaymentMethodId
                );
                if (string.IsNullOrEmpty(stripeSubscriptionId))
                    throw new ExternalServiceException("Stripe", "No se pudo crear la suscripción en Stripe");

                dto.ReferenciaPago = stripeSubscriptionId;
            }

            // 2) Inicia transacción para todas las operaciones en BD
            return await ExecuteInTransactionAsync(async () =>
            {
                // 2.1) Cancelar suscripción activa en BD y Stripe
                var suscripcionActiva = await _suscripcionRepository.GetActivaByClienteIdAsync(clienteId);
                if (suscripcionActiva != null)
                {
                    suscripcionActiva.estado = "cancelada";
                    await _suscripcionRepository.UpdateAsync(suscripcionActiva);

                    if (suscripcionActiva.metodo_pago.Equals("stripe", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(suscripcionActiva.referencia_pago))
                    {
                        await _paymentService.CancelSubscription(suscripcionActiva.referencia_pago);
                    }
                }

                // 2.2) Crear nueva suscripción
                var fechaInicio = DateTime.UtcNow;
                var fechaFin = fechaInicio.AddMonths(_clienteSettings.SuscripcionDuracionMeses);
                var suscripcion = new suscripciones_vip
                {
                    cliente_id = clienteId,
                    membresia_id = dto.MembresiaId,
                    fecha_inicio = fechaInicio,
                    fecha_fin = fechaFin,
                    estado = "activa",
                    metodo_pago = dto.MetodoPago,
                    referencia_pago = dto.ReferenciaPago,
                    es_renovacion_automatica = dto.RenovacionAutomatica
                };

                // 2.3) Actualizar cliente como VIP
                cliente.es_vip = true;
                cliente.fecha_inicio_vip = fechaInicio;
                cliente.fecha_fin_vip = fechaFin;

                // 2.4) Actualizar rol del usuario
                var usuario = await _userRepository.GetByIdAsync(cliente.usuario_id);
                if (usuario != null)
                {
                    var rolClienteVip = await _userRepository.GetRoleIdByNameAsync("cliente_vip");
                    if (rolClienteVip != 0)
                    {
                        usuario.rol_id = rolClienteVip;
                        await _userRepository.UpdateAsync(usuario);
                        await _userRepository.SaveChangesAsync();
                    }
                }

                // 2.5) Persistir suscripción y cliente
                await _suscripcionRepository.AddAsync(suscripcion);
                await _clienteRepository.UpdateAsync(cliente);
                await _suscripcionRepository.SaveChangesAsync();
                await _clienteRepository.SaveChangesAsync();

                // 2.6) Otorgar puntos mensuales
                if (membresia.puntos_mensuales > 0)
                {
                    try
                    {
                        await _puntosService.OtorgarPuntosManualesAsync(
                            clienteId,
                            membresia.puntos_mensuales,
                            $"Puntos mensuales membresía VIP {membresia.nombre}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al otorgar puntos mensuales de membresía VIP al cliente {ClienteId}", clienteId);
                    }
                }

                // 2.7) Notificar al cliente sobre la suscripción VIP
                if (cliente.usuario?.email != null)
                {
                    await _emailProfesionalService.EnviarConfirmacionSuscripcionVIP(
                        cliente.usuario.email,
                        cliente.nickname ?? "Cliente",
                        membresia.nombre,
                        membresia.precio_mensual,
                        fechaFin.ToString("dd/MM/yyyy")
                    );
                }

                // Invalidar cachés del cliente usando el servicio de caché
                _cacheService.InvalidarCacheCliente(clienteId);

                // También invalidar cachés de perfiles populares
                _cacheService.InvalidarCachePerfilesPopulares();

                _logger.LogDebug("Cachés invalidadas tras suscripción VIP del cliente {ClienteId}", clienteId);

                return true;
            });
        }
        public async Task<bool> CancelarSuscripcionVipAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var suscripcionActiva = await _suscripcionRepository.GetActivaByClienteIdAsync(clienteId);
            if (suscripcionActiva == null)
                throw new BusinessRuleViolationException("El cliente no tiene una suscripción VIP activa");

            // Cancelar la suscripción
            suscripcionActiva.estado = "cancelada";
            suscripcionActiva.es_renovacion_automatica = false;

            // Mantener el estado VIP hasta la fecha de fin
            // El estado VIP se actualizará automáticamente mediante un proceso programado
            await _suscripcionRepository.UpdateAsync(suscripcionActiva);
            await _suscripcionRepository.SaveChangesAsync();

            // Invalidar cachés del cliente usando el servicio de caché
            _logger.LogDebug("Invalidando cachés tras cancelación de suscripción VIP del cliente {ClienteId}", clienteId);
            _cacheService.InvalidarCacheCliente(clienteId);

            return true;
        }
        public async Task<bool> EsClienteVipAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            return cliente.es_vip == true;
        }

        public async Task<List<AcompananteCardDto>> GetPerfilesVisitadosRecientementeAsync(int clienteId, int cantidad = 5)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            // 1. Obtenemos los IDs de perfiles visitados con una sola consulta
            var perfilesIds = await _visitaRepository.GetPerfilesVisitadosRecientementeIdsByClienteAsync(clienteId, cantidad);
            if (!perfilesIds.Any())
                return new List<AcompananteCardDto>();

            // 2. Cargamos todos los perfiles en una sola consulta
            var acompanantes = await _acompananteRepository.GetByIdsAsync(perfilesIds);

            // 3. Cargamos estadísticas en operaciones por lotes
            var estadisticas = await _acompananteRepository.GetEstadisticasMultiplesAsync(perfilesIds);

            // 4. Mapeamos a DTOs usando datos precargados
            var perfiles = new List<AcompananteCardDto>();
            foreach (var acompanante in acompanantes.Where(a => a.esta_disponible == true))
            {
                var perfilDto = _mapper.Map<AcompananteCardDto>(acompanante);

                // Usar estadísticas precargadas
                if (estadisticas.TryGetValue(acompanante.id, out var stats))
                {
                    perfilDto.TotalVisitas = stats.TotalVisitas;
                    perfilDto.TotalContactos = stats.TotalContactos;
                    perfilDto.FotoPrincipalUrl = stats.FotoPrincipalUrl;
                }

                perfiles.Add(perfilDto);
                if (perfiles.Count >= cantidad)
                    break;
            }

            return perfiles;
        }
        public async Task<List<AcompananteCardDto>> GetPerfilesRecomendadosAsync(int clienteId, int cantidad = 5)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            // 1. Obtener perfiles visitados de forma optimizada
            var perfilesVisitadosIds = await _acompananteRepository.GetPerfilesVisitadosIdsByClienteAsync(clienteId);
            if (!perfilesVisitadosIds.Any())
            {
                // Si no hay historial, devolver populares directamente
                return await GetPerfilesPopularesAsync(cantidad);
            }

            // 2. Obtener intereses (categorías y ciudades) usando métodos con caché
            var categoriasInteres = await GetCategoriasInteresAsync(clienteId, perfilesVisitadosIds);
            var ciudadesInteres = await GetCiudadesInteresAsync(clienteId, perfilesVisitadosIds);

            // 3. Obtener candidatos a recomendar (sin consultas individuales)
            var candidatosIds = new List<int>();

            // Perfiles por categorías
            if (categoriasInteres.Any())
            {
                var perfilesPorCategoriaIds = await _acompananteRepository.GetIdsByCategoriasAsync(
                    categoriasInteres, 20, perfilesVisitadosIds);
                candidatosIds.AddRange(perfilesPorCategoriaIds);
            }

            // Perfiles por ciudades
            if (ciudadesInteres.Any() && candidatosIds.Count < cantidad * 2)
            {
                var perfilesPorCiudadIds = await _acompananteRepository.GetIdsByCiudadesAsync(
                    ciudadesInteres, 20, perfilesVisitadosIds);
                candidatosIds.AddRange(perfilesPorCiudadIds.Where(id => !candidatosIds.Contains(id)));
            }

            // Si no hay suficientes, agregar populares
            if (candidatosIds.Count < cantidad)
            {
                var perfilesPopularesIds = await _acompananteRepository.GetIdsPopularesAsync(
                    20, perfilesVisitadosIds);
                candidatosIds.AddRange(perfilesPopularesIds.Where(id => !candidatosIds.Contains(id)));
            }

            // 4. Limitar candidatos y obtener datos completos en pocas consultas
            candidatosIds = candidatosIds.Take(cantidad).ToList();
            if (!candidatosIds.Any())
                return new List<AcompananteCardDto>();

            var acompanantes = await _acompananteRepository.GetByIdsAsync(candidatosIds);
            var estadisticas = await _acompananteRepository.GetEstadisticasMultiplesAsync(candidatosIds);

            // Precargamos todas las categorías de los perfiles candidatos en una sola consulta
            var todasLasCategorias = new Dictionary<int, List<int>>();
            foreach (var id in candidatosIds)
            {
                todasLasCategorias[id] = await _acompananteRepository.GetCategoriasIdsDePerfilAsync(id);
            }

            // 5. Mapear resultados con información precargada
            var resultado = new List<AcompananteCardDto>();
            foreach (var acompanante in acompanantes.Where(a => a.esta_disponible == true))
            {
                var perfilDto = _mapper.Map<AcompananteCardDto>(acompanante);

                // Usar estadísticas precargadas
                if (estadisticas.TryGetValue(acompanante.id, out var stats))
                {
                    perfilDto.TotalVisitas = stats.TotalVisitas;
                    perfilDto.TotalContactos = stats.TotalContactos;
                    perfilDto.FotoPrincipalUrl = stats.FotoPrincipalUrl;
                }

                // Determinar razón de recomendación (sin consultas adicionales)
                var categoriasPerfil = todasLasCategorias[acompanante.id];
                if (categoriasInteres.Any() && categoriasPerfil.Any(c => categoriasInteres.Contains(c)))
                {
                    perfilDto.RazonRecomendacion = "Basado en tus intereses";
                }
                else if (ciudadesInteres.Contains(acompanante.ciudad))
                {
                    perfilDto.RazonRecomendacion = $"En {acompanante.ciudad}";
                }
                else
                {
                    perfilDto.RazonRecomendacion = "Popular en la plataforma";
                }

                resultado.Add(perfilDto);
            }

            _logger.LogDebug("Recomendaciones generadas para cliente {ClienteId}. Cantidad: {Cantidad}", clienteId, resultado.Count);
            return resultado;
        }

        private async Task<List<AcompananteCardDto>> GetPerfilesPopularesAsync(int cantidad)
        {
            // Usar el servicio de caché para obtener perfiles populares
            return await _cacheService.GetPerfilesPopularesAsync(cantidad, async () =>
            {
                // Esta función lambda se ejecutará solo si los datos no están en caché
                _logger.LogDebug("Consultando perfiles populares en base de datos. Cantidad: {Cantidad}", cantidad);

                var perfilesIds = await _acompananteRepository.GetIdsPopularesAsync(cantidad);
                var acompanantes = await _acompananteRepository.GetByIdsAsync(perfilesIds);
                var estadisticas = await _acompananteRepository.GetEstadisticasMultiplesAsync(perfilesIds);

                var resultado = new List<AcompananteCardDto>();
                foreach (var acompanante in acompanantes.Where(a => a.esta_disponible == true))
                {
                    var perfilDto = _mapper.Map<AcompananteCardDto>(acompanante);

                    if (estadisticas.TryGetValue(acompanante.id, out var stats))
                    {
                        perfilDto.TotalVisitas = stats.TotalVisitas;
                        perfilDto.TotalContactos = stats.TotalContactos;
                        perfilDto.FotoPrincipalUrl = stats.FotoPrincipalUrl;
                    }

                    perfilDto.RazonRecomendacion = "Popular en la plataforma";
                    resultado.Add(perfilDto);
                }

                return resultado;
            });
        }


        private async Task<List<int>> GetCategoriasInteresAsync(int clienteId, List<int> perfilesVisitadosIds)
        {
            // Usar el servicio de caché para obtener categorías de interés
            return await _cacheService.GetCategoriasInteresAsync(
                clienteId,
                perfilesVisitadosIds,
                async (ids) =>
                {
                    // Esta función lambda se ejecutará solo si los datos no están en caché
                    return await _acompananteRepository.GetCategoriasDePerfilesAsync(ids);
                });
        }
        private async Task<List<string>> GetCiudadesInteresAsync(int clienteId, List<int> perfilesVisitadosIds)
        {
            // Usar el servicio de caché para obtener ciudades de interés
            return await _cacheService.GetCiudadesInteresAsync(
                clienteId,
                perfilesVisitadosIds,
                async (ids) =>
                {
                    // Esta función lambda se ejecutará solo si los datos no están en caché
                    return await _acompananteRepository.GetCiudadesDePerfilesAsync(ids);
                });
        }

        private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                T result = await operation();
                scope.Complete();
                return result;
            }
        }

        public async Task UpdateAsync(cliente cliente)
        {
            try
            {
                await _clienteRepository.UpdateAsync(cliente);
                await _clienteRepository.SaveChangesAsync();
                _logger.LogInformation("Cliente actualizado correctamente: ClienteId={ClienteId}", cliente.id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente: ClienteId={ClienteId}", cliente.id);
                throw;
            }
        }

        private async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await operation();
                scope.Complete();
            }
        }




    }
}

