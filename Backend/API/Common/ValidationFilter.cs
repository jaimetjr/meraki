using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace API.Common;

/// <summary>
/// Runs the registered FluentValidation validator for every action argument before the action
/// executes, answering 400 ValidationProblemDetails (same shape as model-binding errors). Its
/// <c>detail</c> joins the validation messages, which the admin shows to the user as they are.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var messages = new List<string>();
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var error in result.Errors)
            {
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                messages.Add(error.ErrorMessage);
            }
        }

        if (!context.ModelState.IsValid)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problem = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
            // Only our own (Portuguese) messages; model-binding errors are technical English, so without
            // a detail the admin falls back to its generic "Requisição inválida" text.
            if (messages.Count > 0) problem.Detail = string.Join(" ", messages.Distinct());
            context.Result = new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
            return;
        }

        await next();
    }
}
