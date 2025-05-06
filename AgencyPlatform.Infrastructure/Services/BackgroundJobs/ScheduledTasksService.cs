using AgencyPlatform.Application.DTOs.Jobs;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.ScheduledTasks;
using AgencyPlatform.Application.Interfaces.Services.BackgroundJob;
using AgencyPlatform.Application.Interfaces.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Common;
using Hangfire.States;
using AgencyPlatform.Application.Interfaces.Services;

namespace AgencyPlatform.Infrastructure.Services.BackgroundJobs
{
    public class ScheduledTasksService : IScheduledTasksService
    {
       
        private readonly IIntentoLoginRepository _intentoLoginRepository;
        private readonly ISuscripcionVipRepository _suscripcionVipRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IAnuncioDestacadoRepository _anuncioDestacadoRepository;
        private readonly ICuponClienteRepository _cuponClienteRepository;
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly ISolicitudAgenciaRepository _solicitudAgenciaRepository;
        private readonly IEmailSender _emailSender;
        private readonly INotificadorRealTime _notificador;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly ILogger<ScheduledTasksService> _logger;

        public ScheduledTasksService(
           
            IIntentoLoginRepository intentoLoginRepository,
            ISuscripcionVipRepository suscripcionVipRepository,
            IClienteRepository clienteRepository,
            IAnuncioDestacadoRepository anuncioDestacadoRepository,
            ICuponClienteRepository cuponClienteRepository,
            IAcompananteRepository acompananteRepository,
            ISolicitudAgenciaRepository solicitudAgenciaRepository,
            IEmailSender emailSender,
            INotificadorRealTime notificador,
            IBackgroundJobService backgroundJobService,
            ILogger<ScheduledTasksService> logger)
        {
            
            _intentoLoginRepository = intentoLoginRepository;
            _suscripcionVipRepository = suscripcionVipRepository;
            _clienteRepository = clienteRepository;
            _anuncioDestacadoRepository = anuncioDestacadoRepository;
            _cuponClienteRepository = cuponClienteRepository;
            _acompananteRepository = acompananteRepository;
            _solicitudAgenciaRepository = solicitudAgenciaRepository;
            _emailSender = emailSender;
            _notificador = notificador;
            _backgroundJobService = backgroundJobService;
            _logger = logger;
        }

   


        public Task InitializeScheduledTasksAsync()
        {
            _logger.LogInformation("Inicializando tareas programadas...");

            try
            {
                // Eliminamos la tarea de limpieza de tokens ya que no tenemos la interfaz
                // _backgroundJobService.RecurringJob<IScheduledTasksService>(
                //     x => x.LimpiarTokensVencidosAsync(),
                //     "0 3 * * *",
                //     "limpiar-tokens-vencidos");

                // Limpieza de intentos de login cada 12 horas
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.LimpiarIntentosLoginAntiguosAsync(),
                    "0 */12 * * *",
                    "limpiar-intentos-login");

                // Actualización de estados VIP cada hora
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.ActualizarEstadoMembresiasVipAsync(),
                    "0 * * * *",
                    "actualizar-estado-vip");

                // Actualización de estados de anuncios destacados cada hora
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.ActualizarEstadoAnunciosDestacadosAsync(),
                    "15 * * * *",
                    "actualizar-anuncios-destacados");

