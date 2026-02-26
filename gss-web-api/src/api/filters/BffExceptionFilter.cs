using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace GssWebApi.Api.Filters
{
    public class BffExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<BffExceptionFilter> _logger;

        public BffExceptionFilter(ILogger<BffExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpRequestException httpEx)
            {
                _logger.LogWarning(context.Exception, "Service call failed with status: {StatusCode}", httpEx.StatusCode);

                if (httpEx.StatusCode == HttpStatusCode.BadRequest)
                {
                    context.Result = new BadRequestObjectResult(new
                    {
                        error = "Service Validation Error",
                        details = httpEx.Message
                    });
                    context.ExceptionHandled = true;
                }
                else if (httpEx.StatusCode == HttpStatusCode.NotFound)
                {
                    context.Result = new NotFoundObjectResult(new
                    {
                        error = "Resource Not Found in Service",
                        details = httpEx.Message
                    });
                    context.ExceptionHandled = true;
                }
            }
            else
            {
                _logger.LogError(context.Exception, "An unhandled exception occurred in the BFF.");
                // Let other exceptions bubble up as 500s but logged
            }
        }
    }
}
