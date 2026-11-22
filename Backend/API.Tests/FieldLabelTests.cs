using API.Common;
using FluentValidation;

namespace API.Tests;

/// <summary>
/// Validation messages name the field the way the admin form does ("'Nome' deve ser informado."), so every
/// validated property needs a Portuguese label. A new field without one would show its C# name instead.
/// </summary>
public class FieldLabelTests
{
    public static TheoryData<string> ValidatedFields()
    {
        var fields = new TheoryData<string>();
        var validators = typeof(Program).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true } && typeof(IValidator).IsAssignableFrom(t));
        foreach (var name in validators
                     .Select(t => (IValidator)Activator.CreateInstance(t)!)
                     .SelectMany(v => v.CreateDescriptor().GetMembersWithValidators().Select(m => m.Key))
                     .Distinct())
            fields.Add(name);
        return fields;
    }

    [Theory]
    [MemberData(nameof(ValidatedFields))]
    public void Every_validated_field_has_a_portuguese_label(string field) =>
        Assert.False(string.IsNullOrWhiteSpace(FieldLabels.For(field)), $"Add a label for '{field}' to FieldLabels.");
}