                // Limpieza de cupones vencidos cada día a las 4:00 AM
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.LimpiarCuponesVencidosAsync(),
                    "0 4 * * *",
                    "limpiar-cupones-vencidos");

                // Actualización de scores de actividad cada 3 horas
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.ActualizarScoresActividadAsync(),
                    "0 */3 * * *",
                    "actualizar-scores-actividad");

                // Actualización de rankings cada 6 horas
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.ActualizarRankingsPerfilesAsync(),
                    "0 */6 * * *",
                    "actualizar-rankings");

                // Recordatorios de renovación de membresías cada día a las 10:00 AM
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.EnviarRecordatoriosRenovacionMembresiasAsync(),
                    "0 10 * * *",
                    "recordatorios-renovacion");

                // Procesamiento de solicitudes pendientes antiguas cada día a las 9:00 AM
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.ProcesarSolicitudesPendientesAntiguasAsync(),
                    "0 9 * * *",
                    "procesar-solicitudes-antiguas");

                // Generación de informes diarios a las 5:00 AM
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.GenerarInformesPeriodicosAsync("diario"),
                    "0 5 * * *",
                    "informes-diarios");

                // Generación de informes semanales los lunes a las 6:00 AM
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.GenerarInformesPeriodicosAsync("semanal"),
                    "0 6 * * 1",
                    "informes-semanales");

                // Generación de informes mensuales el día 1 de cada mes a las 7:00 AM
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.GenerarInformesPeriodicosAsync("mensual"),
                    "0 7 1 * *",
                    "informes-mensuales");

                // Desactivación de perfiles inactivos cada semana los domingos a las 1:00 AM
                _backgroundJobService.RecurringJob<IScheduledTasksService>(
                    x => x.DesactivarPerfilesInactivosAsync(),
                    "0 1 * * 0",
                    "desactivar-perfiles-inactivos");

                _logger.LogInformation("Tareas programadas inicializadas correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar tareas programadas");
                throw;
            }

            return Task.CompletedTask;
        }

        public async Task LimpiarIntentosLoginAntiguosAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando limpieza de intentos de login antiguos");
                var fechaLimite = DateTime.UtcNow.AddDays(-7); // Intentos más antiguos de 7 días

                int intentosEliminados = await _intentoLoginRepository.EliminarIntentosAntiguosAsync(fechaLimite);

                _logger.LogInformation("Limpieza de intentos de login completada. Intentos eliminados: {Intentos}", intentosEliminados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar intentos de login antiguos");
                throw;
            }
        }

        public async Task ActualizarEstadoMembresiasVipAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de estados de membresías VIP");
                var ahora = DateTime.UtcNow;

                // Obtener suscripciones activas que han vencido
                // Obtener suscripciones activas que han vencido
                var suscripcionesVencidas = await _suscripcionVipRepository.GetSuscripcionesVencidasActivasAsync(ahora);

                int clientesActualizados = 0;

                foreach (var suscripcion in suscripcionesVencidas)
                {
                    try
                    {
                        // Cambiar estado de la suscripción a "vencida"
                        suscripcion.estado = "vencida";
                        await _suscripcionVipRepository.UpdateAsync(suscripcion);

                        // Actualizar estado VIP del cliente
                        if (suscripcion.cliente_id > 0)
                        {
                            var cliente = await _clienteRepository.GetByIdAsync(suscripcion.cliente_id);
                            if (cliente != null)
                            {
                                cliente.es_vip = false;
                                cliente.fecha_fin_vip = null;
                                await _clienteRepository.UpdateAsync(cliente);
                                clientesActualizados++;

                                // Notificar al cliente
                                await _notificador.NotificarUsuarioAsync(
                                    cliente.usuario_id,
                                    "Tu membresía VIP ha vencido. ¡Renuévala para seguir disfrutando de los beneficios!"
                                );

                                // Enviar email
                                if (cliente.usuario?.email != null)
                                {
                                    await _emailSender.SendEmailAsync(
                                        cliente.usuario.email,
                                        "Tu membresía VIP ha vencido",
                                        "Hola,\n\nTu membresía VIP ha vencido. Renuévala ahora para seguir disfrutando de todos los beneficios exclusivos.\n\nSaludos,\nEl equipo de la plataforma"
                                    );
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar suscripción vencida ID: {SuscripcionId}", suscripcion.id);
                        // Continuar con la siguiente suscripción
                    }
                }

                // Procesar renovaciones automáticas
                var suscripcionesPorRenovar = await _suscripcionVipRepository.GetSuscripcionesPorRenovarAsync(ahora);
                int renovacionesExitosas = 0;

                foreach (var suscripcion in suscripcionesPorRenovar)
                {
                    try
                    {
                        // Aquí se implementaría la lógica de cobro automático
                        // Para este ejemplo, simplemente extendemos la fecha

                        // Extender suscripción por un mes
                        suscripcion.fecha_inicio = ahora;
                        suscripcion.fecha_fin = ahora.AddMonths(1);
                        await _suscripcionVipRepository.UpdateAsync(suscripcion);

                        // Actualizar estado VIP del cliente
                        if (suscripcion.cliente_id > 0)
                        {
                            var cliente = await _clienteRepository.GetByIdAsync(suscripcion.cliente_id);
                            if (cliente != null)
                            {
                                cliente.es_vip = true;
                                cliente.fecha_inicio_vip = ahora;
                                cliente.fecha_fin_vip = ahora.AddMonths(1);
                                await _clienteRepository.UpdateAsync(cliente);
                                renovacionesExitosas++;

                                // Notificar al cliente
                                await _notificador.NotificarUsuarioAsync(
                                    cliente.usuario_id,
                                    "¡Tu membresía VIP ha sido renovada automáticamente!"
                                );

                                // Enviar email
                                if (cliente.usuario?.email != null)
                                {
                                    await _emailSender.SendEmailAsync(
                                        cliente.usuario.email,
                                        "Renovación automática de membresía VIP",
                                        "Hola,\n\nTu membresía VIP ha sido renovada automáticamente. Seguirás disfrutando de todos los beneficios exclusivos.\n\nSaludos,\nEl equipo de la plataforma"
                                    );
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al renovar automáticamente suscripción ID: {SuscripcionId}", suscripcion.id);
                        // Continuar con la siguiente suscripción
                    }
                }

                await _suscripcionVipRepository.SaveChangesAsync();
                await _clienteRepository.SaveChangesAsync();

                _logger.LogInformation("Actualización de estados VIP completada. Suscripciones vencidas: {Vencidas}, Clientes actualizados: {Clientes}, Renovaciones automáticas: {Renovaciones}",
                    suscripcionesVencidas.Count, clientesActualizados, renovacionesExitosas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estados de membresías VIP");
                throw;
            }
        }

        public async Task ActualizarEstadoAnunciosDestacadosAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de estados de anuncios destacados");
                var ahora = DateTime.UtcNow;

                // Desactivar anuncios vencidos
                int anunciosDesactivados = await _anuncioDestacadoRepository.DesactivarAnunciosVencidosAsync(ahora);

                // Activar anuncios que entran en vigencia
                int anunciosActivados = await _anuncioDestacadoRepository.ActivarAnunciosProgramadosAsync(ahora);

                _logger.LogInformation("Actualización de estados de anuncios completada. Anuncios desactivados: {Desactivados}, Anuncios activados: {Activados}",
                    anunciosDesactivados, anunciosActivados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estados de anuncios destacados");
                throw;
            }
        }
        public async Task LimpiarCuponesVencidosAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando limpieza de cupones vencidos");
                var ahora = DateTime.UtcNow;

                // Eliminar cupones vencidos no utilizados
                int cuponesEliminados = await _cuponClienteRepository.EliminarCuponesVencidosNoUsadosAsync(ahora);

                _logger.LogInformation("Limpieza de cupones completada. Cupones eliminados: {Cupones}", cuponesEliminados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar cupones vencidos");
                throw;
            }
        }

        // Implementación de tareas de negocio

        public async Task ActualizarScoresActividadAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de scores de actividad");

                // Obtener todos los acompañantes activos
                var acompanantes = await _acompananteRepository.GetAllActivosAsync();
                int actualizados = 0;

                foreach (var acompanante in acompanantes)
                {
                    try
                    {
                        // Calcular nuevo score de actividad
                        long nuevoScore = await CalcularScoreActividadAsync(acompanante.id);

                        // Actualizar score si es diferente
                        if (acompanante.score_actividad != nuevoScore)
                        {
                            await _acompananteRepository.ActualizarScoreActividadAsync(acompanante.id, nuevoScore);
                            actualizados++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al actualizar score de actividad para acompañante ID: {AcompananteId}", acompanante.id);
                        // Continuar con el siguiente
                    }
                }

                _logger.LogInformation("Actualización de scores de actividad completada. Perfiles actualizados: {Actualizados} de {Total}",
                    actualizados, acompanantes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar scores de actividad");
                throw;
            }
        }
        private async Task<long> CalcularScoreActividadAsync(int acompananteId)
        {
            // Pesos para el cálculo
            const int PESO_VISITA = 1;
            const int PESO_CONTACTO = 5;
            const int PESO_DESTACADO = 10;

            // Factores temporales (mayor peso a actividad reciente)
            const double FACTOR_24H = 1.0;
            const double FACTOR_7D = 0.7;
            const double FACTOR_30D = 0.3;

            var ahora = DateTime.UtcNow;
            var hace24h = ahora.AddHours(-24);
            var hace7d = ahora.AddDays(-7);
            var hace30d = ahora.AddDays(-30);

            // Visitas en diferentes períodos
            int visitas24h = await _acompananteRepository.CountVisitasPeriodoAsync(acompananteId, hace24h, ahora);
            int visitas7d = await _acompananteRepository.CountVisitasPeriodoAsync(acompananteId, hace7d, hace24h);
            int visitas30d = await _acompananteRepository.CountVisitasPeriodoAsync(acompananteId, hace30d, hace7d);

            // Contactos en diferentes períodos
            int contactos24h = await _acompananteRepository.CountContactosPeriodoAsync(acompananteId, hace24h, ahora);
            int contactos7d = await _acompananteRepository.CountContactosPeriodoAsync(acompananteId, hace7d, hace24h);
            int contactos30d = await _acompananteRepository.CountContactosPeriodoAsync(acompananteId, hace30d, hace7d);

            // Anuncios destacados activos
            bool tieneDestacado = await _anuncioDestacadoRepository.TieneAnuncioActivoAsync(acompananteId);

            // Verificar si está verificado (bonus)
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            bool estaVerificado = acompanante?.esta_verificado == true;

            // Calcular score con factores temporales
            double scoreVisitas = (visitas24h * PESO_VISITA * FACTOR_24H) +
                               (visitas7d * PESO_VISITA * FACTOR_7D) +
                               (visitas30d * PESO_VISITA * FACTOR_30D);

            double scoreContactos = (contactos24h * PESO_CONTACTO * FACTOR_24H) +
                                 (contactos7d * PESO_CONTACTO * FACTOR_7D) +
                                 (contactos30d * PESO_CONTACTO * FACTOR_30D);

            double scoreTotal = scoreVisitas + scoreContactos;

            // Bonificaciones
            if (tieneDestacado)
                scoreTotal += PESO_DESTACADO;

            if (estaVerificado)
                scoreTotal *= 1.2; // 20% extra por estar verificado

            return (long)Math.Round(scoreTotal);
        }
        public async Task ActualizarRankingsPerfilesAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de rankings de perfiles...");

                // Usar una variable estática para seguir los fallos consecutivos
                if (_rankingUpdateFailureCount > 5)
                {
                    _logger.LogWarning("Omitiendo actualización de rankings después de 5 fallos consecutivos. " +
                                       "Se reintentará en la próxima ejecución programada.");
                    return;
                }

                // Llamada al repositorio para verificar/actualizar rankings
                await _acompananteRepository.ActualizarVistaRankingPerfilesAsync();

                // Si llegamos aquí, la operación fue exitosa
                _rankingUpdateFailureCount = 0;
                _logger.LogInformation("Actualización de rankings de perfiles completada con éxito");
            }
            catch (Exception ex)
            {
                _rankingUpdateFailureCount++;

                // Registrar el error con detalles
                _logger.LogError(ex,
                    "Error #{FailCount} al actualizar rankings de perfiles: {Message}. Stack: {StackTrace}",
                    _rankingUpdateFailureCount, ex.Message, ex.StackTrace);

                // Si el error está relacionado con vistas ya existentes, no queremos propagarlo
                if (ex.Message.Contains("ya existe"))
                {
                    _logger.LogWarning("Error relacionado con 'relación ya existe'. " +
                                       "No se volverá a intentar esta operación hasta la próxima ejecución programada.");
                    return; // No propagar la excepción para que Hangfire no reintente
                }

                if (_rankingUpdateFailureCount <= 3)
                {
                    // Para los primeros 3 errores, permitimos que Hangfire reintente
                    throw;
                }

                // Después de 3 intentos, no propagamos la excepción para detener los reintentos
                _logger.LogWarning("Se alcanzó el máximo de reintentos. La tarea se marcará como completada " +
                                  "y se volverá a intentar en la próxima ejecución programada.");
            }
        }

        // Variable estática para contar fallos consecutivos
        private static int _rankingUpdateFailureCount = 0;
        public async Task EnviarRecordatoriosRenovacionMembresiasAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando envío de recordatorios de renovación de membresías");
                var ahora = DateTime.UtcNow;

                // Obtener suscripciones que vencen en 3 días
                var vencenEn3Dias = await _suscripcionVipRepository.GetSuscripcionesVencenEnDiasAsync(3);
                int recordatoriosEnviados = 0;

                foreach (var suscripcion in vencenEn3Dias)
                {
                    try
                    {
                        if (suscripcion.cliente_id > 0 && suscripcion.cliente?.usuario?.email != null)
                        {
                            // Enviar email de recordatorio
                            await _emailSender.SendEmailAsync(
                                suscripcion.cliente.usuario.email,
                                "Tu membresía VIP vence en 3 días",
                                $"Hola,\n\nTu membresía VIP vence el {suscripcion.fecha_fin:dd/MM/yyyy}. " +
                                "Renuévala ahora para seguir disfrutando de todos los beneficios exclusivos.\n\n" +
                                "Saludos,\nEl equipo de la plataforma"
                            );

                            // Notificar en tiempo real
                            await _notificador.NotificarUsuarioAsync(
                                suscripcion.cliente.usuario_id,
                                "Tu membresía VIP vence en 3 días. ¡Renuévala ahora!"
                            );

                            recordatoriosEnviados++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar recordatorio de renovación para suscripción ID: {SuscripcionId}", suscripcion.id);
                        // Continuar con la siguiente
                    }
                }

                _logger.LogInformation("Envío de recordatorios de renovación completado. Recordatorios enviados: {Enviados}", recordatoriosEnviados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar recordatorios de renovación de membresías");
                throw;
            }
        }

        public async Task ProcesarSolicitudesPendientesAntiguasAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando procesamiento de solicitudes pendientes antiguas");
                var limiteAntiguedad = DateTime.UtcNow.AddDays(-14); // Solicitudes de más de 14 días

                // Obtener solicitudes pendientes antiguas
                var solicitudesAntiguas = await _solicitudAgenciaRepository.GetSolicitudesPendientesAntiguasAsync(limiteAntiguedad);
                int solicitudesProcesadas = 0;

                foreach (var solicitud in solicitudesAntiguas)
                {
                    try
                    {
                        // Cambiar estado a "cancelada por inactividad"
                        solicitud.estado = "cancelada";
                        solicitud.motivo_cancelacion = "Cancelada automáticamente por inactividad";
                        solicitud.fecha_respuesta = DateTime.UtcNow;
                        await _solicitudAgenciaRepository.UpdateAsync(solicitud);

                        // Notificar a las partes
                        if (solicitud.acompanante?.usuario_id > 0)
                        {
                            await _notificador.NotificarUsuarioAsync(
                                solicitud.acompanante.usuario_id,
                                $"Tu solicitud a la agencia '{solicitud.agencia?.nombre}' ha sido cancelada automáticamente por inactividad."
                            );
                        }

                        if (solicitud.agencia?.usuario_id > 0)
                        {
                            await _notificador.NotificarUsuarioAsync(
                                solicitud.agencia.usuario_id,
                                $"La solicitud del acompañante '{solicitud.acompanante?.nombre_perfil}' ha sido cancelada automáticamente por inactividad."
                            );
                        }

                        solicitudesProcesadas++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar solicitud antigua ID: {SolicitudId}", solicitud.id);
                        // Continuar con la siguiente
                    }
                }

                await _solicitudAgenciaRepository.SaveChangesAsync();

                _logger.LogInformation("Procesamiento de solicitudes pendientes antiguas completado. Solicitudes procesadas: {Procesadas}", solicitudesProcesadas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitudes pendientes antiguas");
                throw;
            }
        }

        public async Task GenerarInformesPeriodicosAsync(string periodo)
        {
            try
            {
                _logger.LogInformation("Iniciando generación de informes periódicos: {Periodo}", periodo);

                // En una implementación real, aquí generaríamos informes para administradores
                // y los enviaríamos por email o los guardaríamos en el sistema

                // Ejemplo: informes de nuevos usuarios, actividad, ingresos, etc.

                _logger.LogInformation("Generación de informes periódicos completada: {Periodo}", periodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informes periódicos: {Periodo}", periodo);
                throw;
            }
        }

        public async Task DesactivarPerfilesInactivosAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando desactivación de perfiles inactivos");

                // Fecha límite: perfiles sin actividad en los últimos 30 días
                var limiteInactividad = DateTime.UtcNow.AddDays(-30);

                // Obtener perfiles inactivos
                var perfilesInactivos = await _acompananteRepository.GetPerfilesInactivosDesdeAsync(limiteInactividad);
                int perfilesDesactivados = 0;

                foreach (var perfil in perfilesInactivos)
                {
                    try
                    {
                        // Desactivar perfil
                        perfil.esta_disponible = false;
                        await _acompananteRepository.UpdateAsync(perfil);

                        // Notificar al acompañante
                        if (perfil.usuario_id > 0)
                        {
                            await _notificador.NotificarUsuarioAsync(
                                perfil.usuario_id,
                                "Tu perfil ha sido desactivado temporalmente por inactividad. Ingresa a tu cuenta para reactivarlo."
                            );

                            // Enviar email
                            if (perfil.usuario?.email != null)
                            {
                                await _emailSender.SendEmailAsync(
                                    perfil.usuario.email,
                                    "Perfil desactivado por inactividad",
                                    "Hola,\n\nTu perfil ha sido desactivado temporalmente debido a la falta de actividad en los últimos 30 días. " +
                                    "Para reactivarlo, simplemente inicia sesión en tu cuenta y activa nuevamente tu perfil desde tu panel de control.\n\n" +
                                    "Saludos,\nEl equipo de la plataforma"
                                );
                            }
                        }

                        perfilesDesactivados++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al desactivar perfil inactivo ID: {PerfilId}", perfil.id);
                        // Continuar con el siguiente
                    }
                }

                await _acompananteRepository.SaveChangesAsync();

                _logger.LogInformation("Desactivación de perfiles inactivos completada. Perfiles desactivados: {Desactivados}", perfilesDesactivados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar perfiles inactivos");
                throw;
            }
        }

        public async Task<List<RecurringJobInfoDto>> GetRecurringJobsInfoAsync()
        {
            var result = new List<RecurringJobInfoDto>();

            try
            {
                // Obtener todos los trabajos recurrentes de Hangfire
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var recurringJobs = connection.GetRecurringJobs();

                    foreach (var job in recurringJobs)
                    {
                        var jobInfo = new RecurringJobInfoDto
                        {
                            Id = job.Id,
                            JobName = GetJobName(job.Id),
                            CronExpression = job.Cron,
                            Description = GetJobDescription(job.Id),
                            NextExecution = job.NextExecution?.ToString("dd/MM/yyyy HH:mm") ?? "No programada",
                            LastExecution = job.LastExecution?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca",
                            LastStatus = job.LastJobState ?? "Desconocido"
                        };

                        result.Add(jobInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de trabajos recurrentes");
            }

            return result;
        }

        private string GetJobName(string jobId)
        {
            // Mapeo de IDs a nombres amigables
            return jobId switch
            {
                "limpiar-tokens-vencidos" => "Limpieza de Tokens Vencidos",
                "limpiar-intentos-login" => "Limpieza de Intentos de Login",
                "actualizar-estado-vip" => "Actualización de Membresías VIP",
                "actualizar-anuncios-destacados" => "Actualización de Anuncios Destacados",
                "limpiar-cupones-vencidos" => "Limpieza de Cupones Vencidos",
                "actualizar-scores-actividad" => "Actualización de Scores de Actividad",
                "actualizar-rankings" => "Actualización de Rankings",
                "recordatorios-renovacion" => "Recordatorios de Renovación VIP",
                "procesar-solicitudes-antiguas" => "Procesar Solicitudes Antiguas",
                "informes-diarios" => "Informes Diarios",
                "informes-semanales" => "Informes Semanales",
                "informes-mensuales" => "Informes Mensuales",
                "desactivar-perfiles-inactivos" => "Desactivación de Perfiles Inactivos",
                _ => jobId
            };
        }

        private string GetJobDescription(string jobId)
        {
            // Descripciones detalladas de cada trabajo
            return jobId switch
            {
                "limpiar-tokens-vencidos" => "Elimina tokens de acceso y reset de password vencidos o utilizados para mantener la base de datos limpia",
                "limpiar-intentos-login" => "Elimina registros antiguos de intentos de inicio de sesión",
                "actualizar-estado-vip" => "Actualiza el estado de las membresías VIP, desactivando las vencidas y procesando renovaciones automáticas",
                "actualizar-anuncios-destacados" => "Actualiza el estado de los anuncios destacados, activando y desactivando según sus fechas de vigencia",
                "limpiar-cupones-vencidos" => "Elimina cupones vencidos que no fueron utilizados",
                "actualizar-scores-actividad" => "Recalcula los scores de actividad de todos los perfiles basados en visitas y contactos recientes",
                "actualizar-rankings" => "Actualiza los rankings de perfiles populares, destacados y recientes",
                "recordatorios-renovacion" => "Envía recordatorios a clientes cuyas membresías VIP están próximas a vencer",
                "procesar-solicitudes-antiguas" => "Cancela automáticamente solicitudes de agencia que han estado pendientes por más de 14 días",
                "informes-diarios" => "Genera informes diarios de actividad de la plataforma para administradores",
                "informes-semanales" => "Genera informes semanales de métricas y rendimiento para administradores",
                "informes-mensuales" => "Genera informes mensuales detallados de métricas, ingresos y actividad",
                "desactivar-perfiles-inactivos" => "Desactiva temporalmente perfiles que no han tenido actividad en los últimos 30 días",
                _ => "Trabajo periódico del sistema"
            };
        }
    }



}


