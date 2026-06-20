using LLE.Frontend;
using LLE.Kernel;

namespace LLE.Application;

public static class ModuleRegistry
{
    public static void AddModules()
    {
        // ============================
        // Add all modules from core
        // here
        // ============================
        ApplicationLoader.AddModule(new FrontendModule());
    }
}