using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.Geocalizacion
{
    public interface IGeocodingService
    {
        Task<(double latitude, double longitude)?> GeocodeAddressAsync(string address);
        Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude);
        Task<(double latitude, double longitude, string city, string country)?> GetLocationFromIpAsync(string ip);
    }
}
