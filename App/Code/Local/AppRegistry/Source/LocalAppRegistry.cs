using LLE.FlashCards;
using LLE.Kernel;

namespace LLE.LocalAppRegistry;

public static class LocalAppRegistry
{
    public static void LoadModules()
    {
        ApplicationLoader.AddModule(new FlashCardsModule());
    }
}