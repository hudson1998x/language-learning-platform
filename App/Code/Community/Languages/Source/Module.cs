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

            await AddLanguage("English",
                "Spoken by over 1.5 billion people worldwide and the most widely used international language.",
                "english");

            await AddLanguage("Spanish",
                "Spoken by more than 500 million native speakers across Europe and the Americas.",
                "spanish");

            await AddLanguage("French",
                "An official language in nearly 30 countries and spoken on every inhabited continent.",
                "french");

            await AddLanguage("German",
                "The most widely spoken native language in Europe.",
                "german");

            await AddLanguage("Italian",
                "A Romance language known for its influence on music, art, and culture.",
                "italian");

            await AddLanguage("Ukrainian",
                "An East Slavic language spoken by tens of millions of people, primarily in Ukraine.",
                "ukrainian");

            await AddLanguage("Finnish",
                "Known for its unique grammar and extensive use of vowel harmony.",
                "finnish");

            await AddLanguage("Portuguese",
                "Spoken by over 250 million people across four continents.",
                "portuguese");

            await AddLanguage("Dutch",
                "The native language of the Netherlands and one of Belgium's official languages.",
                "dutch");

            await AddLanguage("Swedish",
                "A North Germanic language spoken by more than 10 million people.",
                "swedish");

            await AddLanguage("Norwegian",
                "Known for its two official written forms: Bokmål and Nynorsk.",
                "norwegian");

            await AddLanguage("Danish",
                "The official language of Denmark and closely related to Norwegian and Swedish.",
                "danish");

            await AddLanguage("Polish",
                "One of the largest Slavic languages, spoken by over 40 million people.",
                "polish");

            await AddLanguage("Czech",
                "A West Slavic language with a literary history spanning more than 700 years.",
                "czech");

            await AddLanguage("Hungarian",
                "Part of the Uralic language family and unrelated to most European languages.",
                "hungarian");

            await AddLanguage("Chinese (Mandarin)",
                "The most spoken native language in the world, with over 900 million native speakers.",
                "chinese");

            await AddLanguage("Japanese",
                "Uses a unique writing system combining Kanji, Hiragana, and Katakana.",
                "japanese");

            await AddLanguage("Korean",
                "Written using Hangul, one of the world's most scientifically designed alphabets.",
                "korean");

            return repository;
        });

        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}