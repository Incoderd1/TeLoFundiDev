// AgencyPlatform.API.Controllers.CategoriasController.cs
using AgencyPlatform.Application.DTOs.Categoria;
using AgencyPlatform.Application.Interfaces.Services.Categoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Obtiene todas las categorías paginadas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 20)
        {
            var categorias = await _categoriaService.GetAllAsync(pagina, elementosPorPagina);
            return Ok(categorias);
        }

        /// <summary>
        /// Obtiene una categoría por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _categoriaService.GetByIdAsync(id);
            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        /// <summary>
        /// Busca categorías por nombre o descripción
        /// </summary>
        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> BuscarCategorias(
            [FromQuery] string termino)
        {
            if (string.IsNullOrEmpty(termino))
                return BadRequest("El término de búsqueda es requerido");

            var categorias = await _categoriaService.BuscarPorNombreAsync(termino);
            return Ok(categorias);
        }

        /// <summary>
        /// Obtiene las categorías más populares
        /// </summary>
        [HttpGet("populares")]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategoriasPopulares(
            [FromQuery] int cantidad = 5)
        {
            var categorias = await _categoriaService.GetMasPopularesAsync(cantidad);
            return Ok(categorias);
        }

        /// <summary>
        /// Obtiene estadísticas de una categoría
        /// </summary>
        [HttpGet("{id}/estadisticas")]
        public async Task<ActionResult<CategoriaEstadisticasDto>> GetEstadisticas(int id)
        {
            var estadisticas = await _categoriaService.GetEstadisticasAsync(id);
            if (estadisticas == null)
                return NotFound();

            return Ok(estadisticas);
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CategoriaDto>> CreateCategoria(CrearCategoriaDto categoriaDto)
        {
            try
            {
                var createdCategoria = await _categoriaService.CreateAsync(categoriaDto);
                return CreatedAtAction(nameof(GetCategoria), new { id = createdCategoria.Id }, createdCategoria);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCategoria(int id, ActualizarCategoriaDto categoriaDto)
        {
            if (id != categoriaDto.Id)
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo de la solicitud");

            try
            {
                var result = await _categoriaService.UpdateAsync(categoriaDto);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una categoría
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            try
            {
                var result = await _categoriaService.DeleteAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}