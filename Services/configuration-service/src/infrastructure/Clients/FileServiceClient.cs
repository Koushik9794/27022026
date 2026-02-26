
using System.Net.Http.Headers;
using System.Text.Json;

using ConfigurationService.Application.Abstractions;

namespace ConfigurationService.Infrastructure.Clients;


public sealed class FileServiceClient : IFileServiceClient
{
    private readonly HttpClient _http;

    public FileServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();

        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // "file" should match your File Service controller parameter name
        form.Add(fileContent, "file", fileName);

        using var response = await _http.PostAsync("api/files/upload", form, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);

        // Adjust to your actual response contract:
        // e.g. { "filePath": "configuration/123/a.png", "fileId": "..." }
        var doc = JsonDocument.Parse(json);
        var filePath = doc.RootElement.GetProperty("storagePath").GetString()!;

        Guid? fileId = null;
        if (doc.RootElement.TryGetProperty("id", out var idProp) &&
            Guid.TryParse(idProp.GetString(), out var parsed))
        {
            fileId = parsed;
        }

        return new FileUploadResult(filePath, fileId);
    }
}

