using LLE.Kernel;
using LLE.ReactFrontend;

namespace LLE.LocalAppRegistry;

public static class CommunityModuleRegistry
{
    public static void LoadModules()
    {
        ApplicationLoader.AddModule(new ReactFrontendModule());
    }
}