using LLE.FlashCards;
using LLE.Kernel;
using LLE.Scenarios;

namespace LLE.LocalAppRegistry;

public static class LocalAppRegistry
{
    public static void LoadModules()
    {
        ApplicationLoader.AddModule(new FlashCardsModule());
        ApplicationLoader.AddModule(new ScenarioModule());
    }
}