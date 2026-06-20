using LLE.Frontend.Events;
using LLE.Kernel.Contracts;
using LLE.TypeScript.Builders;
using LLE.TypeScript.Events;

namespace LLE.ReactFrontend;

public class ReactFrontendModule : IModuleLoader
{
    public Task AppStart()
    {
        // inject the mount container.
        Eventing.Eventing.Of<HtmlBuilderEvents>().Body.Concurrent(writer =>
        {
            writer.Append("<div id=\"app\"></div>");
        });

        // make sure the tsconfig is properly configured to use React.
        Eventing.Eventing.Of<TypeScriptEvents>().TsConfig.Concurrent(
            tsconfig =>
            {
                tsconfig.CompilerOptions.AllowJs = true;
                tsconfig.CompilerOptions.Jsx = JsxMode.ReactJsx;
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@component/*", [
                        "./Design/React/Components/Local/*",
                        "./Design/React/Components/Community/*",
                        "./Design/React/Components/Core/*",
                    ]
                );
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@hook/*", [
                        "./Design/React/Hooks/Local/*",
                        "./Design/React/Hooks/Community/*",
                        "./Design/React/Hooks/Core/*",
                    ]
                );
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@theme:admin/*", [
                        "./Design/React/Themes/Admin/Local/*",
                        "./Design/React/Themes/Admin/Community/*",
                        "./Design/React/Themes/Admin/Core/*",
                    ]
                );
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@theme:frontend/*", [
                        "./Design/React/Themes/Frontend/Local/*",
                        "./Design/React/Themes/Frontend/Community/*",
                        "./Design/React/Themes/Frontend/Core/*",
                    ]
                );
            }
        );

        Eventing.Eventing.Of<NodeEvents>().PackageJson.Concurrent(
            packageJson =>
            {
                // basic react packages.
                packageJson.AddDependency("react", Dependencies.App);
                packageJson.AddDependency("react-dom", Dependencies.App);
                
                // types etc...
                packageJson.AddDependency("@types/react", Dependencies.Dev);
                packageJson.AddDependency("@types/react-dom", Dependencies.Dev);
                
                // tooling.
                packageJson.AddDependency("esbuild", Dependencies.Dev);
                packageJson.AddDependency("esbuild-sass-plugin", Dependencies.Dev);
            }
        );
        
        
        
        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}