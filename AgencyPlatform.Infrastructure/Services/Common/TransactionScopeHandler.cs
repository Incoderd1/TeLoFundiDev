using AgencyPlatform.Application.Interfaces.Services.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AgencyPlatform.Infrastructure.Services.Common
{
    public class TransactionScopeHandler : ITransactionScopeHandler
    {
        private readonly ILogger<TransactionScopeHandler> _logger;

        public TransactionScopeHandler(ILogger<TransactionScopeHandler> logger)
        {
            _logger = logger;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            // Usar código moderno con async/await para TransactionScope
            using var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                T result = await operation();
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la transacción");
                // No llamamos a scope.Complete(), lo que automáticamente hace rollback
                throw;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                await operation();
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la transacción");
                throw;
            }
        }
    }
}
