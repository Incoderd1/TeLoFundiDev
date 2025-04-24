using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia
{
    public class CrearSolicitudRegistroAgenciaDtoValidator : AbstractValidator<CrearSolicitudRegistroAgenciaDto>
    {
        public CrearSolicitudRegistroAgenciaDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre de la agencia es obligatorio.")
                .MaximumLength(255).WithMessage("El nombre no puede tener más de 255 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El correo electrónico no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.");

            //RuleFor(x => x.LogoUrl)
            //    .Matches(@"^(http[s]?:\/\/.*\.(?:png|jpg|jpeg|gif|bmp))$").WithMessage("El logo debe ser una URL válida de imagen.");

            //RuleFor(x => x.SitioWeb)
            //    .Matches(@"^(http[s]?:\/\/[^\s]+)$").WithMessage("El sitio web debe ser una URL válida.");
        }
    }

}