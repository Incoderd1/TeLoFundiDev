using AgencyPlatform.Application.DTOs.Usuarios;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Validators.Usuarios
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("Formato de email inválido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");

            RuleFor(x => x.TipoUsuario)
                .NotEmpty().WithMessage("El tipo de usuario es obligatorio.")
                .Must(tipo => new[] { "cliente", "agencia", "acompanante", "cliente_vip", "admin" }.
                    Contains(tipo.ToLower())).WithMessage("TipoUsuario no válido.");
        }
    }
}

