using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileService.Application.Interfaces;
using FileService.Domain.Aggregates;
using FileService.Infrastructure.Configuration;
using FluentValidation;
using Wolverine;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FileService.Application.Commands
{
    public class UploadFileCommand 
    {
        public IFormFile File { get; set; }
        public string UploadedBy { get; set; } = "System";
    }

    public class UploadFileCommandHandler 
    {
        private readonly IStorageProvider _storageProvider;
        private readonly FileValidationOptions _validationOptions;

        public UploadFileCommandHandler(IStorageProvider storageProvider, IOptions<FileValidationOptions> validationOptions)
        {
            _storageProvider = storageProvider;
            _validationOptions = validationOptions.Value;
        }

        public async Task<FileMetadata> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            // Manual Validation (simple checks, full validation uses FluentValidation)
            if (request.File == null || request.File.Length == 0)
            {
                throw new ArgumentException("No file provided.");
            }

            // Size Check
            var sizeInMb = request.File.Length / 1024f / 1024f;
            if (sizeInMb > _validationOptions.MaxFileSizeMb)
            {
                throw new ArgumentException($"File size exceeds limit of {_validationOptions.MaxFileSizeMb}MB.");
            }

            // Extension Check
            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!_validationOptions.AllowedExtensions.Contains(extension) && _validationOptions.AllowedExtensions.Count > 0)
            {
                throw new ArgumentException($"File type '{extension}' is not allowed.");
            }

            // Save to Storage
            using (var stream = request.File.OpenReadStream())
            {
                var storagePath = await _storageProvider.SaveFileAsync(stream, request.File.FileName, request.File.ContentType);

                // Create Metadata Entity
                var metadata = new FileMetadata(
                    originalName: request.File.FileName,
                    storedName: Path.GetFileName(storagePath),
                    contentType: request.File.ContentType,
                    sizeBytes: request.File.Length,
                    storagePath: storagePath,
                    uploadedBy: request.UploadedBy
                );

                // Ideally, save metadata to DB here using repository
                // await _repository.AddAsync(metadata);

                return metadata;
            }
        }
    }
}
