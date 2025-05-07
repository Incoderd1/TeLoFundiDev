using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Application.DTOs.Anuncios;
using AgencyPlatform.Application.DTOs.Categoria;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.DTOs.Servicio;
using AgencyPlatform.Application.DTOs.SolicitudesRegistroAgencia;
using AgencyPlatform.Application.DTOs.Verificaciones;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Mappers
{
    public class MappingProfile : Profile
    {
      
            public MappingProfile()
            {
            // Mapeos de Agencia
                        CreateMap<agencia, AgenciaDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
             .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.usuario_id))
             .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
             .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion ?? string.Empty))
             .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.logo_url ?? string.Empty))
             .ForMember(dest => dest.SitioWeb, opt => opt.MapFrom(src => src.sitio_web ?? string.Empty))
             .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.direccion ?? string.Empty))
             .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => src.ciudad ?? string.Empty))
             .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.pais ?? string.Empty))
             .ForMember(dest => dest.EstaVerificada, opt => opt.MapFrom(src => src.esta_verificada == true))
             .ForMember(dest => dest.FechaVerificacion, opt => opt.MapFrom(src => src.fecha_verificacion))
             .ForMember(dest => dest.ComisionPorcentaje, opt => opt.MapFrom(src => src.comision_porcentaje))
                .ForMember(dest => dest.StripeAccountId, opt => opt.MapFrom(src => src.stripe_account_id));
            

            CreateMap<CrearAgenciaDto, agencia>()
                    .ForMember(dest => dest.nombre, opt => opt.MapFrom(src => src.Nombre))
                    .ForMember(dest => dest.descripcion, opt => opt.MapFrom(src => src.Descripcion))
                    .ForMember(dest => dest.logo_url, opt => opt.MapFrom(src => src.LogoUrl))
                    .ForMember(dest => dest.sitio_web, opt => opt.MapFrom(src => src.SitioWeb))
                    .ForMember(dest => dest.direccion, opt => opt.MapFrom(src => src.Direccion))
                    .ForMember(dest => dest.ciudad, opt => opt.MapFrom(src => src.Ciudad))
                    .ForMember(dest => dest.pais, opt => opt.MapFrom(src => src.Pais));


            CreateMap<acompanante, AcompananteResumen2Dto>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre_perfil))
                .ForMember(dest => dest.Edad, opt => opt.MapFrom(src => src.edad))
                .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.pais))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion))
                .ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.genero))
                .ForMember(dest => dest.Fotos, opt => opt.MapFrom(src => src.fotos))
                .ForMember(dest => dest.Whatsapp, opt => opt.MapFrom(src => src.whatsapp));

                CreateMap<agencia, AgenciaPendienteVerificacionDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
                    .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion))
                    .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.logo_url))
                    .ForMember(dest => dest.SitioWeb, opt => opt.MapFrom(src => src.sitio_web))
                    .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => src.ciudad))
                    .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.pais))
                    .ForMember(dest => dest.TotalAcompanantes, opt => opt.MapFrom(src => src.acompanantes != null ? src.acompanantes.Count : 0))
                    .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => src.created_at));
                    CreateMap<CrearSolicitudRegistroAgenciaDto, agencia>()
                    .ForMember(dest => dest.nombre, opt => opt.MapFrom(src => src.Nombre))
                    .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.descripcion, opt => opt.MapFrom(src => src.Descripcion))
                    .ForMember(dest => dest.direccion, opt => opt.MapFrom(src => src.Direccion))
                    .ForMember(dest => dest.ciudad, opt => opt.MapFrom(src => src.Ciudad))
                    .ForMember(dest => dest.pais, opt => opt.MapFrom(src => src.Pais))
                    .ForMember(dest => dest.logo_url, opt => opt.Ignore())
                    .ForMember(dest => dest.sitio_web, opt => opt.Ignore());

            CreateMap<CompletarPerfilAgenciaDto, agencia>()
              .ForMember(dest => dest.logo_url, opt => opt.MapFrom(src => src.LogoUrl ?? string.Empty))
              .ForMember(dest => dest.sitio_web, opt => opt.MapFrom(src => src.SitioWeb ?? string.Empty))
              // Ignorar explícitamente cada propiedad que no queremos mapear
              .ForMember(dest => dest.id, opt => opt.Ignore())
              .ForMember(dest => dest.usuario_id, opt => opt.Ignore())
              .ForMember(dest => dest.nombre, opt => opt.Ignore())
              .ForMember(dest => dest.descripcion, opt => opt.Ignore())
              .ForMember(dest => dest.direccion, opt => opt.Ignore())
              .ForMember(dest => dest.ciudad, opt => opt.Ignore())
              .ForMember(dest => dest.pais, opt => opt.Ignore())
              .ForMember(dest => dest.esta_verificada, opt => opt.Ignore())
              .ForMember(dest => dest.fecha_verificacion, opt => opt.Ignore())
              .ForMember(dest => dest.comision_porcentaje, opt => opt.Ignore())
              .ForMember(dest => dest.created_at, opt => opt.Ignore())
              .ForMember(dest => dest.updated_at, opt => opt.Ignore())
              .ForMember(dest => dest.email, opt => opt.Ignore())
              .ForMember(dest => dest.puntos_gastados, opt => opt.Ignore())
              .ForMember(dest => dest.puntos_acumulados, opt => opt.Ignore())
              .ForMember(dest => dest.acompanantes, opt => opt.Ignore())
              .ForMember(dest => dest.usuario, opt => opt.Ignore())
              .ForMember(dest => dest.verificaciones, opt => opt.Ignore()); 
            // Mapeos de Acompañante
            // Verificar el nombre correcto de la propiedad de categorías
            // Si tu entidad usa "acompanante_categorias", usa eso
            // De lo contrario, usa el nombre correcto que tenga tu entidad
            CreateMap<acompanante, AcompananteDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.usuario_id))
                    .ForMember(dest => dest.AgenciaId, opt => opt.MapFrom(src => src.agencia_id))
                    .ForMember(dest => dest.NombreAgencia, opt => opt.MapFrom(src => src.agencia != null ? src.agencia.nombre : null))
                    .ForMember(dest => dest.NombrePerfil, opt => opt.MapFrom(src => src.nombre_perfil))
                    .ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.genero))
                    .ForMember(dest => dest.Edad, opt => opt.MapFrom(src => src.edad))
                    .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion))
                    .ForMember(dest => dest.Altura, opt => opt.MapFrom(src => src.altura))
                    .ForMember(dest => dest.Peso, opt => opt.MapFrom(src => src.peso))
                    .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => src.ciudad))
                    .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.pais))
                    .ForMember(dest => dest.Idiomas, opt => opt.MapFrom(src => src.idiomas))
                    .ForMember(dest => dest.Disponibilidad, opt => opt.MapFrom(src => src.disponibilidad))
                    .ForMember(dest => dest.TarifaBase, opt => opt.MapFrom(src => src.tarifa_base))
                    .ForMember(dest => dest.Moneda, opt => opt.MapFrom(src => src.moneda))
                    .ForMember(dest => dest.EstaVerificado, opt => opt.MapFrom(src => src.esta_verificado == true))
                    .ForMember(dest => dest.Whatsapp, opt => opt.MapFrom(src => src.whatsapp))
                    .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.esta_disponible == true))
                    .ForMember(dest => dest.Fotos, opt => opt.MapFrom(src => src.fotos))
                    .ForMember(dest => dest.Servicios, opt => opt.MapFrom(src => src.servicios))
                    .ForMember(dest => dest.StripeAccountId, opt => opt.MapFrom(src => src.stripe_account_id))
                    .ForMember(dest => dest.StripePayoutsEnabled, opt => opt.MapFrom(src => src.stripe_payouts_enabled))
                    .ForMember(dest => dest.StripeChargesEnabled, opt => opt.MapFrom(src => src.stripe_charges_enabled))
                    .ForMember(dest => dest.StripeOnboardingCompleted, opt => opt.MapFrom(src => src.stripe_onboarding_completed))







                   
                    .ForMember(dest => dest.FotoPrincipal, opt => opt.MapFrom(src =>
                                    src.fotos == null
                                    ? null
                                    : src.fotos.Where(f => f.es_principal == true).Select(f => f.url).FirstOrDefault()));
            CreateMap<foto, FotoDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.id))
                .ForMember(d => d.AcompananteId, o => o.MapFrom(s => s.acompanante_id))
                .ForMember(d => d.Url, o => o.MapFrom(s => s.url))
                .ForMember(d => d.EsPrincipal, o => o.MapFrom(s => s.es_principal == true))
                .ForMember(d => d.Orden, o => o.MapFrom(s => s.orden))
                .ForMember(d => d.Verificada, o => o.MapFrom(s => s.verificada))
                .ForMember(d => d.FechaVerificacion, o => o.MapFrom(s => s.fecha_verificacion));


            CreateMap<servicio, ServicioDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.AcompananteId, opt => opt.MapFrom(src => src.acompanante_id))
                    .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
                    .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion))
                    .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.precio))
                    .ForMember(dest => dest.DuracionMinutos, opt => opt.MapFrom(src => src.duracion_minutos));

            CreateMap<categoria, AgencyPlatform.Application.DTOs.Categoria.CategoriaDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion));

            // Mapeos de Verificación
            CreateMap<verificacione, VerificacionDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.AgenciaId, opt => opt.MapFrom(src => src.agencia_id))
                    .ForMember(dest => dest.AcompananteId, opt => opt.MapFrom(src => src.acompanante_id))
                    .ForMember(dest => dest.FechaVerificacion, opt => opt.MapFrom(src => src.fecha_verificacion))
                    .ForMember(dest => dest.MontoCobrado, opt => opt.MapFrom(src => src.monto_cobrado))
                    .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.estado))
                    .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.observaciones));

                // Mapeos de Anuncio
                CreateMap<anuncios_destacado, AnuncioDestacadoDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.AcompananteId, opt => opt.MapFrom(src => src.acompanante_id))
                    .ForMember(dest => dest.NombreAcompanante, opt => opt.MapFrom(src =>
                        src.acompanante != null ? src.acompanante.nombre_perfil : string.Empty))
                    .ForMember(dest => dest.FechaInicio, opt => opt.MapFrom(src => src.fecha_inicio))
                    .ForMember(dest => dest.FechaFin, opt => opt.MapFrom(src => src.fecha_fin))
                    .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.tipo))
                    .ForMember(dest => dest.MontoPagado, opt => opt.MapFrom(src => src.monto_pagado))
                    .ForMember(dest => dest.CuponId, opt => opt.MapFrom(src => src.cupon_id))
                    .ForMember(dest => dest.EstaActivo, opt => opt.MapFrom(src => src.esta_activo == true));

            CreateMap<solicitud_registro_agencia, SolicitudRegistroAgenciaDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.nombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.descripcion))
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.logo_url))
                .ForMember(dest => dest.SitioWeb, opt => opt.MapFrom(src => src.sitio_web))
                .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.direccion))
                .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => src.ciudad))
                .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.pais))
                .ForMember(dest => dest.FechaSolicitud, opt => opt.MapFrom(src => src.fecha_solicitud))
                .ForMember(dest => dest.FechaRespuesta, opt => opt.MapFrom(src => src.fecha_respuesta))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.estado))
                .ForMember(dest => dest.MotivoRechazo, opt => opt.MapFrom(src => src.motivo_rechazo));
            // En tu perfil de AutoMapper
            CreateMap<pago_verificacion, PagoVerificacionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.VerificacionId, opt => opt.MapFrom(src => src.verificacion_id))
                .ForMember(dest => dest.AcompananteId, opt => opt.MapFrom(src => src.acompanante_id))
                .ForMember(dest => dest.AgenciaId, opt => opt.MapFrom(src => src.agencia_id))
                .ForMember(dest => dest.Monto, opt => opt.MapFrom(src => src.monto))
                .ForMember(dest => dest.Moneda, opt => opt.MapFrom(src => src.moneda))
                .ForMember(dest => dest.MetodoPago, opt => opt.MapFrom(src => src.metodo_pago))
                .ForMember(dest => dest.ReferenciaPago, opt => opt.MapFrom(src => src.referencia_pago))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.estado))
                .ForMember(dest => dest.FechaPago, opt => opt.MapFrom(src => src.fecha_pago));


           
        }

    }
        }
    
