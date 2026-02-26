using System.Collections.Generic;

namespace FileService.Infrastructure.Configuration
{
    public class FileValidationOptions
    {
        public const string SectionName = "FileValidation";

        public long MaxFileSizeMb { get; set; } = 50;
        public List<string> AllowedExtensions { get; set; } = new List<string>();
        public bool MagicNumberCheck { get; set; } = false;
        public string LocalStoragePath { get; set; } = "uploads"; // For local provider
    }
}
