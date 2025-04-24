using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services.FileStorage
{
    public interface IServerFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        bool DeleteFile(string filePath);
        bool FileExists(string filePath);
    }
}
