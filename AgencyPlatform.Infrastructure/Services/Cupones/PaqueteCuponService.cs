using AgencyPlatform.Application.Interfaces.Services.Cupones;
using AgencyPlatform.Application.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgencyPlatform.Application.DTOs.Payments;

namespace AgencyPlatform.Infrastructure.Services.Cupones
{
    public class PaqueteCuponService : IPaqueteCuponService
    {
        private readonly IPaqueteCuponRepository _paqueteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PaqueteCuponService> _logger;

        public PaqueteCuponService(
            IPaqueteCuponRepository paqueteRepository,
            IMapper mapper,
            ILogger<PaqueteCuponService> logger)
        {
            _paqueteRepository = paqueteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PaqueteCuponDto>> GetAllActivosAsync()
        {
            try
            {
                // Obtener los paquetes con sus detalles incluidos
                var paquetes = await _paqueteRepository.GetAllActivosAsync();

                // Lista para el resultado final
                var paquetesDto = new List<PaqueteCuponDto>();

                foreach (var paquete in paquetes)
                {
                    // Mapear el paquete principal
                    var paqueteDto = _mapper.Map<PaqueteCuponDto>(paquete);

                    // Cargar y mapear detalles manualmente
                    if (paquete.paquete_cupones_detalles != null && paquete.paquete_cupones_detalles.Any())
                    {
                        paqueteDto.Detalles = new List<DetallePaqueteDto>();

                        foreach (var detalle in paquete.paquete_cupones_detalles)
                        {
                            var detalleDto = new DetallePaqueteDto
                            {
                                TipoCuponId = detalle.tipo_cupon_id,
                                Cantidad = detalle.cantidad
                            };

                            // Agregar información del tipo de cupón si está disponible
                            if (detalle.tipo_cupon != null)
                            {
                                detalleDto.NombreCupon = detalle.tipo_cupon.nombre;
                                detalleDto.PorcentajeDescuento = (int)detalle.tipo_cupon.porcentaje_descuento;
                            }

                            paqueteDto.Detalles.Add(detalleDto);
                        }
                    }
                    else
                    {
                        // Inicializar como lista vacía en lugar de null
                        paqueteDto.Detalles = new List<DetallePaqueteDto>();
                    }

                    paquetesDto.Add(paqueteDto);
                }

                return paquetesDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquetes de cupones activos");
                throw;
            }
        }

        public async Task<PaqueteCuponDto> GetByIdAsync(int id)
        {
            try
            {
                var paquete = await _paqueteRepository.GetByIdAsync(id);
                if (paquete == null)
                {
                    return null;
                }

                var paqueteDto = _mapper.Map<PaqueteCuponDto>(paquete);

                // Cargar detalles
                var detalles = await _paqueteRepository.GetDetallesByPaqueteIdAsync(id);
                paqueteDto.Detalles = _mapper.Map<List<DetallePaqueteDto>>(detalles);

                return paqueteDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquete de cupones con ID {Id}", id);
                throw;
            }
        }
    }

}