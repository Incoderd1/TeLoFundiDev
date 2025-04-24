using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.DTOs.Sorteos;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.MapperProfiles
{
    public class ClienteProfile : Profile
    {
        public ClienteProfile()
        {
            CreateMap<cliente, ClienteDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.usuario_id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.usuario.email))
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.nickname))
                .ForMember(dest => dest.EsVip, opt => opt.MapFrom(src => src.es_vip ?? false))
                .ForMember(dest => dest.FechaInicioVip, opt => opt.MapFrom(src => src.fecha_inicio_vip))
                .ForMember(dest => dest.FechaFinVip, opt => opt.MapFrom(src => src.fecha_fin_vip))
                .ForMember(dest => dest.PuntosAcumulados, opt => opt.MapFrom(src => src.puntos_acumulados ?? 0))
                .ForMember(dest => dest.Cupones, opt => opt.Ignore());

            CreateMap<cliente, ClienteDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.usuario_id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.usuario.email))
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.nickname))
                .ForMember(dest => dest.EsVip, opt => opt.MapFrom(src => src.es_vip ?? false))
                .ForMember(dest => dest.FechaInicioVip, opt => opt.MapFrom(src => src.fecha_inicio_vip))
                .ForMember(dest => dest.FechaFinVip, opt => opt.MapFrom(src => src.fecha_fin_vip))
                .ForMember(dest => dest.PuntosAcumulados, opt => opt.MapFrom(src => src.puntos_acumulados ?? 0))
                .ForMember(dest => dest.Cupones, opt => opt.Ignore())
                .ForMember(dest => dest.UltimosMovimientos, opt => opt.Ignore())
                .ForMember(dest => dest.UltimasCompras, opt => opt.Ignore())
                .ForMember(dest => dest.MembresiaCurrent, opt => opt.Ignore());

            CreateMap<movimientos_punto, MovimientoPuntosDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.cliente_id))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.cantidad))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.tipo))
                .ForMember(dest => dest.Concepto, opt => opt.MapFrom(src => src.concepto))
                .ForMember(dest => dest.SaldoAnterior, opt => opt.MapFrom(src => src.saldo_anterior))
                .ForMember(dest => dest.SaldoNuevo, opt => opt.MapFrom(src => src.saldo_nuevo))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.fecha));

            CreateMap<cupones_cliente, CuponClienteDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.cliente_id))
                .ForMember(dest => dest.TipoCuponId, opt => opt.MapFrom(src => src.tipo_cupon_id))
                .ForMember(dest => dest.TipoCuponNombre, opt => opt.MapFrom(src => src.tipo_cupon.nombre))
                .ForMember(dest => dest.PorcentajeDescuento, opt => opt.MapFrom(src => src.tipo_cupon.porcentaje_descuento))
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.codigo))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.fecha_creacion))
                .ForMember(dest => dest.FechaExpiracion, opt => opt.MapFrom(src => src.fecha_expiracion))
                .ForMember(dest => dest.EstaUsado, opt => opt.MapFrom(src => src.esta_usado ?? false))
                .ForMember(dest => dest.FechaUso, opt => opt.MapFrom(src => src.fecha_uso));



            CreateMap<sorteo, SorteoDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
               .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
               .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion))
               .ForMember(dest => dest.FechaInicio, opt => opt.MapFrom(src => src.fecha_inicio))
               .ForMember(dest => dest.FechaFin, opt => opt.MapFrom(src => src.fecha_fin))
               .ForMember(dest => dest.EstaActivo, opt => opt.MapFrom(src => src.esta_activo == true))
               .ForMember(dest => dest.EstoyParticipando, opt => opt.Ignore())
               .ForMember(dest => dest.TotalParticipantes, opt => opt.Ignore());


            // Mapeo de compras_paquete a CompraDto
            CreateMap<compras_paquete, CompraDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.cliente_id))
                .ForMember(dest => dest.PaqueteId, opt => opt.MapFrom(src => src.paquete_id))
                .ForMember(dest => dest.PaqueteNombre, opt => opt.MapFrom(src => src.paquete.nombre))
                .ForMember(dest => dest.FechaCompra, opt => opt.MapFrom(src => src.fecha_compra))
                .ForMember(dest => dest.MontoPagado, opt => opt.MapFrom(src => src.monto_pagado))
                .ForMember(dest => dest.MetodoPago, opt => opt.MapFrom(src => src.metodo_pago))
                .ForMember(dest => dest.ReferenciaPago, opt => opt.MapFrom(src => src.referencia_pago))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.estado));

            // Mapeo de membresias_vip a MembresiVipDto
            CreateMap<membresias_vip, MembresiVipDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
                .ForMember(dest => dest.PrecioMensual, opt => opt.MapFrom(src => src.precio_mensual))
                .ForMember(dest => dest.PuntosMensuales, opt => opt.MapFrom(src => src.puntos_mensuales))
                .ForMember(dest => dest.DescuentoAnuncios, opt => opt.MapFrom(src => src.descuento_anuncios))
                .ForMember(dest => dest.Activa, opt => opt.MapFrom(src => src.esta_activa ?? true));

            // Mapeo de acompanante a AcompananteCardDto
            CreateMap<acompanante, AcompananteCardDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.NombrePerfil, opt => opt.MapFrom(src => src.nombre_perfil))
                .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => src.ciudad))
                .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.pais))
                .ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.genero))
                .ForMember(dest => dest.TarifaBase, opt => opt.MapFrom(src => src.tarifa_base))
                .ForMember(dest => dest.Moneda, opt => opt.MapFrom(src => src.moneda))
                .ForMember(dest => dest.FotoPrincipalUrl, opt => opt.Ignore()) // Se asigna manualmente
                .ForMember(dest => dest.EstaVerificado, opt => opt.MapFrom(src => src.esta_verificado == true))
                .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.esta_disponible == true))
                .ForMember(dest => dest.TotalVisitas, opt => opt.Ignore()) // Se calcula manualmente
                .ForMember(dest => dest.TotalContactos, opt => opt.Ignore()) // Se calcula manualmente
                .ForMember(dest => dest.RazonRecomendacion, opt => opt.Ignore()); // Se asigna manualmente

        }
    }
}
