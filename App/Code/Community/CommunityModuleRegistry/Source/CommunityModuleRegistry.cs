using LLE.Kernel;
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
    }
}