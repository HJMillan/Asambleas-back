using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Asambleas.Infrastructure.Filters;

/// <summary>
/// H24: Filtro manual de validación que reemplaza la auto-validación deprecada
/// de FluentValidation.AspNetCore. Ejecuta validators registrados en DI para
/// cada argumento de la acción.
/// </summary>
public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values.Where(v => v != null))
        {
            var argumentType = argument!.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            if (serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationResult = await validator.ValidateAsync(
                new ValidationContext<object>(argument));

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                context.Result = new UnprocessableEntityObjectResult(
                    new ValidationProblemDetails(errors)
                    {
                        Type = "https://httpstatuses.com/422",
                        Title = "Error de validación",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Instance = context.HttpContext.Request.Path
                    });
                return;
            }
        }

        await next();
    }
}
