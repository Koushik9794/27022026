using System;
using System.Collections.Generic;
using System.Text;

namespace GssCommon.Export;

public interface IFileServiceClient
{
    Task<FileUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default);
}

public record FileUploadResult(string FilePath, Guid? FileId);
