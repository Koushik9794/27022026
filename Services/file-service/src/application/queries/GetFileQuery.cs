using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileService.Application.Interfaces;

namespace FileService.Application.Queries
{
    public class GetFileQuery
    {
        public string FileName { get; set; }

        public GetFileQuery(string fileName)
        {
            FileName = fileName;
        }
    }

    public class GetFileQueryHandler
    {
        private readonly IStorageProvider _storageProvider;

        public GetFileQueryHandler(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<Stream> Handle(GetFileQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query.FileName))
            {
                throw new ArgumentException("File name is required.");
            }

            // In a real scenario, we might want to resolve the path from a database using metadata ID.
            // For now, we assume the stored name is passed and we can construct the path or it's already the path.
            // Based on LocalStorageProvider, it stores files in a configured directory.
            
            // If the query.FileName is just the name, we might need more logic.
            // But IStorageProvider.GetFileStreamAsync(string filePath) expects a path.
            
            return await _storageProvider.GetFileStreamAsync(query.FileName);
        }
    }
}
