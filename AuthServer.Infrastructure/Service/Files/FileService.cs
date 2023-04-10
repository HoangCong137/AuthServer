using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using AuthServer.Domain.Services.Files;

namespace AuthServer.Infrastructure.Service.Files
{
    public class FileService : IFileService
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public FileService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> UploadImage(IFormFile Image)
        {

            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Images");

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = File.Create(filePath);

            await Image.CopyToAsync(stream);

            return "/Images/" + uniqueFileName;

        }
    }
}
