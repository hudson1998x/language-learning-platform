using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.Registry;

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
                    case RepositoryAttribute:
                        RepositoryCatalog.GetRepository(type);
                        break;
                    case ServiceAttribute:
                        ServiceCatalog.GetService(type);
                        break;
                    case ConfigurationAttribute:
                        ConfigurationCatalog.GetConfiguration(type);
                        break;
                }
            }
        }
    }
}