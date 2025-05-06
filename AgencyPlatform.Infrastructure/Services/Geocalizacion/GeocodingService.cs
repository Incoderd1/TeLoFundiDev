using AgencyPlatform.Application.Interfaces.Services.Geocalizacion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.Geocalizacion
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<GeocodingService> _logger;
        private readonly HttpClient _httpClient;

        public GeocodingService(
            IMemoryCache cache,
            ILogger<GeocodingService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("GeoCoding");
        }

        public async Task<(double latitude, double longitude)?> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogWarning("Dirección vacía proporcionada para geocodificación.");
                return null;
            }

            // Intentar obtener de caché primero
            string cacheKey = $"geocode_{address.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out (double latitude, double longitude) coordinates))
            {
                _logger.LogInformation("Coordenadas obtenidas desde caché para dirección: {Address}.", address);
                return coordinates;
            }

            try
            {
                _logger.LogWarning("Geocodificación de direcciones no implementada. Dirección: {Address}.", address);
                return null;

                // Ejemplo con Google Maps API (descomentar y configurar con una API Key real si lo necesitas):
                /*
                var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GoogleGeocodeResponse>(json);

                if (result.Status == "OK" && result.Results.Any())
                {
                    var location = result.Results[0].Geometry.Location;
                    coordinates = (location.Lat, location.Lng);
                    _cache.Set(cacheKey, coordinates, TimeSpan.FromDays(30));
                    return coordinates;
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al geocodificar dirección: {Address}.", address);
                return null;
            }
        }

        public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            _logger.LogWarning("Geocodificación inversa no implementada para coordenadas: Lat={Latitude}, Lon={Longitude}.", latitude, longitude);
            return "Dirección no disponible";
        }

        public async Task<(double latitude, double longitude, string city, string country)?> GetLocationFromIpAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip) || ip == "Unknown")
            {
                _logger.LogWarning("IP no válida proporcionada para geolocalización: {Ip}.", ip);
                return null;
            }

            // Intentar obtener de caché primero
            string cacheKey = $"ip_location_{ip}";
            if (_cache.TryGetValue(cacheKey, out (double latitude, double longitude, string city, string country) location))
            {
                _logger.LogInformation("Ubicación obtenida desde caché para IP: {Ip}. Latitud={Latitude}, Longitud={Longitude}, Ciudad={City}, País={Country}.", ip, location.latitude, location.longitude, location.city, location.country);
                return location;
            }

            try
            {
                _logger.LogInformation("Realizando solicitud a ip-api.com para obtener la ubicación de la IP: {Ip}.", ip);
                var response = await _httpClient.GetAsync($"http://ip-api.com/json/{ip}?fields=status,lat,lon,city,country");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta recibida de ip-api.com para IP {Ip}: {JsonResponse}.", ip, json);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.GetProperty("status").GetString() == "success")
                {
                    double latitude = root.GetProperty("lat").GetDouble();
                    double longitude = root.GetProperty("lon").GetDouble();
                    string city = root.TryGetProperty("city", out var cityElement) ? cityElement.GetString() : "Desconocido";
                    string country = root.TryGetProperty("country", out var countryElement) ? countryElement.GetString() : "Desconocido";

                    location = (latitude, longitude, city, country);
                    _cache.Set(cacheKey, location, TimeSpan.FromDays(7));
                    _logger.LogInformation("Ubicación obtenida para IP {Ip}: Ciudad={City}, País={Country}, Latitud={Latitude}, Longitud={Longitude}.", ip, city, country, latitude, longitude);
                    return location;
                }
                else
                {
                    _logger.LogWarning("No se pudo obtener la ubicación para IP {Ip}. Respuesta de ip-api.com: {Status}.", ip, root.GetProperty("status").GetString());
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la ubicación para IP {Ip} desde ip-api.com.", ip);
                return null;
            }
        }
    }
}