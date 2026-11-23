using System.Globalization;
using FluentValidation;

namespace API.Common;

/// <summary>
/// Validation messages are shown to the clinic staff as they are, so they are in Portuguese and name each
/// field the way the admin form labels it ("'Nome' deve ser informado."). A validated property without a
/// label here fails <c>FieldLabelTests</c>.
/// </summary>
public static class FieldLabels
{
    private static readonly Dictionary<string, string> Labels = new()
    {
        ["AuthorAvatarUrl"] = "Avatar",
        ["AuthorBadge"] = "Badge do autor",
        ["AuthorName"] = "Nome do autor",
        ["Benefits"] = "Benefícios",
        ["Bio"] = "Bio",
        ["Consent"] = "Consentimento",
        ["Content"] = "Conteúdo",
        ["Currency"] = "Moeda",
        ["Description"] = "Descrição",
        ["Duration"] = "Duração",
        ["Education"] = "Formação",
        ["Email"] = "E-mail",
        ["EndDate"] = "Data de término",
        ["Experience"] = "Experiência",
        ["FirstName"] = "Nome",
        ["Instructor"] = "Instrutor",
        ["LastName"] = "Sobrenome",
        ["Link"] = "Link",
        ["LongDescription"] = "Descrição longa",
        ["Message"] = "Mensagem",
        ["Modality"] = "Modalidade",
        ["Name"] = "Nome",
        ["Password"] = "Senha",
        ["Phone"] = "Telefone",
        ["Price"] = "Preço",
        ["ProId"] = "Profissional ConsultorioME",
        ["Rating"] = "Nota",
        ["Role"] = "Papel",
        ["Service"] = "Serviço",
        ["Status"] = "Status",
        ["Title"] = "Título",
        ["Type"] = "Tipo",
    };

    public static string? For(string propertyName) => Labels.GetValueOrDefault(propertyName);

    /// <summary>
    /// Makes FluentValidation write its messages in Brazilian Portuguese with these labels. Its language
    /// would otherwise follow the request culture, which is invariant (English) on purpose.
    /// </summary>
    public static void UseForValidationMessages()
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("pt-BR");
        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member is null ? null : For(member.Name);
    }
}
