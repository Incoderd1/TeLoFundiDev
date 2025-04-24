using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.MapperProfiles
{
    public class AcompananteProfile : Profile
    {
        public AcompananteProfile()
        {
            CreateMap<AcompananteCreateDto, acompanante>()
              .ForMember(dest => dest.nombre_perfil, opt => opt.MapFrom(src => src.NombrePerfil))
              .ForMember(dest => dest.genero, opt => opt.MapFrom(src => src.Genero))
              .ForMember(dest => dest.edad, opt => opt.MapFrom(src => src.Edad))
              .ForMember(dest => dest.ciudad, opt => opt.MapFrom(src => src.Ciudad))
              .ForMember(dest => dest.pais, opt => opt.MapFrom(src => src.Pais))

              // Campos de contacto
              .ForMember(dest => dest.telefono, opt => opt.MapFrom(src => src.Telefono))
              .ForMember(dest => dest.whatsapp, opt => opt.MapFrom(src => src.WhatsApp))
              .ForMember(dest => dest.email_contacto, opt => opt.MapFrom(src => src.EmailContacto))

              // Establecer valores fijos (no dependen del DTO)
              .ForMember(dest => dest.mostrar_telefono, opt => opt.MapFrom(_ => true))
              .ForMember(dest => dest.mostrar_whatsapp, opt => opt.MapFrom(_ => true))
              .ForMember(dest => dest.mostrar_email, opt => opt.MapFrom(_ => true))

              // Campos a ignorar
              .ForMember(dest => dest.esta_verificado, opt => opt.Ignore())
              .ForMember(dest => dest.fecha_verificacion, opt => opt.Ignore())
              .ForMember(dest => dest.created_at, opt => opt.Ignore())
              .ForMember(dest => dest.updated_at, opt => opt.Ignore())
              .ForMember(dest => dest.score_actividad, opt => opt.Ignore())
              .ForMember(dest => dest.acompanante_categoria, opt => opt.Ignore())
              .ForMember(dest => dest.agencia, opt => opt.Ignore())
              .ForMember(dest => dest.anuncios_destacados, opt => opt.Ignore())
              .ForMember(dest => dest.contactos, opt => opt.Ignore())
              .ForMember(dest => dest.fotos, opt => opt.Ignore())
              .ForMember(dest => dest.servicios, opt => opt.Ignore())
              .ForMember(dest => dest.usuario, opt => opt.Ignore())
              .ForMember(dest => dest.verificacione, opt => opt.Ignore())
              .ForMember(dest => dest.visitas_perfils, opt => opt.Ignore());

        }
    }
}
