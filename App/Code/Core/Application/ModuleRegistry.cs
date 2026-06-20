using LLE.Frontend;
using LLE.Kernel;
using LLE.LocalAppRegistry;
using LLE.TypeScript;

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
        ApplicationLoader.AddModule(new TypeScriptModule());
        
        // ============================
        // Community Modules
        // ============================
        CommunityModuleRegistry.LoadModules();
        
        // ============================
        // Add Local Modules here
        // ============================
        LocalAppRegistry.LocalAppRegistry.LoadModules();
    }
}