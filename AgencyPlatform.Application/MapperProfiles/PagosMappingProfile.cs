using AgencyPlatform.Application.DTOs.Payments;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.MapperProfiles
{
    public class PagosMappingProfile : Profile
    {
        public PagosMappingProfile()
        {
            // Mapeo de paquetes_cupone a PaqueteCuponDto
            CreateMap<paquetes_cupone, PaqueteCuponDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion))
                .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.precio))
                .ForMember(dest => dest.PuntosOtorgados, opt => opt.MapFrom(src => src.puntos_otorgados))
                .ForMember(dest => dest.IncluyeSorteo, opt => opt.MapFrom(src => src.incluye_sorteo));

            // Mapeo de paquete_cupones_detalle a DetallePaqueteDto
            CreateMap<paquete_cupones_detalle, DetallePaqueteDto>()
                .ForMember(dest => dest.TipoCuponId, opt => opt.MapFrom(src => src.tipo_cupon_id))
                .ForMember(dest => dest.NombreCupon, opt => opt.MapFrom(src => src.tipo_cupon.nombre))
                .ForMember(dest => dest.PorcentajeDescuento, opt => opt.MapFrom(src => src.tipo_cupon.porcentaje_descuento))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.cantidad));
        }
    }
}
