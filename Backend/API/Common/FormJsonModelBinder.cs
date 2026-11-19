using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Common;

/// <summary>
/// Binds a multipart/form-data field whose value is JSON (e.g. <c>category = {"id": "..."}</c>)
/// into a typed DTO property, so validation sees the complete DTO before the action runs.
/// Malformed JSON becomes a model-state error on that field (→ 400).
/// </summary>
public sealed class FormJsonModelBinder : IModelBinder
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (string.IsNullOrWhiteSpace(value)) return Task.CompletedTask;

        try
        {
            bindingContext.Result = ModelBindingResult.Success(
                JsonSerializer.Deserialize(value, bindingContext.ModelType, Options));
        }
        catch (JsonException)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"'{bindingContext.ModelName}' must be valid JSON.");
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}
