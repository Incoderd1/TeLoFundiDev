using AgencyPlatform.Application.DTOs.Usuarios;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Validators
{
    public class ResetPasswordConfirmRequestValidator : AbstractValidator<ResetPasswordConfirmRequest>
    {
        public ResetPasswordConfirmRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("El token es obligatorio.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La nueva contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[0-9]").WithMessage("La nueva contraseña debe contener al menos un número.")
                .Matches(@"[!@#$%^&*]").WithMessage("La nueva contraseña debe contener al menos un carácter especial (!@#$%^&*).");
        }
    }
}
