using LLE.Kernel.AutoEntity;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Security;

namespace LLE.Languages;

public class LanguageModule : IModuleLoader
{
    public Task AppStart()
    {
        AutoEntityFeature.AutoFeature<Language, ILanguageRepository>();
        
        Eventing.Eventing.Of<DatabaseEvents>().Seeding<ILanguageRepository>().Concurrent(async repository =>
        {
            async Task AddLanguage(string name, string description, string flag)
            {
                await repository.CreateAsync(new Language
                {
                    Name = name,
                    Description = description,
                    FlagIcon = flag
                }, UserContext.Guest, DataOptions.Bypass);
            }

            await AddLanguage("English", "The English language", "english");
            await AddLanguage("Spanish", "The Spanish language", "spanish");
            await AddLanguage("French", "The French language", "french");
            await AddLanguage("German", "The German language", "german");
            await AddLanguage("Italian", "The Italian language", "italian");
            await AddLanguage("Ukrainian", "The Ukrainian language", "ukrainian");
            await AddLanguage("Finnish", "The Finnish language", "finnish");
            await AddLanguage("Portuguese", "The Portuguese language", "portuguese");
            await AddLanguage("Dutch", "The Dutch language", "dutch");
            await AddLanguage("Swedish", "The Swedish language", "swedish");
            await AddLanguage("Norwegian", "The Norwegian language", "norwegian");
            await AddLanguage("Danish", "The Danish language", "danish");
            await AddLanguage("Polish", "The Polish language", "polish");
            await AddLanguage("Czech", "The Czech language", "czech");
            await AddLanguage("Hungarian", "The Hungarian language", "hungarian");

            await AddLanguage("Chinese (Mandarin)", "Mandarin Chinese", "chinese");
            await AddLanguage("Japanese", "The Japanese language", "japanese");
            await AddLanguage("Korean", "The Korean language", "korean");

            return repository;
        });

        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}