using LLE.Kernel;
using LLE.ReactFrontend;

namespace LLE.LocalAppRegistry;

public static class LocalAppRegistry
{
    public static void LoadModules()
    {
        ApplicationLoader.AddModule(new ReactFrontendModule());
    }
}