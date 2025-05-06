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
    public class SearchMappingProfile : Profile
    {
        public SearchMappingProfile()
        {
            CreateMap<acompanante, AcompananteSearchResultDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.NombrePerfil, opt => opt.MapFrom(src => src.nombre_perfil))
                .ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.genero))
                .ForMember(dest => dest.Edad, opt => opt.MapFrom(src => src.edad))
                .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => src.ciudad))
                .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.pais))
                .ForMember(dest => dest.TarifaBase, opt => opt.MapFrom(src => src.tarifa_base))
                .ForMember(dest => dest.Moneda, opt => opt.MapFrom(src => src.moneda))
                .ForMember(dest => dest.EstaVerificado, opt => opt.MapFrom(src => src.esta_verificado))
                .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.esta_disponible))
                .ForMember(dest => dest.ScoreActividad, opt => opt.MapFrom(src => src.score_actividad ?? 0));
            // Las otras propiedades como FotoPrincipalUrl, TotalFotos, Categorias, Servicios, etc.
            // se configuran manualmente en el código de SearchAcompanantesAsync
        }
    }
}
