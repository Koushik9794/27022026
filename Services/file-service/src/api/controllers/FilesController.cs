using System.IO;
using System.Threading.Tasks;
using FileService.Application.Commands;
using FileService.Application.Interfaces;
using FileService.Application.Queries;
using FileService.Domain.Aggregates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace FileService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IMessageBus _mediator;
        private readonly IStorageProvider _storageProvider;

        public FilesController(IMessageBus mediator, IStorageProvider storageProvider)
        {
            _mediator = mediator;
            _storageProvider = storageProvider;
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("No file provided.");
            }

            try
            {
                var command = new UploadFileCommand { File = file };
                var result = await _mediator.InvokeAsync<FileMetadata>(command);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download([FromQuery] string filePath, [FromQuery] string? contentType = null)
        {
            try
            {
                // We use storageProvider directly here because returning a Stream 
                // through Wolverine/MediatR often results in ObjectDisposedException
                var stream = await _storageProvider.GetFileStreamAsync(filePath);
                
                return File(stream, contentType ?? "application/octet-stream", Path.GetFileName(filePath));
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found.");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
