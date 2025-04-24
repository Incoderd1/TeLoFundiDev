// AgencyPlatform.Infrastructure.Services.Acompanantes.AcompananteService.cs
using AgencyPlatform.Application;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.DTOs.Servicio;
using AgencyPlatform.Application.DTOs.Visitas;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Repositories.Archivos;
using AgencyPlatform.Application.Interfaces.Services.Acompanantes;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AgencyPlatform.Infrastructure.Services.Acompanantes
{
    public class AcompananteService : IAcompananteService
    {
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IFotoRepository _fotoRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IArchivosService _archivosService;
        private readonly IVisitaPerfilRepository _visitaPerfilRepository;
        private readonly IContactoRepository _contactoRepository;
        private readonly IMapper _mapper;
        private readonly IAgenciaRepository _agenciaRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AcompananteService> _logger;

        private const string CACHE_KEY_POPULARES = "AcompanantesPopulares";
        private const string CACHE_KEY_DESTACADOS = "AcompanantesDestacados";
        private const string CACHE_KEY_RECIENTES = "AcompanantesRecientes";
        private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(15);


        public AcompananteService(
            IAcompananteRepository acompananteRepository,
            IFotoRepository fotoRepository,
            IServicioRepository servicioRepository,
            IUsuarioRepository usuarioRepository,
            IArchivosService archivosService,
            IVisitaPerfilRepository visitaPerfilRepository,
            IContactoRepository contactoRepository,
            IMapper mapper, IAgenciaRepository agenciaRepository,
            IMemoryCache cache,
            ILogger<AcompananteService> logger)
        {
            _acompananteRepository = acompananteRepository;
            _fotoRepository = fotoRepository;
            _servicioRepository = servicioRepository;
            _usuarioRepository = usuarioRepository;
            _archivosService = archivosService;
            _visitaPerfilRepository = visitaPerfilRepository;
            _contactoRepository = contactoRepository;
            _mapper = mapper;
           _agenciaRepository =  agenciaRepository;

            _cache = cache;
            _logger = logger;
        }

        public async Task<List<AcompananteDto>> GetAllAsync()
        {
            var acompanantes = await _acompananteRepository.GetAllAsync();
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task<AcompananteDto> GetByIdAsync(int id)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(id);
            return _mapper.Map<AcompananteDto>(acompanante);
        }

        public async Task<AcompananteDto> GetByUsuarioIdAsync(int usuarioId)
        {
            var acompanante = await _acompananteRepository.GetByUsuarioIdAsync(usuarioId);
            return _mapper.Map<AcompananteDto>(acompanante);
        }

        public async Task<int> CrearAsync(CrearAcompananteDto nuevoAcompanante, int usuarioId)
        {
            // Verificar si el usuario ya tiene un perfil de acompañante
            var existingAcompanante = await _acompananteRepository.GetByUsuarioIdAsync(usuarioId);

            // Verificar que el usuario exista
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException("El usuario no existe");

            if (existingAcompanante != null)
            {
                // En lugar de lanzar excepción, actualizar el perfil existente
                existingAcompanante.agencia_id = nuevoAcompanante.AgenciaId;
                existingAcompanante.nombre_perfil = nuevoAcompanante.NombrePerfil;
                existingAcompanante.genero = nuevoAcompanante.Genero;
                existingAcompanante.edad = nuevoAcompanante.Edad;
                existingAcompanante.descripcion = nuevoAcompanante.Descripcion;
                existingAcompanante.altura = nuevoAcompanante.Altura;
                existingAcompanante.peso = nuevoAcompanante.Peso;
                existingAcompanante.ciudad = nuevoAcompanante.Ciudad;
                existingAcompanante.pais = nuevoAcompanante.Pais;
                existingAcompanante.idiomas = nuevoAcompanante.Idiomas;
                existingAcompanante.disponibilidad = nuevoAcompanante.Disponibilidad;
                existingAcompanante.tarifa_base = nuevoAcompanante.TarifaBase;
                existingAcompanante.moneda = nuevoAcompanante.Moneda ?? "USD";

                // Actualizar campos nuevos de contacto
                existingAcompanante.telefono = nuevoAcompanante.Telefono;
                existingAcompanante.whatsapp = nuevoAcompanante.WhatsApp;
                existingAcompanante.email_contacto = nuevoAcompanante.EmailContacto;
                existingAcompanante.mostrar_telefono = true;
                existingAcompanante.mostrar_whatsapp = true;
                existingAcompanante.mostrar_email = true;

                existingAcompanante.updated_at = DateTime.UtcNow;

                await _acompananteRepository.UpdateAsync(existingAcompanante);
                await _acompananteRepository.SaveChangesAsync();

                // Agregar categorías si se proporcionaron
                if (nuevoAcompanante.CategoriaIds != null && nuevoAcompanante.CategoriaIds.Any())
                {
                    foreach (var categoriaId in nuevoAcompanante.CategoriaIds)
                    {
                        // Verificar si ya tiene la categoría para evitar duplicados
                        if (!await _acompananteRepository.TieneCategoriaAsync(existingAcompanante.id, categoriaId))
                        {
                            await _acompananteRepository.AgregarCategoriaAsync(existingAcompanante.id, categoriaId);
                        }
                    }
                }

                return existingAcompanante.id;
            }
            else
            {
                // Crear el nuevo acompañante (código original)
                var acompanante = new acompanante
                {
                    usuario_id = usuarioId,
                    agencia_id = nuevoAcompanante.AgenciaId,
                    nombre_perfil = nuevoAcompanante.NombrePerfil,
                    genero = nuevoAcompanante.Genero,
                    edad = nuevoAcompanante.Edad,
                    descripcion = nuevoAcompanante.Descripcion,
                    altura = nuevoAcompanante.Altura,
                    peso = nuevoAcompanante.Peso,
                    ciudad = nuevoAcompanante.Ciudad,
                    pais = nuevoAcompanante.Pais,
                    idiomas = nuevoAcompanante.Idiomas,
                    disponibilidad = nuevoAcompanante.Disponibilidad,
                    tarifa_base = nuevoAcompanante.TarifaBase,
                    moneda = nuevoAcompanante.Moneda ?? "USD",

                    // Campos nuevos de contacto
                    telefono = nuevoAcompanante.Telefono,
                    whatsapp = nuevoAcompanante.WhatsApp,
                    email_contacto = nuevoAcompanante.EmailContacto,
                    mostrar_telefono = true,
                    mostrar_whatsapp = true,
                    mostrar_email = true,

                    esta_verificado = false,
                    esta_disponible = true,
                    created_at = DateTime.UtcNow,
                    score_actividad = 0
                };

                await _acompananteRepository.AddAsync(acompanante);
                await _acompananteRepository.SaveChangesAsync();

                // Agregar categorías si se proporcionaron
                if (nuevoAcompanante.CategoriaIds != null && nuevoAcompanante.CategoriaIds.Any())
                {
                    foreach (var categoriaId in nuevoAcompanante.CategoriaIds)
                    {
                        await _acompananteRepository.AgregarCategoriaAsync(acompanante.id, categoriaId);
                    }
                }

                return acompanante.id;
            }
        }
        public async Task ActualizarAsync(UpdateAcompananteDto acompananteActualizado, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteActualizado.Id);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para actualizar este perfil");

            // Actualizar propiedades
            if (!string.IsNullOrEmpty(acompananteActualizado.NombrePerfil))
                acompanante.nombre_perfil = acompananteActualizado.NombrePerfil;

            if (!string.IsNullOrEmpty(acompananteActualizado.Genero))
                acompanante.genero = acompananteActualizado.Genero;

            if (acompananteActualizado.Edad.HasValue)
                acompanante.edad = acompananteActualizado.Edad;

            if (acompananteActualizado.Descripcion != null)
                acompanante.descripcion = acompananteActualizado.Descripcion;

            if (acompananteActualizado.Altura.HasValue)
                acompanante.altura = acompananteActualizado.Altura;

            if (acompananteActualizado.Peso.HasValue)
                acompanante.peso = acompananteActualizado.Peso;

            if (!string.IsNullOrEmpty(acompananteActualizado.Ciudad))
                acompanante.ciudad = acompananteActualizado.Ciudad;

            if (!string.IsNullOrEmpty(acompananteActualizado.Pais))
                acompanante.pais = acompananteActualizado.Pais;

            if (!string.IsNullOrEmpty(acompananteActualizado.Idiomas))
                acompanante.idiomas = acompananteActualizado.Idiomas;

            if (!string.IsNullOrEmpty(acompananteActualizado.Disponibilidad))
                acompanante.disponibilidad = acompananteActualizado.Disponibilidad;

            if (acompananteActualizado.TarifaBase.HasValue)
                acompanante.tarifa_base = acompananteActualizado.TarifaBase;

            if (!string.IsNullOrEmpty(acompananteActualizado.Moneda))
                acompanante.moneda = acompananteActualizado.Moneda;

            if (acompananteActualizado.EstaDisponible.HasValue)
                acompanante.esta_disponible = acompananteActualizado.EstaDisponible;

            // Solo administradores y agencias pueden cambiar la agencia
            if (acompananteActualizado.AgenciaId.HasValue && (rolId == 1 || rolId == 2))
            {
                acompanante.agencia_id = acompananteActualizado.AgenciaId;
                // Al cambiar de agencia, se pierde la verificación
                if (acompanante.agencia_id != acompananteActualizado.AgenciaId)
                {
                    acompanante.esta_verificado = false;
                    acompanante.fecha_verificacion = null;
                }
            }

            acompanante.updated_at = DateTime.UtcNow;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
        }



        public async Task EliminarAsync(int id, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(id);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para eliminar este perfil");

            // Eliminar fotos asociadas
            var fotos = await _fotoRepository.GetByAcompananteIdAsync(id);
            foreach (var foto in fotos)
            {
                // Eliminar archivo físico
                await _archivosService.EliminarArchivoAsync(foto.url);
                await _fotoRepository.DeleteAsync(foto);
            }

            // Eliminar servicios asociados
            var servicios = await _servicioRepository.GetByAcompananteIdAsync(id);
            foreach (var servicio in servicios)
            {
                await _servicioRepository.DeleteAsync(servicio);
            }

            // Eliminar categorías asociadas
            var categorias = await _acompananteRepository.GetCategoriasByAcompananteIdAsync(id);
            foreach (var categoria in categorias)
            {
                await _acompananteRepository.EliminarCategoriaAsync(id, categoria.categoria_id);
            }

            // Finalmente eliminar el acompañante
            await _acompananteRepository.DeleteAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
        }

        public async Task<int> AgregarFotoAsync(int acompananteId, AgregarFotoDto fotoDto, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para agregar fotos a este perfil");

            // Guardar el archivo
            var url = await _archivosService.GuardarArchivoAsync(fotoDto.Archivo);

            // Si es la primera foto o se marca como principal
            bool esPrincipal = fotoDto.EsPrincipal;
            if (!esPrincipal)
            {
                var fotos = await _fotoRepository.GetByAcompananteIdAsync(acompananteId);
                if (fotos.Count == 0)
                    esPrincipal = true;
            }

            // Si es principal, quitar el estado principal de las demás fotos
            if (esPrincipal)
            {
                await _fotoRepository.QuitarFotosPrincipalesAsync(acompananteId);
            }

            // Crear la foto en la base de datos
            var foto = new foto
            {
                acompanante_id = acompananteId,
                url = url,
                es_principal = esPrincipal,
                orden = fotoDto.Orden,
                created_at = DateTime.UtcNow
            };

            await _fotoRepository.AddAsync(foto);
            await _fotoRepository.SaveChangesAsync();

            return foto.id;
        }

        public async Task EliminarFotoAsync(int fotoId, int usuarioId, int rolId)
        {
            var foto = await _fotoRepository.GetByIdAsync(fotoId);
            if (foto == null)
                throw new InvalidOperationException("Foto no encontrada");

            var acompanante = await _acompananteRepository.GetByIdAsync(foto.acompanante_id);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para eliminar fotos de este perfil");

            // Si es la foto principal, no permitir eliminar si es la única
            if (acompanante.esta_verificado == true)
            {
                var fotos = await _fotoRepository.GetByAcompananteIdAsync(foto.acompanante_id);
                if (fotos.Count <= 1)
                    throw new InvalidOperationException("No se puede eliminar la única foto del perfil");
            }

            // Eliminar archivo físico
            await _archivosService.EliminarArchivoAsync(foto.url);

            // Eliminar de la base de datos
            await _fotoRepository.DeleteAsync(foto);
            await _fotoRepository.SaveChangesAsync();

            // Si era la principal, establecer otra como principal
            if (acompanante.esta_verificado == true)
            {
                var nuevasPrincipales = await _fotoRepository.GetByAcompananteIdAsync(foto.acompanante_id);
                if (nuevasPrincipales.Any())
                {
                    var primera = nuevasPrincipales.First();
                    primera.es_principal = true;
                    await _fotoRepository.UpdateAsync(primera);
                    await _fotoRepository.SaveChangesAsync();
                }
            }
        }

        public async Task EstablecerFotoPrincipalAsync(int acompananteId, int fotoId, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para modificar este perfil");

            var foto = await _fotoRepository.GetByIdAsync(fotoId);
            if (foto == null || foto.acompanante_id != acompananteId)
                throw new InvalidOperationException("Foto no encontrada o no pertenece al acompañante");

            // Quitar el estado principal de las demás fotos
            await _fotoRepository.QuitarFotosPrincipalesAsync(acompananteId);

            // Establecer esta foto como principal
            foto.es_principal = true;
            await _fotoRepository.UpdateAsync(foto);
            await _fotoRepository.SaveChangesAsync();
        }

        public async Task<int> AgregarServicioAsync(int acompananteId, AgregarServicioDto servicioDto, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para agregar servicios a este perfil");

            // Crear el servicio
            var servicio = new servicio
            {
                acompanante_id = acompananteId,
                nombre = servicioDto.Nombre,
                descripcion = servicioDto.Descripcion,
                precio = servicioDto.Precio,
                duracion_minutos = servicioDto.DuracionMinutos,
                created_at = DateTime.UtcNow
            };

            await _servicioRepository.AddAsync(servicio);
            await _servicioRepository.SaveChangesAsync();

            return servicio.id;
        }

        public async Task ActualizarServicioAsync(int servicioId, ActualizarServicioDto servicioDto, int usuarioId, int rolId)
        {
            var servicio = await _servicioRepository.GetByIdAsync(servicioId);
            if (servicio == null)
                throw new InvalidOperationException("Servicio no encontrado");

            var acompanante = await _acompananteRepository.GetByIdAsync(servicio.acompanante_id);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para modificar servicios de este perfil");

            // Actualizar el servicio
            servicio.nombre = servicioDto.Nombre;
            servicio.descripcion = servicioDto.Descripcion;
            servicio.precio = servicioDto.Precio;
            servicio.duracion_minutos = servicioDto.DuracionMinutos;
            servicio.updated_at = DateTime.UtcNow;

            await _servicioRepository.UpdateAsync(servicio);
            await _servicioRepository.SaveChangesAsync();
        }

        public async Task EliminarServicioAsync(int servicioId, int usuarioId, int rolId)
        {
            var servicio = await _servicioRepository.GetByIdAsync(servicioId);
            if (servicio == null)
                throw new InvalidOperationException("Servicio no encontrado");

            var acompanante = await _acompananteRepository.GetByIdAsync(servicio.acompanante_id);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para eliminar servicios de este perfil");

            await _servicioRepository.DeleteAsync(servicio);
            await _servicioRepository.SaveChangesAsync();
        }

        public async Task AgregarCategoriaAsync(int acompananteId, int categoriaId, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para modificar categorías de este perfil");

            // Verificar si ya tiene la categoría
            bool tieneCat = await _acompananteRepository.TieneCategoriaAsync(acompananteId, categoriaId);
            if (tieneCat)
                throw new InvalidOperationException("El acompañante ya tiene asignada esta categoría");

            await _acompananteRepository.AgregarCategoriaAsync(acompananteId, categoriaId);
        }
        public async Task EliminarCategoriaAsync(int acompananteId, int categoriaId, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para modificar categorías de este perfil");

            // Verificar si tiene la categoría
            bool tieneCat = await _acompananteRepository.TieneCategoriaAsync(acompananteId, categoriaId);
            if (!tieneCat)
                throw new InvalidOperationException("El acompañante no tiene asignada esta categoría");

            await _acompananteRepository.EliminarCategoriaAsync(acompananteId, categoriaId);
        }

        public async Task<List<AcompananteDto>> BuscarAsync(AcompananteFiltroDto filtro)
        {
            var acompanantes = await _acompananteRepository.BuscarAsync(
                filtro.Busqueda,
                filtro.Ciudad,
                filtro.Pais,
                filtro.Genero,
                filtro.EdadMinima,
                filtro.EdadMaxima,
                filtro.TarifaMinima,
                filtro.TarifaMaxima,
                filtro.SoloVerificados,
                filtro.SoloDisponibles,
                filtro.CategoriaIds,
                filtro.OrdenarPor,
                filtro.Pagina,
                filtro.ElementosPorPagina);

            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }
      
        public async Task<List<AcompananteDto>> GetDestacadosAsync()
        {
            var acompanantes = await _acompananteRepository.GetDestacadosAsync();
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task<List<AcompananteDto>> GetRecientesAsync(int cantidad = 10)
        {
            var acompanantes = await _acompananteRepository.GetRecientesAsync(cantidad);
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task<List<AcompananteDto>> GetPopularesAsync(int cantidad = 10)
        {
            var acompanantes = await _acompananteRepository.GetPopularesAsync(cantidad);
            return _mapper.Map<List<AcompananteDto>>(acompanantes);
        }

        public async Task<bool> EstaVerificadoAsync(int acompananteId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            return acompanante != null && acompanante.esta_verificado == true;
        }

        public async Task VerificarAcompananteAsync(int acompananteId, int agenciaId, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Solo agencias o administradores pueden verificar
            if (rolId != 1 && rolId != 2)
                throw new UnauthorizedAccessException("No tienes permisos para verificar acompañantes");

            // Si es agencia, solo puede verificar acompañantes de su agencia
            if (rolId == 2 && acompanante.agencia_id != agenciaId)
                throw new UnauthorizedAccessException("Solo puedes verificar acompañantes de tu agencia");

            // Verificar el acompañante
            acompanante.esta_verificado = true;
            acompanante.fecha_verificacion = DateTime.UtcNow;
            acompanante.updated_at = DateTime.UtcNow;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();

            // Aquí se podría crear un registro en la tabla verificaciones
        }

        public async Task RevocarVerificacionAsync(int acompananteId, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Solo agencias o administradores pueden revocar verificación
            if (rolId != 1 && rolId != 2)
                throw new UnauthorizedAccessException("No tienes permisos para revocar verificación");

            // Si es agencia, solo puede modificar acompañantes de su agencia
            if (rolId == 2 && acompanante.agencia_id != usuarioId)
                throw new UnauthorizedAccessException("Solo puedes modificar acompañantes de tu agencia");

            // Revocar verificación
            acompanante.esta_verificado = false;
            acompanante.fecha_verificacion = null;
            acompanante.updated_at = DateTime.UtcNow;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
        }

      


        public async Task RegistrarVisitaAsync(int acompananteId, string ipVisitante, string userAgent, int? clienteId = null)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Crear registro de visita
            var visita = new visitas_perfil
            {
                acompanante_id = acompananteId,
                cliente_id = clienteId,
                ip_visitante = ipVisitante,
                user_agent = userAgent,
                fecha_visita = DateTime.UtcNow,
                duracion_segundos = 0, // Se podría actualizar con otra llamada al cerrar la página
                created_at = DateTime.UtcNow
            };

            await _visitaPerfilRepository.AddAsync(visita);
            await _visitaPerfilRepository.SaveChangesAsync();

            // Actualizar score de actividad
            long nuevoScore = await CalcularScoreActividadAsync(acompananteId);
            await _acompananteRepository.ActualizarScoreActividadAsync(acompananteId, nuevoScore);
        }

        public async Task RegistrarContactoAsync(int acompananteId, string tipoContacto, string ipContacto, int? clienteId = null)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Validar tipo de contacto
            if (!new[] { "telefono", "whatsapp", "email" }.Contains(tipoContacto.ToLower()))
                throw new InvalidOperationException("Tipo de contacto inválido");

            // Crear registro de contacto
            var contacto = new contacto
            {
                acompanante_id = acompananteId,
                cliente_id = clienteId,
                tipo_contacto = tipoContacto.ToLower(),
                fecha_contacto = DateTime.UtcNow,
                esta_registrado = clienteId.HasValue,
                ip_contacto = ipContacto,
                created_at = DateTime.UtcNow
            };

            await _contactoRepository.AddAsync(contacto);
            await _contactoRepository.SaveChangesAsync();

            // Actualizar score de actividad (los contactos valen más que las visitas)
            long nuevoScore = await CalcularScoreActividadAsync(acompananteId);
            await _acompananteRepository.ActualizarScoreActividadAsync(acompananteId, nuevoScore);
        }

        public async Task<AcompananteEstadisticasDto> GetEstadisticasAsync(int acompananteId, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos (solo el dueño, su agencia o admin pueden ver estadísticas)
            if (!TienePermisosEstadisticas(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para ver estadísticas de este perfil");

            // Obtener datos de contactos y visitas
            var totalVisitas = await _visitaPerfilRepository.GetTotalByAcompananteIdAsync(acompananteId);
            var totalContactos = await _contactoRepository.GetTotalByAcompananteIdAsync(acompananteId);
            var contactosPorTipo = await _contactoRepository.GetContactosPorTipoAsync(acompananteId);
            var visitasPorDia = await _visitaPerfilRepository.GetVisitasPorDiaAsync(acompananteId, 30); // Últimos 30 días

            var estadisticas = new AcompananteEstadisticasDto
            {
                Id = acompanante.id,
                NombrePerfil = acompanante.nombre_perfil,
                TotalVisitas = totalVisitas,
                TotalContactos = totalContactos,
                ScoreActividad = acompanante.score_actividad ?? 0,
                ContactosPorTipo = contactosPorTipo.ToDictionary(c => c.Key, c => c.Value),
                VisitasPorDia = visitasPorDia.Select(v => new VisitaDiaDto
                {
                    Fecha = v.Key,
                    CantidadVisitas = v.Value
                }).ToList()
            };

            return estadisticas;
        }

        public async Task CambiarDisponibilidadAsync(int acompananteId, bool estaDisponible, int usuarioId, int rolId)
        {
            var acompanante = await _acompananteRepository.GetByIdAsync(acompananteId);
            if (acompanante == null)
                throw new InvalidOperationException("Acompañante no encontrado");

            // Verificar permisos
            if (!TienePermisosEdicion(acompanante, usuarioId, rolId))
                throw new UnauthorizedAccessException("No tienes permisos para modificar este perfil");

            acompanante.esta_disponible = estaDisponible;
            acompanante.updated_at = DateTime.UtcNow;

            await _acompananteRepository.UpdateAsync(acompanante);
            await _acompananteRepository.SaveChangesAsync();
        }

        // Métodos privados auxiliares
        private bool TienePermisosEdicion(acompanante acompanante, int usuarioId, int rolId)
        {
            // Admin (rolId = 1) siempre tiene permisos
            if (rolId == 1) return true;

            // Es el propio acompañante
            if (acompanante.usuario_id == usuarioId) return true;

            // Es una agencia que gestiona al acompañante
            if (rolId == 2 && acompanante.agencia_id.HasValue && acompanante.agencia_id == usuarioId)
                return true;

            return false;
        }

        private bool TienePermisosEstadisticas(acompanante acompanante, int usuarioId, int rolId)
        {
            return TienePermisosEdicion(acompanante, usuarioId, rolId);
        }

        private async Task<long> CalcularScoreActividadAsync(int acompananteId)
        {
            // Pesos para el cálculo
            const int PESO_VISITA = 1;
            const int PESO_CONTACTO = 5;

            // Período de cálculo (últimos 30 días)
            var fechaInicio = DateTime.UtcNow.AddDays(-30);

            // Obtener conteos
            int visitas = await _visitaPerfilRepository.GetTotalDesdeAsync(acompananteId, fechaInicio);
            int contactos = await _contactoRepository.GetTotalDesdeAsync(acompananteId, fechaInicio);

            // Calcular score
            long score = (visitas * PESO_VISITA) + (contactos * PESO_CONTACTO);

            return score;
        }


        // AcompananteService.cs
        // Añade estos métodos a la implementación existente

        public async Task<PaginatedResultDto<AcompananteDto>> GetAllPaginatedAsync(int pageNumber, int pageSize)
        {
            // Verificar parámetros de paginación
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Obtener total de registros y la página solicitada
            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetAllPaginatedAsync(skip, pageSize);

            return new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResultDto<AcompananteDto>> GetRecientesPaginadosAsync(int pageNumber, int pageSize)
        {
            // Verificar parámetros de paginación
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            string cacheKey = $"RecientesPaginados_{pageNumber}_{pageSize}";

            // Intentar obtener de la caché
            if (_cache.TryGetValue(cacheKey, out PaginatedResultDto<AcompananteDto> cachedResult))
            {
                _logger.LogInformation("Obteniendo acompañantes recientes paginados desde la caché. Página: {Page}, Tamaño: {Size}",
                    pageNumber, pageSize);
                return cachedResult;
            }

            // Obtener total de registros y la página solicitada
            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetRecientesPaginadosAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Guardar en caché por 5 minutos (los recientes cambian con frecuencia)
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<PaginatedResultDto<AcompananteDto>> GetPopularesPaginadosAsync(int pageNumber, int pageSize)
        {
            // Verificar parámetros de paginación
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            string cacheKey = $"PopularesPaginados_{pageNumber}_{pageSize}";

            // Intentar obtener de la caché
            if (_cache.TryGetValue(cacheKey, out PaginatedResultDto<AcompananteDto> cachedResult))
            {
                _logger.LogInformation("Obteniendo acompañantes populares paginados desde la caché. Página: {Page}, Tamaño: {Size}",
                    pageNumber, pageSize);
                return cachedResult;
            }

            // Obtener total de registros y la página solicitada
            var total = await _acompananteRepository.CountAsync(a => a.esta_disponible == true);
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetPopularesPaginadosAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Guardar en caché por 15 minutos
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }

        public async Task<PaginatedResultDto<AcompananteDto>> GetDestacadosPaginadosAsync(int pageNumber, int pageSize)
        {
            // Verificar parámetros de paginación
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            string cacheKey = $"DestacadosPaginados_{pageNumber}_{pageSize}";

            // Intentar obtener de la caché
            if (_cache.TryGetValue(cacheKey, out PaginatedResultDto<AcompananteDto> cachedResult))
            {
                _logger.LogInformation("Obteniendo acompañantes destacados paginados desde la caché. Página: {Page}, Tamaño: {Size}",
                    pageNumber, pageSize);
                return cachedResult;
            }

            // Obtener total de registros y la página solicitada
            var total = await _acompananteRepository.CountDestacadosAsync();
            var skip = (pageNumber - 1) * pageSize;

            var acompanantes = await _acompananteRepository.GetDestacadosPaginadosAsync(skip, pageSize);

            var result = new PaginatedResultDto<AcompananteDto>
            {
                Items = _mapper.Map<List<AcompananteDto>>(acompanantes),
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Guardar en caché por 15 minutos
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }

    }
}


