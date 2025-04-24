using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IIntentoLoginRepository
    {
        Task<int> GetFailedAttemptsAsync(string email, string ip);
        Task<DateTime?> GetLastAttemptTimeAsync(string email, string ip);
        Task RegisterFailedAttemptAsync(string email, string ip);
        Task ResetFailedAttemptsAsync(string email, string ip);
    }
}
