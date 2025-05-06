using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Static
{
    public static class GeoUtils
    {
        // Radio de la Tierra en kilómetros
        private const double EarthRadiusKm = 6371.0;

        // Calcular distancia entre dos puntos usando la fórmula haversine
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        // Convertir grados a radianes
        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        // Validar coordenadas
        public static bool AreValidCoordinates(double? latitude, double? longitude)
        {
            return latitude.HasValue && longitude.HasValue &&
                   latitude.Value >= -90 && latitude.Value <= 90 &&
                   longitude.Value >= -180 && longitude.Value <= 180;
        }
    }

}