using LLE.Kernel;
using LLE.ReactFrontend;
using LLE.SQLiteAdapter;

namespace LLE.LocalAppRegistry;

public static class CommunityModuleRegistry
{
    public static void LoadModules()
    {
        ApplicationLoader.AddModule(new ReactFrontendModule());
        ApplicationLoader.AddModule(new SQLiteModule());
    }
}