using FluentValidation;
using Wolverine;

namespace AdminService.Application.Validators
{
    /// <summary>
    /// Wolverine middleware for automatic validation using FluentValidation
    /// </summary>
    public static class ValidationMiddleware
    {
        public static async Task<T> ValidateAsync<T>(
            T command,
            IEnumerable<IValidator<T>> validators,
            Func<Task<T>> next)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<T>(command);

            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);

            return await next();
        }
    }
}
