using LLE.Kernel.Attributes;
using LLE.Kernel.Security;
using Microsoft.AspNetCore.Http;

namespace LLE.Languages;

[Service]
public class LanguageService(ILanguageRepository languageRepository)
{
    public async Task<Language?> GetCurrentLanguage(HttpContext context)
    {
        var languageIdStr = context.Session.GetString("Language");

        if (string.IsNullOrEmpty(languageIdStr))
        {
            return null;
        }

        var guid = Guid.Parse(languageIdStr);

        if (guid == Guid.Empty)
        {
            return null;
        }

        return await languageRepository.FindByIdAsync(guid, UserContext.Guest, DataOptions.Bypass);
    }
}