using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.Cliente;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Jobs
{
    // Infrastructure/Jobs/VipMembershipCheckJob.cs
    public class VipMembershipCheckJob : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VipMembershipCheckJob> _logger;

        public VipMembershipCheckJob(
            IServiceScopeFactory scopeFactory,
            ILogger<VipMembershipCheckJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servicio de verificación de membresías VIP iniciado");

            // Ejecutar cada 24 horas
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using var scope = _scopeFactory.CreateScope();
            var clienteService = scope.ServiceProvider.GetRequiredService<IClienteService>();
            var repository = scope.ServiceProvider.GetRequiredService<ISuscripcionVipRepository>();

            var now = DateTime.UtcNow;
            var suscripcionesExpiradas = await repository.GetExpiradas(now);

            foreach (var suscripcion in suscripcionesExpiradas)
            {
                try
                {
                    if (suscripcion.es_renovacion_automatica == true)
                    {
                        // Intentar renovar automáticamente
                        _logger.LogInformation($"Intentando renovar suscripción {suscripcion.id}");

                        // Buscar información de pago
                        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                        // Si tiene información de pago guardada (Stripe)
                        if (suscripcion.metodo_pago?.ToLower() == "stripe" && !string.IsNullOrEmpty(suscripcion.referencia_pago))
                        {
                            try
                            {
                                // Cobrar nuevamente
                                var nuevaFechaFin = suscripcion.fecha_fin.AddMonths(1);
                                suscripcion.fecha_fin = nuevaFechaFin;
                                await repository.UpdateAsync(suscripcion);

                                // Actualizar también al cliente
                                var cliente = await repository.GetClienteFromSuscripcion(suscripcion.id);
                                if (cliente != null)
                                {
                                    cliente.fecha_fin_vip = nuevaFechaFin;
                                    await repository.UpdateClienteAsync(cliente);
                                }

                                _logger.LogInformation($"Suscripción {suscripcion.id} renovada automáticamente hasta {nuevaFechaFin}");
                            }
                            catch
                            {
                                // Si falla el pago, marcar como expirada
                                suscripcion.estado = "expirada";
                                await repository.UpdateAsync(suscripcion);

                                // Quitar estado VIP
                                var cliente = await repository.GetClienteFromSuscripcion(suscripcion.id);
                                if (cliente != null && cliente.es_vip == true)
                                {
                                    cliente.es_vip = false;
                                    await repository.UpdateClienteAsync(cliente);
                                }
                            }
                        }
                    }
                    else
                    {
                        // No tiene renovación automática, marcar como expirada
                        suscripcion.estado = "expirada";
                        await repository.UpdateAsync(suscripcion);

                        // Actualizar cliente para quitar estado VIP
                        var cliente = await repository.GetClienteFromSuscripcion(suscripcion.id);
                        if (cliente != null && cliente.es_vip == true)
                        {
                            cliente.es_vip = false;
                            await repository.UpdateClienteAsync(cliente);
                            _logger.LogInformation($"Cliente {cliente.id} ya no es VIP");
                        }
                    }

                    await repository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al procesar suscripción {suscripcion.id}");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
