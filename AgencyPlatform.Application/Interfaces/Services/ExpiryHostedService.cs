using AgencyPlatform.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services
{
    public class ExpiryHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiryHostedService> _logger;

        public ExpiryHostedService(IServiceScopeFactory scopeFactory,
                                   ILogger<ExpiryHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiryHostedService iniciado.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var susRepo = scope.ServiceProvider.GetRequiredService<ISuscripcionVipRepository>();
                    var cliRepo = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
                    var cuponRepo = scope.ServiceProvider.GetRequiredService<ICuponClienteRepository>();

                    var ahora = DateTime.UtcNow;

                    // 1) Expirar suscripciones VIP pasadas de fecha
                    var activas = await susRepo.GetActivasByFechaFinAsync(ahora);
                    foreach (var s in activas)
                    {
                        s.estado = "cancelada";
                        await susRepo.UpdateAsync(s);
                        // además actualizar cliente.es_vip = false si ya no tiene otra suscripción activa
                        var cliente = await cliRepo.GetByIdAsync(s.cliente_id);
                        cliente.es_vip = false;
                        await cliRepo.UpdateAsync(cliente);
                    }
                    await susRepo.SaveChangesAsync();
                    await cliRepo.SaveChangesAsync();

                    // 2) Marcar cupones expirados
                    var vencidos = await cuponRepo.GetExpiradosAsync(ahora);
                    foreach (var c in vencidos)
                    {
                        c.esta_usado = true; // ó un flag propio de expirado
                        await cuponRepo.UpdateAsync(c);
                    }
                    await cuponRepo.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en ExpiryHostedService");
                }

                // duerme 24h antes de volver a revisar
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
