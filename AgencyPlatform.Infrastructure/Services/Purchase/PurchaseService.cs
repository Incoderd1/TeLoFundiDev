using AgencyPlatform.Application.Configuration;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services.ClienteCache;
using AgencyPlatform.Application.Interfaces.Services.Purchase;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Core.Entities;
using System.Transactions;
using AgencyPlatform.Shared.Exceptions;

namespace AgencyPlatform.Infrastructure.Services.Purchase
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IPaqueteCuponRepository _paqueteRepository;
        private readonly ICuponClienteRepository _cuponRepository;
        private readonly ICompraRepository _compraRepository;
        private readonly IPaymentService _paymentService;
        private readonly IPuntosService _puntosService;
        private readonly IClienteCacheService _cacheService;
        private readonly ILogger<PurchaseService> _logger;
        private readonly IMapper _mapper;
        private readonly ClienteSettings _clienteSettings;

        public PurchaseService(
            IClienteRepository clienteRepository,
            IPaqueteCuponRepository paqueteRepository,
            ICuponClienteRepository cuponRepository,
            ICompraRepository compraRepository,
            IPaymentService paymentService,
            IPuntosService puntosService,
            IClienteCacheService cacheService,
            ILogger<PurchaseService> logger,
            IMapper mapper,
            ClienteSettings clienteSettings)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _paqueteRepository = paqueteRepository ?? throw new ArgumentNullException(nameof(paqueteRepository));
            _cuponRepository = cuponRepository ?? throw new ArgumentNullException(nameof(cuponRepository));
            _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _puntosService = puntosService ?? throw new ArgumentNullException(nameof(puntosService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _clienteSettings = clienteSettings ?? throw new ArgumentNullException(nameof(clienteSettings));
        }

        public async Task<CompraDto> ComprarPaqueteAsync(int clienteId, ComprarPaqueteDto dto)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var paquete = await _paqueteRepository.GetByIdAsync(dto.PaqueteId);
            if (paquete == null)
                throw new NotFoundException("Paquete", dto.PaqueteId);

            // ⚠️ Comparación explícita con bool?
            if (paquete.activo != true)
                throw new BusinessRuleViolationException("El paquete no está disponible para compra");

            // Confirmación de pago en Stripe
            if (dto.MetodoPago.Equals("stripe", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(dto.PaymentIntentId))
            {
                var ok = await _paymentService.ConfirmPayment(dto.PaymentIntentId);
                if (!ok)
                    throw new ExternalServiceException("Stripe", "El pago no ha sido completado correctamente");
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var compra = new compras_paquete
            {
                cliente_id = clienteId,
                paquete_id = dto.PaqueteId,
                fecha_compra = DateTime.UtcNow,
                monto_pagado = paquete.precio,
                metodo_pago = dto.MetodoPago,
                referencia_pago = dto.PaymentIntentId ?? dto.ReferenciaPago,
                estado = "completado"
            };

            await _compraRepository.AddAsync(compra);
            await _compraRepository.SaveChangesAsync();

            try
            {
                await _puntosService.OtorgarPuntosPorAccionAsync(clienteId, "compra_paquete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al otorgar puntos por compra de paquete al cliente {ClienteId}", clienteId);
            }

            await GenerarCuponesPorCompraAsync(compra.id);

            // Invalidar caché de cupones usando el servicio de caché
            _cacheService.InvalidarCacheCliente(clienteId);

            scope.Complete();

            return _mapper.Map<CompraDto>(compra);
        }

        public async Task<List<CompraDto>> GetHistorialComprasAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var compras = await _compraRepository.GetByClienteIdAsync(clienteId);
            return _mapper.Map<List<CompraDto>>(compras);
        }
        public async Task<List<CuponClienteDto>> ObtenerCuponesDisponiblesAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            // Usar el servicio de caché para obtener los cupones
            return await _cacheService.GetCuponesDisponiblesAsync(clienteId, async () =>
            {
                _logger.LogDebug("Consultando cupones disponibles en base de datos. Cliente: {ClienteId}", clienteId);
                var cupones = await _cuponRepository.GetDisponiblesByClienteIdAsync(clienteId);
                return _mapper.Map<List<CuponClienteDto>>(cupones);
            });
        }

        public async Task<bool> UsarCuponAsync(int clienteId, string codigo)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente", clienteId);

            var cupon = await _cuponRepository.GetByCodigoAsync(codigo);
            if (cupon == null)
                throw new NotFoundException("Cupón", codigo);

            if (cupon.cliente_id != clienteId)
                throw new AccessDeniedException("No puedes usar un cupón que no te pertenece");

            if (cupon.esta_usado == true)
                throw new BusinessRuleViolationException("Este cupón ya ha sido utilizado");

            if (cupon.fecha_expiracion < DateTime.UtcNow)
                throw new BusinessRuleViolationException("Este cupón ha expirado");

            // Marcar como usado
            cupon.esta_usado = true;
            cupon.fecha_uso = DateTime.UtcNow;

            await _cuponRepository.UpdateAsync(cupon);
            await _cuponRepository.SaveChangesAsync();

            // Invalidar caché de cupones usando el servicio de caché
            _cacheService.InvalidarCacheCliente(clienteId);
            _logger.LogDebug("Caché de cupones invalidada para cliente {ClienteId} tras usar cupón", clienteId);

            return true;
        }

        public async Task GenerarCuponesPorCompraAsync(int compraId)
        {
            var compra = await _compraRepository.GetByIdAsync(compraId);
            if (compra == null)
                throw new NotFoundException("Compra", compraId);

            var paqueteDetalles = await _paqueteRepository.GetDetallesByPaqueteIdAsync(compra.paquete_id);
            if (paqueteDetalles == null || !paqueteDetalles.Any())
                return; // No hay detalles de cupones para generar

            foreach (var detalle in paqueteDetalles)
            {
                for (int i = 0; i < detalle.cantidad; i++)
                {
                    string codigo = $"CP-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

                    var cupon = new cupones_cliente
                    {
                        cliente_id = compra.cliente_id,
                        tipo_cupon_id = detalle.tipo_cupon_id,
                        codigo = codigo,
                        fecha_creacion = DateTime.UtcNow,
                        fecha_expiracion = DateTime.UtcNow.AddMonths(_clienteSettings.CuponesValidezMeses),
                        esta_usado = false
                    };

                    await _cuponRepository.AddAsync(cupon);
                }
            }

            await _cuponRepository.SaveChangesAsync();
        }
    }
}

