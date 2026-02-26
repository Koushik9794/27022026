using System;

namespace FileService.Domain.Aggregates
{
    public class FileMetadata
    {
        public Guid Id { get; private set; }
        public string OriginalName { get; private set; }
        public string StoredName { get; private set; } // May differ to avoid collisions
        public string ContentType { get; private set; }
        public long SizeBytes { get; private set; }
        public string StoragePath { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public string UploadedBy { get; private set; } // User ID or "System"

        private FileMetadata() { } // For EF/Serialization

        public FileMetadata(string originalName, string storedName, string contentType, long sizeBytes, string storagePath, string uploadedBy)
        {
            Id = Guid.NewGuid();
            OriginalName = originalName;
            StoredName = storedName;
            ContentType = contentType;
            SizeBytes = sizeBytes;
            StoragePath = storagePath;
            UploadedAt = DateTime.UtcNow;
            UploadedBy = uploadedBy;
        }
    }
}
