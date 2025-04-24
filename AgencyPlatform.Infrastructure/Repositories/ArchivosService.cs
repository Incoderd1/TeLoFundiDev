using AgencyPlatform.Application.Interfaces.Repositories.Archivos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AgencyPlatform.Infrastructure.Repositories
{
    public class ArchivosService : IArchivosService
    {
        private readonly IConfiguration _config;
        private readonly string _carpetaArchivos = "uploads/fotos";
        private readonly string _rutaBase;

        public ArchivosService(IConfiguration config)
        {
            _config = config;

            // Obtener la ruta base de la configuración o usar una predeterminada
            _rutaBase = _config["AppSettings:RutaArchivos"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

            // Asegurar que la carpeta exista
            string rutaCarpeta = Path.Combine(_rutaBase, _carpetaArchivos);
            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);
        }

        public async Task<string> GuardarArchivoAsync(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("No se ha proporcionado un archivo válido");

            // Verificar el tipo de archivo (solo permitir imágenes)
            string extension = Path.GetExtension(archivo.FileName).ToLower();
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
                throw new ArgumentException("Tipo de archivo no permitido. Solo se admiten imágenes.");

            // Crear carpeta si no existe
            string rutaCarpeta = Path.Combine(_rutaBase, _carpetaArchivos);
            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            // Generar nombre único
            string nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
            string rutaRelativa = Path.Combine(_carpetaArchivos, nombreArchivo).Replace("\\", "/");

            // Guardar archivo
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return rutaRelativa;
        }

        public async Task<bool> EliminarArchivoAsync(string ruta)
        {
            if (string.IsNullOrEmpty(ruta))
                return false;

            string rutaCompleta = Path.Combine(_rutaBase, ruta.TrimStart('/'));

            if (File.Exists(rutaCompleta))
            {
                try
                {
                    File.Delete(rutaCompleta);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public async Task<string> ObtenerUrlCompleta(string rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa))
                return null;

            string baseUrl = _config["AppSettings:BaseUrl"];
            return $"{baseUrl.TrimEnd('/')}/{rutaRelativa.TrimStart('/')}";
        }
    }
}
