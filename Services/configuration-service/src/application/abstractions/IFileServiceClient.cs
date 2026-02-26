namespace ConfigurationService.Application.Abstractions;


public interface IFileServiceClient
{
    Task<FileUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default);
}

public sealed record FileUploadResult(
    string FilePath,   // or RelativePath
    Guid? FileId = null);

