using AgencyPlatform.Application.DTOs.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.ScheduledTasks
{
    public interface IScheduledTasksService
    {
        Task LimpiarIntentosLoginAntiguosAsync();
        Task ActualizarEstadoMembresiasVipAsync();
        Task ActualizarEstadoAnunciosDestacadosAsync();
        Task LimpiarCuponesVencidosAsync();
        Task ActualizarScoresActividadAsync();
        Task ActualizarRankingsPerfilesAsync();
        Task EnviarRecordatoriosRenovacionMembresiasAsync();
        Task ProcesarSolicitudesPendientesAntiguasAsync();
        Task GenerarInformesPeriodicosAsync(string periodo);
        Task DesactivarPerfilesInactivosAsync();
        Task<List<RecurringJobInfoDto>> GetRecurringJobsInfoAsync();
        Task InitializeScheduledTasksAsync();
    }
}
