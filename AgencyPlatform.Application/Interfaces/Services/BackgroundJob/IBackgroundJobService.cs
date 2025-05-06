using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.BackgroundJob
{
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Encola un trabajo para ejecución inmediata en segundo plano
        /// </summary>
        /// <typeparam name="T">Tipo del servicio que contiene el método a ejecutar</typeparam>
        /// <param name="methodCall">Expresión lambda que representa el método a ejecutar</param>
        /// <returns>ID del trabajo encolado</returns>
        string Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : class;

        /// <summary>
        /// Encola un trabajo para ejecución diferida en segundo plano
        /// </summary>
        /// <typeparam name="T">Tipo del servicio que contiene el método a ejecutar</typeparam>
        /// <param name="methodCall">Expresión lambda que representa el método a ejecutar</param>
        /// <param name="delay">Tiempo de espera antes de la ejecución</param>
        /// <returns>ID del trabajo encolado</returns>
        string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) where T : class;

        /// <summary>
        /// Programa un trabajo recurrente usando una expresión cron
        /// </summary>
        /// <typeparam name="T">Tipo del servicio que contiene el método a ejecutar</typeparam>
        /// <param name="methodCall">Expresión lambda que representa el método a ejecutar</param>
        /// <param name="cronExpression">Expresión cron que define la recurrencia</param>
        /// <param name="jobId">Identificador único del trabajo recurrente</param>
        /// <returns>ID del trabajo recurrente</returns>
        string RecurringJob<T>(Expression<Func<T, Task>> methodCall, string cronExpression, string jobId) where T : class;

        /// <summary>
        /// Elimina un trabajo recurrente
        /// </summary>
        /// <param name="jobId">ID del trabajo recurrente a eliminar</param>
        void DeleteRecurringJob(string jobId);

        /// <summary>
        /// Verifica si un trabajo recurrente existe
        /// </summary>
        /// <param name="jobId">ID del trabajo recurrente</param>
        /// <returns>True si el trabajo existe, false en caso contrario</returns>
        bool RecurringJobExists(string jobId);
    }
}
