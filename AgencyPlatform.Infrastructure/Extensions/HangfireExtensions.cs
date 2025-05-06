using AgencyPlatform.Application.Interfaces.ScheduledTasks;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace AgencyPlatform.Infrastructure.Extensions
{
    public static class HangfireExtensions
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar Hangfire con PostgreSQL
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire", // Esquema separado para tablas de Hangfire
                    QueuePollInterval = TimeSpan.FromSeconds(15), // Intervalo de sondeo de cola
                    InvisibilityTimeout = TimeSpan.FromMinutes(5), // Tiempo máximo para trabajos bloqueados
                    PrepareSchemaIfNecessary = true, // Crear esquema si no existe
                    UseNativeDatabaseTransactions = true, // Usar transacciones nativas de PostgreSQL
                }));

            // Configurar servidor de procesamiento de Hangfire
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Math.Max(Environment.ProcessorCount * 2, 10); // Número de workers = 2 * CPU cores (mínimo 10)
                options.Queues = new[] { "default", "critical", "background" }; // Colas con prioridades
                options.ServerName = $"server:{Environment.MachineName}"; // Nombre del servidor para identificación
                options.SchedulePollingInterval = TimeSpan.FromSeconds(15); // Intervalo para verificar trabajos programados
            });

            return services;
        }

        public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app, IConfiguration configuration)
        {
            // Configurar el panel de administración de Hangfire
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                IgnoreAntiforgeryToken = true,
                IsReadOnlyFunc = _ => false, // Panel de solo lectura o no
                DisplayStorageConnectionString = false, // Ocultar cadena de conexión
                DisplayNameFunc = (context, job) => job?.Type?.Name ?? "Tarea" // Personalizar nombres de trabajos
            });

            return app;
        }
        public static IApplicationBuilder InitializeRecurringJobs(this IApplicationBuilder app)
        {
            // Inicializar tareas programadas
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var scheduledTasksService = scope.ServiceProvider.GetRequiredService<IScheduledTasksService>();
                scheduledTasksService.InitializeScheduledTasksAsync().GetAwaiter().GetResult();
            }

            return app;
        }
        // En HangfireExtensions.cs
        public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                // En entorno de desarrollo, permite acceso a todos
                return true;

                // Código original (comentado)
                // var httpContext = context.GetHttpContext();
                // return httpContext.User.Identity?.IsAuthenticated == true &&
                //        httpContext.User.IsInRole("admin");
            }
        }
    }
}