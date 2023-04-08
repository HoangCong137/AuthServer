using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Service.Files
{
    public interface IFileService
    {
        Task<string> UploadImage(IFormFile Image);
    }
}
