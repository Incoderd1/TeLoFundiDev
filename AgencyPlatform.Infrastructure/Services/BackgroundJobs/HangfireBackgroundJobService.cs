using AgencyPlatform.Application.Interfaces.Services.BackgroundJob;
using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;


namespace AgencyPlatform.Infrastructure.Services.BackgroundJobs
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public string Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : class
        {
            return BackgroundJob.Enqueue(methodCall);
        }

        public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) where T : class
        {
            return BackgroundJob.Schedule(methodCall, delay);
        }

        // En HangfireBackgroundJobService.cs
        public string RecurringJob<T>(Expression<Func<T, Task>> methodCall, string cronExpression, string jobId) where T : class
        {
            Hangfire.RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression,
                TimeZoneInfo.Utc);
            return jobId;
        }

        public void DeleteRecurringJob(string jobId)
        {
            Hangfire.RecurringJob.RemoveIfExists(jobId);
        }

        public bool RecurringJobExists(string jobId)
        {
            // Hangfire no tiene un método directo para verificar si existe un trabajo recurrente
            // Una forma de verificarlo es intentar obtener los trabajos recurrentes e iterar para ver si existe
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            return recurringJobs.Any(j => j.Id == jobId);
        }
    }
}
