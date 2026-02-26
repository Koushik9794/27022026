namespace ConfigurationService.Domain.Aggregates;


public sealed record FileUpload(
    Stream Content,
    string FileName,
    string ContentType,
    long Length
);

