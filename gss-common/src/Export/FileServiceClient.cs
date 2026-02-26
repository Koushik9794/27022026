using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GssCommon.Export;

public sealed class FileServiceClient : IFileServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<FileServiceClient> _logger;

    public FileServiceClient(HttpClient http, ILogger<FileServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Uploading file {FileName} to {BaseAddress}", fileName, _http.BaseAddress);

            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(content);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // "file" matches the File Service controller parameter name
            form.Add(fileContent, "file", fileName);

            using var response = await _http.PostAsync("api/files/upload", form, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            _logger.LogInformation("Upload response: {Json}", json);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to File Service. Inner: {InnerMessage}", ex.InnerException?.Message);
            throw;
        }
    }
}

