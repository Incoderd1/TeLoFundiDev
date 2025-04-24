using AgencyPlatform.Application.DTOs.Categoria;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Services.Categoria;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Categoria
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IAcompananteRepository _acompananteRepository;
        private readonly IMapper _mapper;

        public CategoriaService(
            ICategoriaRepository categoriaRepository,
            IAcompananteRepository acompananteRepository,
            IMapper mapper)
        {
            _categoriaRepository = categoriaRepository;
            _acompananteRepository = acompananteRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoriaDto>> GetAllAsync(int pagina = 1, int elementosPorPagina = 20)
        {
            var skip = (pagina - 1) * elementosPorPagina;

            var categorias = await _categoriaRepository.GetPagedAsync(skip, elementosPorPagina);
            return _mapper.Map<List<CategoriaDto>>(categorias);
        }

        public async Task<CategoriaDto> GetByIdAsync(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
                return null;

            return _mapper.Map<CategoriaDto>(categoria);
        }

        public async Task<CategoriaDto> CreateAsync(CrearCategoriaDto categoriaDto)
        {
            // Verificar si ya existe una categoría con ese nombre
            if (await ExisteNombreAsync(categoriaDto.Nombre))
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{categoriaDto.Nombre}'");

            var categoria = new categoria
            {
                nombre = categoriaDto.Nombre,
                descripcion = categoriaDto.Descripcion,
                created_at = DateTime.UtcNow
            };

            await _categoriaRepository.AddAsync(categoria);
            await _categoriaRepository.SaveChangesAsync();

            return _mapper.Map<CategoriaDto>(categoria);
        }

        public async Task<bool> UpdateAsync(ActualizarCategoriaDto categoriaDto)
        {
            // Verificar si existe la categoría
            var existingCategoria = await _categoriaRepository.GetByIdAsync(categoriaDto.Id);
            if (existingCategoria == null)
                return false;

            // Verificar si el nuevo nombre ya existe (excepto para esta misma categoría)
            if (await ExisteNombreAsync(categoriaDto.Nombre, categoriaDto.Id))
                throw new InvalidOperationException($"Ya existe otra categoría con el nombre '{categoriaDto.Nombre}'");

            existingCategoria.nombre = categoriaDto.Nombre;
            existingCategoria.descripcion = categoriaDto.Descripcion;
            existingCategoria.updated_at = DateTime.UtcNow;

            await _categoriaRepository.UpdateAsync(existingCategoria);
            await _categoriaRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
                return false;

            // Verificar si hay acompañantes usando esta categoría
            var tieneAcompanantes = await _acompananteRepository.TieneAcompanantesAsync(id);
            if (tieneAcompanantes)
                throw new InvalidOperationException("No se puede eliminar la categoría porque está siendo utilizada por uno o más acompañantes");

            await _categoriaRepository.DeleteAsync(categoria);
            await _categoriaRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null)
        {
            return await _categoriaRepository.ExisteNombreAsync(nombre, exceptoId);
        }

        public async Task<CategoriaEstadisticasDto> GetEstadisticasAsync(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
                return null;

            var totalAcompanantes = await _categoriaRepository.GetTotalAcompanantesAsync(id);
            var totalVisitas = await _categoriaRepository.GetTotalVisitasAsync(id);

            return new CategoriaEstadisticasDto
            {
                Id = categoria.id,
                Nombre = categoria.nombre,
                TotalAcompanantes = totalAcompanantes,
                TotalVisitas = totalVisitas
            };
        }

        public async Task<List<CategoriaDto>> GetMasPopularesAsync(int cantidad = 5)
        {
            var categorias = await _categoriaRepository.GetMasPopularesAsync(cantidad);
            return _mapper.Map<List<CategoriaDto>>(categorias);
        }

        public async Task<List<CategoriaDto>> BuscarPorNombreAsync(string termino)
        {
            var categorias = await _categoriaRepository.BuscarPorNombreAsync(termino);
            return _mapper.Map<List<CategoriaDto>>(categorias);
        }
    }
}