using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.BackgroundJobs
{

    public class LogFailureAttribute : JobFilterAttribute, IElectStateFilter
    {
        private readonly ILogger<LogFailureAttribute> _logger;

        public LogFailureAttribute(ILogger<LogFailureAttribute> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState == null) return;

            var jobName = context.BackgroundJob.Job.ToString();
            var retryAttempt = context.GetJobParameter<int>("RetryCount") + 1;

            _logger.LogError(
                failedState.Exception,
                "Job {JobName} falló (intento {RetryAttempt}): {ErrorMessage}",
                jobName,
                retryAttempt,
                failedState.Exception.Message);

            // Si el error está relacionado con "relación ya existe", no reintentar
            if (failedState.Exception.Message.Contains("ya existe"))
            {
                _logger.LogWarning("Job {JobName} no reintentará debido a error de 'relación ya existe'", jobName);
                context.CandidateState = new SucceededState(
                    "Saltado debido a un error esperado (relación ya existe)",
                    0L,  // latency
                    0L   // performanceDuration
                );
            }
        }
    }
}
