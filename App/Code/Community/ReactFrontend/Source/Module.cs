using LLE.Frontend.Events;
using LLE.Kernel.Contracts;
using LLE.ReactFrontend.Generators;
using LLE.Sockets.Events;
using LLE.TypeScript.Builders;
using LLE.TypeScript.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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

        Eventing.Eventing.Of<HtmlBuilderEvents>().Created.Concurrent(builder =>
        {
            builder.WithScript("/app.js");
            builder.WithCss("/app.css");
        });

        // make sure the tsconfig is properly configured to use React.
        Eventing.Eventing.Of<TypeScriptEvents>().TsConfig.Concurrent(
            tsconfig =>
            {
                tsconfig.CompilerOptions.AllowJs = true;
                tsconfig.CompilerOptions.Jsx = JsxMode.ReactJsx;
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@component/*", [
                        "./App/Design/React/Components/Community/*",
                        "./App/Design/React/Components/Community/*",
                        "./App/Design/React/Components/Core/*",
                    ]
                );
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@hook/*", [
                        "./Design/React/Hooks/Community/*",
                        "./Design/React/Hooks/Community/*",
                        "./Design/React/Hooks/Core/*",
                    ]
                );
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@theme:admin/*", [
                        "./App/Design/React/Themes/Admin/Community/*",
                        "./App/Design/React/Themes/Admin/Community/*",
                        "./App/Design/React/Themes/Admin/Core/*",
                    ]
                );
                tsconfig.CompilerOptions.Paths.TryAdd(
                    "@theme:frontend/*", [
                        "./App/Design/React/Themes/Frontend/Community/*",
                        "./App/Design/React/Themes/Frontend/Community/*",
                        "./App/Design/React/Themes/Frontend/Core/*",
                    ]
                );
                tsconfig.Include.Add(
                    "./App/Code/Community/ReactFrontend/Source/web/index.tsx"
                );
                tsconfig.Exclude.Add(
                    "./App/Code/Community/ReactFrontend/Source/web/dist/*"
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

                packageJson.AddScript("esbuild:watch", "node App/Code/Community/ReactFrontend/Source/scripts/esbuild.js --index=App/Code/Community/ReactFrontend/Source/web/index.tsx --watch");
                packageJson.AddScript("esbuild:build", "node App/Code/Community/ReactFrontend/Source/scripts/esbuild.js --index=App/Code/Community/ReactFrontend/Source/web/index.tsx");
            }
        );

        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(
            httpServer =>
            {
                httpServer.MapGet("/app.js", async httpContext =>
                {
                    httpContext.Response.ContentType = "text/javascript";
                    httpContext.Response.StatusCode = 200;
                    await httpContext.Response.WriteAsync(await File.ReadAllTextAsync("App/Code/Community/ReactFrontend/Source/web/dist/index.js"));
                });
                httpServer.MapGet("/app.css", async httpContext =>
                {
                    httpContext.Response.ContentType = "text/css";
                    httpContext.Response.StatusCode = 200;
                    await httpContext.Response.WriteAsync(await File.ReadAllTextAsync("App/Code/Community/ReactFrontend/Source/web/dist/index.css"));
                });
            }
        );
        
        // load all components into a registry. 
        var generator = new ComponentRegistryGenerator();
        generator.GenerateComponentRegistry();
        
        return Task.CompletedTask;
    }


    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}