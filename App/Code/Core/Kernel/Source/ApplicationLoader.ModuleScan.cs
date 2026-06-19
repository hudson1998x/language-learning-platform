using LLE.Kernel.Attributes;
using LLE.Kernel.Registry;
using Source.Contracts;

namespace LLE.Kernel;

public static partial class ApplicationLoader
{
    private static void ScanModuleAssemblies(IEnumerable<IModuleLoader> enabledModules)
    {
        foreach (var enabledModule in enabledModules)
        {
            ScanModuleAssembly(enabledModule);
        }
    }

    private static void ScanModuleAssembly(IModuleLoader enabledModule)
    {
        foreach (var type in enabledModule.GetType().Assembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes(false);

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case ControllerAttribute:
                        ControllerCatalog.GetController(type);
                        break;
                    case RepositoryAttribute:
                        RepositoryCatalog.GetRepository(type);
                        break;
                    case ManagerAttribute:
                        // load it into the manager catalog.
                        ManagerCatalog.GetManager(type);
                        break;
                    case ServiceAttribute:
                        ServiceCatalog.GetService(type);
                        break;
                }
            }
        }
    }
}