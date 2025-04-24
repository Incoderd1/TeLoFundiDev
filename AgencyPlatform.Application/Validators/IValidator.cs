using AgencyPlatform.Application.DTOs.Cliente;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Validators
{
    public interface IValidator<T>
    {
        void Validate(T entity);
    }


    public class RegistroClienteDtoValidator : IValidator<RegistroClienteDto>
    {
        public void Validate(RegistroClienteDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Email: trim, lowercase y validación de formato
            dto.Email = dto.Email?.Trim().ToLowerInvariant()
                ?? throw new ArgumentException("El email es requerido.", nameof(dto.Email));

            if (!Regex.IsMatch(dto.Email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.Compiled))
            {
                throw new ValidationException("Formato de correo inválido.");
            }

            // Password: requerido y mínimo 8 caracteres
            if (string.IsNullOrEmpty(dto.Password))
            {
                throw new ArgumentException("La contraseña es requerida.", nameof(dto.Password));
            }

            if (dto.Password.Length < 8)
            {
                throw new ValidationException("La contraseña debe tener al menos 8 caracteres.");
            }

            // Nickname: trim y máximo 30 caracteres
            if (!string.IsNullOrEmpty(dto.Nickname))
            {
                dto.Nickname = dto.Nickname.Trim();

                if (dto.Nickname.Length > 30)
                    throw new ValidationException("El nickname no puede superar 30 caracteres.");
            }
        }
    }
}
