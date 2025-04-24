using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class IntentoLoginRepository : IIntentoLoginRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public IntentoLoginRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetFailedAttemptsAsync(string email, string ip)
        {
            var registro = await _context.intentos_logins
                .FirstOrDefaultAsync(x => x.email == email && x.ip_address == ip);
            return registro?.intentos ?? 0;
        }

        public async Task<DateTime?> GetLastAttemptTimeAsync(string email, string ip)
        {
            return await _context.intentos_logins
                .Where(x => x.email == email && x.ip_address == ip)
                .Select(x => (DateTime?)x.updated_at)
                .FirstOrDefaultAsync();
        }

        public async Task RegisterFailedAttemptAsync(string email, string ip)
        {
            var existente = await _context.intentos_logins
                .FirstOrDefaultAsync(x => x.email == email && x.ip_address == ip);

            if (existente != null)
            {
                existente.intentos++;
                existente.updated_at = DateTime.UtcNow;
            }
            else
            {
                _context.intentos_logins.Add(new intentos_login
                {
                    email = email,
                    ip_address = ip,
                    intentos = 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task ResetFailedAttemptsAsync(string email, string ip)
        {
            var existente = await _context.intentos_logins
                .FirstOrDefaultAsync(x => x.email == email && x.ip_address == ip);

            if (existente != null)
            {
                existente.intentos = 0;
                existente.updated_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }

}
