using System;
using System.IO;
using System.Threading.Tasks;
using FileService.Application.Interfaces;
using FileService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.Storage
{
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string _storagePath;

        public LocalStorageProvider(IOptions<FileValidationOptions> options)
        {
            _storagePath = string.IsNullOrWhiteSpace(options.Value.LocalStoragePath)
                ? "uploads"
                : options.Value.LocalStoragePath;

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
        {
            // Create a unique file name to prevent overwrites
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_storagePath, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            return filePath;
        }

        public Task<Stream> GetFileStreamAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found at path: {filePath}");
            }

            return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        public Task DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
