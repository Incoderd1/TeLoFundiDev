using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Storage
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        bool DeleteFile(string filePath);
        bool FileExists(string filePath);
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            // 📁 Ahora guarda en wwwroot/uploads para que sea accesible públicamente
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            _logger = logger;

            // Crear carpeta base si no existe
            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            try
            {
                _logger.LogInformation("Iniciando guardado de archivo {FileName} en carpeta {Folder}",
                    file?.FileName, folder);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Se intentó guardar un archivo nulo o vacío");
                    throw new ArgumentException("No se proporcionó un archivo válido");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                _logger.LogInformation("Verificando extensión: {Extension}", extension);

                if (string.IsNullOrEmpty(extension) || !Array.Exists(permittedExtensions, e => e == extension))
                {
                    _logger.LogWarning("Extensión no permitida: {Extension}", extension);
                    throw new InvalidOperationException("El archivo debe ser una imagen válida");
                }

                var nombreArchivo = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
                var rutaCarpeta = Path.Combine(_basePath, folder);

                _logger.LogInformation("Verificando si la carpeta existe: {RutaCarpeta}", rutaCarpeta);

                // Verificar explícitamente y crear con manejo de excepciones
                try
                {
                    if (!Directory.Exists(rutaCarpeta))
                    {
                        _logger.LogInformation("Creando carpeta: {RutaCarpeta}", rutaCarpeta);
                        Directory.CreateDirectory(rutaCarpeta);
                    }
                }
                catch (Exception dirEx)
                {
                    _logger.LogError(dirEx, "Error al crear directorio {RutaCarpeta}", rutaCarpeta);
                    throw new IOException($"No se pudo crear el directorio: {rutaCarpeta}", dirEx);
                }

                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
                _logger.LogInformation("Guardando archivo en: {RutaCompleta}", rutaCompleta);

                try
                {
                    using var stream = new FileStream(rutaCompleta, FileMode.Create);
                    await file.CopyToAsync(stream);
                }
                catch (Exception fileEx)
                {
                    _logger.LogError(fileEx, "Error al escribir archivo en {RutaCompleta}", rutaCompleta);
                    throw new IOException($"No se pudo escribir el archivo: {rutaCompleta}", fileEx);
                }

                _logger.LogInformation("Archivo guardado exitosamente: {NombreArchivo}", nombreArchivo);
                return $"/uploads/{folder}/{nombreArchivo}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo {Filename} en carpeta {Carpeta}", file?.FileName, folder);

                // Aquí podrías devolver una URL de imagen predeterminada en lugar de propagar la excepción
                // return "/images/no-image.jpg";

                // O propagar con más información
                throw new ApplicationException($"Error al procesar el archivo {file?.FileName}: {ex.Message}", ex);
            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                var relativePath = filePath.TrimStart('/');
                var fullPath = Path.Combine(_basePath, relativePath.Replace("uploads/", ""));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo {FilePath}", filePath);
                return false;
            }
        }

        public bool FileExists(string filePath)
        {
            try
            {
                var relativePath = filePath.TrimStart('/');
                var fullPath = Path.Combine(_basePath, relativePath.Replace("uploads/", ""));

                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si existe el archivo {FilePath}", filePath);
                return false;
            }
        }
    }
}
