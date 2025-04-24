using AgencyPlatform.Application.DTOs.Categoria;

namespace AgencyPlatform.Application.Interfaces.Services.Categoria
{
    public interface ICategoriaService
    {
        // Operaciones básicas
        Task<List<CategoriaDto>> GetAllAsync(int pagina = 1, int elementosPorPagina = 20);
        Task<CategoriaDto> GetByIdAsync(int id);
        Task<CategoriaDto> CreateAsync(CrearCategoriaDto categoriaDto);
        Task<bool> UpdateAsync(ActualizarCategoriaDto categoriaDto);
        Task<bool> DeleteAsync(int id);

        // Operaciones avanzadas
        Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null);
        Task<CategoriaEstadisticasDto> GetEstadisticasAsync(int id);
        Task<List<CategoriaDto>> GetMasPopularesAsync(int cantidad = 5);
        Task<List<CategoriaDto>> BuscarPorNombreAsync(string termino);
    }
}
