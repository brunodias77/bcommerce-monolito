using Bcommerce.Modules.Catalog.Domain.Services;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bcommerce.Modules.Catalog.Infrastructure.Services;

public class SlugGenerator : ISlugGenerator
{
    public string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var slug = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        
        // Remove characters that aren't a-z, 0-9 or hyphen
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        
        // Convert whitespace to hyphens
        slug = Regex.Replace(slug, @"\s+", "-");
        
        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }
}
