using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AgencyPlatform.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class BusquedaRepository : IBusquedaRepository
    {
        private readonly AgencyPlatformDbContext _context;
        private readonly ILogger<BusquedaRepository> _logger;

        public BusquedaRepository(AgencyPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveSearchAsync(busqueda_guardada busqueda)
        {
            await _context.busqueda_guardadas.AddAsync(busqueda);
            await _context.SaveChangesAsync();
            return busqueda.id;
        }

        public async Task<List<busqueda_guardada>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.busqueda_guardadas
                .Where(b => b.usuario_id == usuarioId)
                .OrderByDescending(b => b.fecha_ultimo_uso ?? b.fecha_creacion)
                .ToListAsync();
        }

        public async Task<busqueda_guardada?> GetByIdAsync(int id)
        {
            return await _context.busqueda_guardadas.FindAsync(id);
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            var busqueda = await _context.busqueda_guardadas
                .FirstOrDefaultAsync(b => b.id == id && b.usuario_id == usuarioId);

            if (busqueda == null)
                return false;

            _context.busqueda_guardadas.Remove(busqueda);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLastUsedAsync(int id)
        {
            var busqueda = await _context.busqueda_guardadas.FindAsync(id);
            if (busqueda == null)
                return false;

            busqueda.fecha_ultimo_uso = DateTime.UtcNow;
            busqueda.veces_usada++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task LogSearchAsync(registro_busqueda registro)
        {
            await _context.registro_busquedas.AddAsync(registro);
            await _context.SaveChangesAsync();
        }

        public async Task<List<dynamic>> GetPopularSearchTermsAsync(int count)
        {
            // Extraer términos de búsqueda de los registros y agruparlos
            var result = new List<dynamic>();

            var registros = await _context.registro_busquedas
                .OrderByDescending(r => r.fecha_busqueda)
                .Take(1000) // Analizar los últimos 1000 registros
                .ToListAsync();

            var terms = new Dictionary<string, int>();

            foreach (var registro in registros)
            {
                try
                {
                    var criterios = JsonSerializer.Deserialize<Dictionary<string, object>>(registro.criterios_json);
                    if (criterios != null && criterios.TryGetValue("SearchText", out var searchText) && searchText != null)
                    {
                        var term = searchText.ToString()?.ToLower() ?? "";
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            if (terms.ContainsKey(term))
                                terms[term]++;
                            else
                                terms[term] = 1;
                        }
                    }
                }
                catch { /* Ignorar errores de deserialización */ }
            }

            return terms.OrderByDescending(t => t.Value)
                .Take(count)
                .Select(t => new { Text = t.Key, Count = t.Value })
                .Cast<dynamic>()
                .ToList();
        }

        public async Task<int> CountAsync()
        {
            return await _context.registro_busquedas.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<string>> GetAutocompleteSuggestionsAsync(string prefix, string tipo, int count = 10)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            {
                return new List<string>();
            }

            prefix = prefix.ToLower();

            try
            {
                switch (tipo.ToLower())
                {
                    case "ciudad":
                        // Autocompletado para ciudades
                        return await _context.acompanantes
                            .Where(a => a.ciudad != null && EF.Functions.ILike(a.ciudad, $"{prefix}%"))
                            .Select(a => a.ciudad)
                            .Distinct()
                            .Take(count)
                            .ToListAsync();

                    case "categoria":
                        // Autocompletado para categorías
                        return await _context.categorias
                            .Where(c => EF.Functions.ILike(c.nombre, $"{prefix}%"))
                            .Select(c => c.nombre)
                            .Take(count)
                            .ToListAsync();

                    case "servicio":
                        // Autocompletado para servicios
                        return await _context.servicios
                            .Where(s => EF.Functions.ILike(s.nombre, $"{prefix}%"))
                            .Select(s => s.nombre)
                            .Distinct()
                            .Take(count)
                            .ToListAsync();

                    case "idioma":
                        // Crear un conjunto de idiomas comunes que coincidan con el prefijo
                        var idiomas = new List<string> {
                            "español", "inglés", "francés", "italiano", "alemán",
                            "portugués", "ruso", "chino", "japonés", "árabe",
                            "coreano", "hindi", "holandés", "griego", "turco"
                        };

                        return idiomas
                            .Where(i => i.ToLower().StartsWith(prefix))
                            .Take(count)
                            .ToList();

                    case "genero":
                        // Autocompletado para género (opciones fijas)
                        var generos = new List<string> {
                            "femenino", "masculino", "trans", "no binario"
                        };

                        return generos
                            .Where(g => g.ToLower().StartsWith(prefix))
                            .Take(count)
                            .ToList();

                    case "termino":
                        // Autocompletado para términos de búsqueda populares
                        var registros = await _context.registro_busquedas
                            .OrderByDescending(r => r.fecha_busqueda)
                            .Take(1000)
                            .ToListAsync();

                        var terms = new Dictionary<string, int>();
                        foreach (var registro in registros)
                        {
                            try
                            {
                                var criterios = JsonSerializer.Deserialize<Dictionary<string, object>>(registro.criterios_json);
                                if (criterios != null && criterios.TryGetValue("SearchText", out var searchText) && searchText != null)
                                {
                                    var term = searchText.ToString()?.ToLower() ?? "";
                                    if (!string.IsNullOrWhiteSpace(term) && term.StartsWith(prefix))
                                    {
                                        if (terms.ContainsKey(term))
                                            terms[term]++;
                                        else
                                            terms[term] = 1;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error al deserializar criterios de búsqueda");
                            }
                        }

                        return terms
                            .OrderByDescending(t => t.Value)
                            .Take(count)
                            .Select(t => t.Key)
                            .ToList();

                    default:
                        return new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sugerencias de autocompletado para '{Tipo}' con prefijo '{Prefix}'", tipo, prefix);
                return new List<string>();
            }
        }
        public async Task<List<AutocompleteSuggestion>> GetDetailedAutocompleteSuggestionsAsync(string prefix, string tipo, int count = 10)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            {
                return new List<AutocompleteSuggestion>();
            }

            prefix = prefix.ToLower();

            try
            {
                switch (tipo.ToLower())
                {
                    case "ciudad":
                        // Sugerencias de ciudades con conteo de perfiles
                        var ciudadesSugerencias = await _context.acompanantes
                            .Where(a => a.ciudad != null && EF.Functions.ILike(a.ciudad, $"{prefix}%") && a.esta_disponible == true)
                            .GroupBy(a => a.ciudad)
                            .Select(g => new AutocompleteSuggestion
                            {
                                Text = g.Key,
                                Count = g.Count(),
                                Type = "Ciudad"
                            })
                            .OrderByDescending(s => s.Count)
                            .Take(count)
                            .ToListAsync();

                        return ciudadesSugerencias;

                    case "categoria":
                        // Sugerencias de categorías con conteo de perfiles asociados
                        var categoriasSugerencias = await _context.categorias
                            .Where(c => EF.Functions.ILike(c.nombre, $"{prefix}%"))
                            .Select(c => new AutocompleteSuggestion
                            {
                                Text = c.nombre,
                                Count = c.acompanante_categoria.Count,
                                Type = "Categoría",
                                Id = c.id
                            })
                            .OrderByDescending(s => s.Count)
                            .Take(count)
                            .ToListAsync();

                        return categoriasSugerencias;

                    case "servicio":
                        // Sugerencias de servicios con conteo
                        var serviciosSugerencias = await _context.servicios
                            .Where(s => EF.Functions.ILike(s.nombre, $"{prefix}%"))
                            .GroupBy(s => s.nombre)
                            .Select(g => new AutocompleteSuggestion
                            {
                                Text = g.Key,
                                Count = g.Count(),
                                Type = "Servicio"
                            })
                            .OrderByDescending(s => s.Count)
                            .Take(count)
                            .ToListAsync();

                        return serviciosSugerencias;

                    default:
                        return new List<AutocompleteSuggestion>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sugerencias detalladas para '{Tipo}' con prefijo '{Prefix}'", tipo, prefix);
                return new List<AutocompleteSuggestion>();
            }
        }
    }

}
