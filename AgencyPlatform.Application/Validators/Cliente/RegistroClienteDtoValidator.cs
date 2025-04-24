using AgencyPlatform.Application.DTOs.Cliente;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Validators.Cliente
{
    public class RegistroClienteDtoValidator : AbstractValidator<RegistroClienteDto>
    {
        public RegistroClienteDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio")
                .EmailAddress().WithMessage("Formato de email inválido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");

            RuleFor(x => x.Nickname)
                .MaximumLength(20).WithMessage("El nickname no puede superar los 20 caracteres");
        }
    }
}