using AgencyPlatform.Application.Configuration;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Puntos
{
    public class PuntosService : IPuntosService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IAccionesPuntosRepository _accionesPuntosRepository;
        private readonly IMovimientoPuntosRepository _movimientoPuntosRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PuntosService> _logger;
        private readonly ClienteSettings _clienteSettings;

        public PuntosService(
            IClienteRepository clienteRepository,
            IAccionesPuntosRepository accionesPuntosRepository,
            IMovimientoPuntosRepository movimientoPuntosRepository,
            IMapper mapper,
            ILogger<PuntosService> logger,
            ClienteSettings clienteSettings)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _accionesPuntosRepository = accionesPuntosRepository ?? throw new ArgumentNullException(nameof(accionesPuntosRepository));
            _movimientoPuntosRepository = movimientoPuntosRepository ?? throw new ArgumentNullException(nameof(movimientoPuntosRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clienteSettings = clienteSettings ?? throw new ArgumentNullException(nameof(clienteSettings));
        }
        public async Task<MovimientoPuntosDto> OtorgarPuntosPorAccionAsync(int clienteId, string accion)
        {
            _logger.LogInformation("Iniciando otorgamiento de puntos. Cliente: {ClienteId}, Acción: {Accion}", clienteId, accion);

            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
            {
                _logger.LogWarning("Intento de otorgar puntos a cliente inexistente. ClienteId: {ClienteId}", clienteId);
                throw new KeyNotFoundException($"Cliente con ID {clienteId} no encontrado");
            }

            // Buscar definición de la acción
            var accionPuntos = await _accionesPuntosRepository.GetByNombreAsync(accion);
            if (accionPuntos == null || accionPuntos.esta_activa != true)
            {
                _logger.LogWarning("Intento de otorgar puntos por acción inexistente o inactiva. Cliente: {ClienteId}, Acción: {Accion}", clienteId, accion);
                throw new ArgumentException($"Acción '{accion}' no encontrada o inactiva");
            }

            // Verificar límites diarios
            if (accionPuntos.limite_diario.HasValue)
            {
                var accionesHoy = await _movimientoPuntosRepository.CountByClienteIdAndConceptoHoyAsync(clienteId, $"accion:{accion}");
                if (accionesHoy >= accionPuntos.limite_diario.Value)
                {
                    _logger.LogInformation("Límite diario alcanzado para la acción. Cliente: {ClienteId}, Acción: {Accion}, Límite: {Limite}",
                        clienteId, accion, accionPuntos.limite_diario.Value);
                    throw new InvalidOperationException($"Se ha alcanzado el límite diario para la acción '{accion}'");
                }
            }

            // Calcular puntos a otorgar con bonus VIP si aplica
            int puntosOtorgar = accionPuntos.puntos;
            if (cliente.es_vip == true)
            {
                // Usar configuración para el porcentaje de bonificación
                puntosOtorgar = (int)(puntosOtorgar * (1 + _clienteSettings.BonusVipPorcentaje / 100m));
                _logger.LogDebug("Bonus VIP aplicado. Cliente: {ClienteId}, Porcentaje: {Porcentaje}%, Puntos base: {PuntosBase}, Puntos finales: {PuntosFinal}",
                    clienteId, _clienteSettings.BonusVipPorcentaje, accionPuntos.puntos, puntosOtorgar);
            }

            return await RegistrarMovimientoPuntosAsync(clienteId, puntosOtorgar, $"accion:{accion}");
        }

        public async Task<MovimientoPuntosDto> OtorgarPuntosPorLoginDiarioAsync(int clienteId)
        {
            try
            {
                return await OtorgarPuntosPorAccionAsync(clienteId, "login_diario");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("límite diario"))
            {
                // Si ya recibió puntos hoy, retornar un mensaje específico
                throw new InvalidOperationException("Ya recibiste tus puntos por login hoy. Vuelve mañana.");
            }
        }

        public async Task<MovimientoPuntosDto> OtorgarPuntosManualesAsync(int clienteId, int cantidad, string concepto)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad de puntos a otorgar debe ser mayor a cero");

            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new KeyNotFoundException($"Cliente con ID {clienteId} no encontrado");

            return await RegistrarMovimientoPuntosAsync(clienteId, cantidad, $"manual:{concepto}");
        }

        public async Task<List<MovimientoPuntosDto>> ObtenerHistorialPuntosAsync(int clienteId, int cantidad = 10)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new KeyNotFoundException($"Cliente con ID {clienteId} no encontrado");

            var movimientos = await _movimientoPuntosRepository.GetByClienteIdAsync(clienteId, cantidad);
            return _mapper.Map<List<MovimientoPuntosDto>>(movimientos);
        }

        // Método privado para centralizar la creación y registro de movimientos de puntos
        private async Task<MovimientoPuntosDto> RegistrarMovimientoPuntosAsync(int clienteId, int cantidad, string concepto, string tipo = "ingreso")
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new KeyNotFoundException($"Cliente con ID {clienteId} no encontrado");

            // Crear movimiento de puntos
            var movimiento = new movimientos_punto
            {
                cliente_id = clienteId,
                cantidad = cantidad,
                tipo = tipo,
                concepto = concepto,
                saldo_anterior = cliente.puntos_acumulados ?? 0,
                saldo_nuevo = tipo == "ingreso"
                    ? (cliente.puntos_acumulados ?? 0) + cantidad
                    : (cliente.puntos_acumulados ?? 0) - cantidad,
                fecha = DateTime.UtcNow
            };

            // Actualizar puntos del cliente
            cliente.puntos_acumulados = movimiento.saldo_nuevo;

            // Guardar cambios
            await _movimientoPuntosRepository.AddAsync(movimiento);
            await _clienteRepository.UpdateAsync(cliente);
            await _movimientoPuntosRepository.SaveChangesAsync();
            await _clienteRepository.SaveChangesAsync();

            _logger.LogInformation("Puntos {Tipo}: Cliente: {ClienteId}, Concepto: {Concepto}, Cantidad: {Cantidad}, Nuevo saldo: {NuevoSaldo}",
                tipo, clienteId, concepto, cantidad, movimiento.saldo_nuevo);

            return _mapper.Map<MovimientoPuntosDto>(movimiento);
        }
    }
}


