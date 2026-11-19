using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace API.Common;

/// <summary>
/// Runs the registered FluentValidation validator for every action argument before the action
/// executes, answering 400 ValidationProblemDetails (same shape as model-binding errors).
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var error in result.Errors)
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!context.ModelState.IsValid)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problem = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
            context.Result = new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
            return;
        }

        await next();
    }
}
