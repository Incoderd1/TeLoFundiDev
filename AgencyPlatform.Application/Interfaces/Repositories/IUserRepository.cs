using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<usuario?> GetByEmailAsync(string email);
        Task<usuario?> GetByIdAsync(int id);
        Task<List<usuario>> GetAllAsync();
        Task AddAsync(usuario user);
        Task UpdateAsync(usuario user);
        Task DeleteAsync(usuario user);
        Task SaveChangesAsync();
        Task<int> GetRoleIdByNameAsync(string roleName);
        string GetRoleNameById(int roleId);


        // Para tokens
        Task<refresh_token?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(refresh_token token);

        // Para reset password
        Task AddResetTokenAsync(tokens_reset_password token);
        Task<tokens_reset_password?> GetResetTokenAsync(string token);
        Task UpdateResetTokenAsync(tokens_reset_password token);

        Task<int> GetFailedLoginAttemptsAsync(string email, string ipAddress);
        Task<DateTime> GetLastFailedLoginAttemptTimeAsync(string email, string ipAddress);
        Task RegisterFailedLoginAttemptAsync(string email, string ipAddress);
        Task ResetFailedLoginAttemptsAsync(string email, string ipAddress);

        // Métodos para tokens
        Task UpdateRefreshTokenAsync(refresh_token token);
        Task RevokeAllRefreshTokensForUserAsync(int userId);
        Task RevokeAllResetTokensForUserAsync(int userId);

        // Métodos para paginación
        Task<(List<usuario> Usuarios, int Total)> GetAllPagedAsync(int pagina, int elementosPorPagina);
        Task<List<string>> GetRolesAsync(int usuarioId); // 🔥 Agregar esta línea
        Task<IEnumerable<usuario>> GetUsersByRoleAsync(string role);

    }
}
