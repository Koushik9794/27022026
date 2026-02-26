using System.IO;
using System.Threading.Tasks;

namespace FileService.Application.Interfaces
{
    public interface IStorageProvider
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> GetFileStreamAsync(string filePath);
        Task DeleteFileAsync(string filePath);
    }
}
