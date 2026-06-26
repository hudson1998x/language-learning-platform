using LLE.HomeChat;
using LLE.Kernel;
using LLE.Languages;
using LLE.LeMessage;
using LLE.LLMFramework;
using LLE.LLMProviders.Ollama;
using LLE.MusicTranslation;
using LLE.Pages;
using LLE.ReactFrontend;
using LLE.SQLiteAdapter;

namespace LLE.CommunityModuleRegistry;

public static class CommunityModuleRegistry
{
    public static void LoadModules()
    {
        ApplicationLoader.AddModule(new ReactFrontendModule());
        ApplicationLoader.AddModule(new SQLiteModule());
        ApplicationLoader.AddModule(new PagesModule());
        ApplicationLoader.AddModule(new LanguageModule());
        ApplicationLoader.AddModule(new LLMModule());
        ApplicationLoader.AddModule(new OllamaModule());
        ApplicationLoader.AddModule(new MusicTranslationModule());
        ApplicationLoader.AddModule(new LeMessageModule());
        ApplicationLoader.AddModule(new HomeChatModule());
    }
}