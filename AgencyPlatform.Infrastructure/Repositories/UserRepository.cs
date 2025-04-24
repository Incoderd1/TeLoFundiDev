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
    public class UserRepository : IUserRepository
    {
        private readonly AgencyPlatformDbContext _context;

        public UserRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<usuario?> GetByEmailAsync(string email)
        {
            return await _context.usuarios.FirstOrDefaultAsync(u => u.email == email);
        }
        public async Task<int> GetRoleIdByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("El nombre del rol no puede estar vacío.");

            var role = await _context.roles
                .Where(r => r.nombre.ToLower() == roleName.ToLower())
                .FirstOrDefaultAsync();

            if (role == null)
                throw new Exception($"Rol '{roleName}' no encontrado en la base de datos.");

            return role.id;
        }
        public string GetRoleNameById(int roleId)
        {
            var role = _context.roles.FirstOrDefault(r => r.id == roleId);
            return role?.nombre ?? string.Empty;
        }


        public async Task<usuario?> GetByIdAsync(int id)
        {
            return await _context.usuarios.FindAsync(id);
        }

        public async Task<List<usuario>> GetAllAsync()
        {
            return await _context.usuarios.ToListAsync();
        }

        public async Task AddAsync(usuario user)
        {
            await _context.usuarios.AddAsync(user);
        }

        public Task UpdateAsync(usuario user)
        {
            _context.usuarios.Update(user);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(usuario user)
        {
            _context.usuarios.Remove(user);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ========== Refresh Tokens ==========
        public async Task<refresh_token?> GetRefreshTokenAsync(string token)
        {
            return await _context.refresh_tokens.FirstOrDefaultAsync(t => t.token == token);
        }

        public async Task AddRefreshTokenAsync(refresh_token token)
        {
            await _context.refresh_tokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        // ========== Reset Password Tokens ==========
        public async Task AddResetTokenAsync(tokens_reset_password token)
        {
            await _context.tokens_reset_passwords.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<tokens_reset_password?> GetResetTokenAsync(string token)
        {
            return await _context.tokens_reset_passwords.FirstOrDefaultAsync(t => t.token == token);
        }

        public async Task UpdateResetTokenAsync(tokens_reset_password token)
        {
            _context.tokens_reset_passwords.Update(token);
            await _context.SaveChangesAsync();
        }


        public async Task<int> GetFailedLoginAttemptsAsync(string email, string ipAddress)
        {
            var failedAttempts = await _context.intentos_login
                .Where(f => f.email == email && f.ip_address == ipAddress)
                .CountAsync();
            return failedAttempts;
        }

        public async Task<DateTime> GetLastFailedLoginAttemptTimeAsync(string email, string ipAddress)
        {
            var lastAttempt = await _context.intentos_login
                .Where(f => f.email == email && f.ip_address == ipAddress)
                .OrderByDescending(f => f.created_at)
                .FirstOrDefaultAsync();

            return lastAttempt?.created_at ?? DateTime.MinValue;
        }

        public async Task RegisterFailedLoginAttemptAsync(string email, string ipAddress)
        {
            var attempt = new intentos_login
            {
                email = email,
                ip_address = ipAddress,
                created_at = DateTime.UtcNow
            };

            await _context.intentos_login.AddAsync(attempt);
            await _context.SaveChangesAsync();
        }

        public async Task ResetFailedLoginAttemptsAsync(string email, string ipAddress)
        {
            var attempts = await _context.intentos_login
                .Where(f => f.email == email && f.ip_address == ipAddress)
                .ToListAsync();

            _context.intentos_login.RemoveRange(attempts);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRefreshTokenAsync(refresh_token token)
        {
            _context.refresh_tokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllRefreshTokensForUserAsync(int userId)
        {
            var tokens = await _context.refresh_tokens
                .Where(t => t.usuario_id == userId)
                .ToListAsync();

            _context.refresh_tokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllResetTokensForUserAsync(int userId)
        {
            var tokens = await _context.tokens_reset_passwords
                .Where(t => t.usuario_id == userId)
                .ToListAsync();

            _context.tokens_reset_passwords.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<usuario> Usuarios, int Total)> GetAllPagedAsync(int pagina, int elementosPorPagina)
        {
            if (pagina < 1 || elementosPorPagina < 1)
                throw new ArgumentException("Página y elementos por página deben ser mayores a 0");

            var query = _context.usuarios.AsQueryable();
            int total = await query.CountAsync();

            var usuarios = await query
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync();

            return (usuarios, total);
        }
        public async Task<string> GetRolNameByUserIdAsync(int usuarioId)
        {
            var usuario = await _context.usuarios
                .Include(u => u.rol)
                .FirstOrDefaultAsync(u => u.id == usuarioId);

            return usuario?.rol?.nombre;
        }
        public async Task<List<string>> GetRolesAsync(int usuarioId)
        {
            var usuario = await _context.usuarios
                .Include(u => u.rol)
                .FirstOrDefaultAsync(u => u.id == usuarioId);

            if (usuario == null || usuario.rol == null)
                return new List<string>();

            return new List<string> { usuario.rol.nombre };
        }

        public async Task<IEnumerable<usuario>> GetUsersByRoleAsync(string role)
        {
            var rol = await _context.roles
         .FirstOrDefaultAsync(r => r.nombre.ToLower() == role.ToLower());

            if (rol == null)
                return Enumerable.Empty<usuario>();

            var usuarios = await _context.usuarios
                .Where(u => u.rol_id == rol.id)
                .ToListAsync();

            return usuarios;
        }
    }
}


