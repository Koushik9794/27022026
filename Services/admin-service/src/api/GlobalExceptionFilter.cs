using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace AdminService.Api
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred.");

            if (context.Exception is ValidationException validationException)
            {
                var errors = validationException.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                context.Result = new BadRequestObjectResult(new
                {
                    Message = "Validation failed.",
                    Errors = errors
                });
                context.ExceptionHandled = true;
            }
            else if (context.Exception is AdminService.Domain.Exceptions.DomainException domainException)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Message = domainException.Message
                });
                context.ExceptionHandled = true;
            }
            else if (context.Exception is ArgumentException || context.Exception is InvalidOperationException)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Message = context.Exception.Message
                });
                context.ExceptionHandled = true;
            }
        }
    }
}
