using LLE.Frontend.Events;
using LLE.Kernel.Contracts;
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

                packageJson.AddScript("esbuild:watch", "node App/Code/Local/ReactFrontend/Source/scripts/esbuild.js --index=App/Code/Local/ReactFrontend/Source/web/index.tsx --watch");
                packageJson.AddScript("esbuild:build", "node App/Code/Local/ReactFrontend/Source/scripts/esbuild.js --index=App/Code/Local/ReactFrontend/Source/web/index.tsx");
            }
        );

        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(
            httpServer =>
            {
                httpServer.MapGet("/app.js", async httpContext =>
                {
                    httpContext.Response.ContentType = "text/javascript";
                    httpContext.Response.StatusCode = 200;
                    await httpContext.Response.WriteAsync(await File.ReadAllTextAsync("App/Code/Local/ReactFrontend/Source/web/dist/index.js"));
                });
                httpServer.MapGet("/app.css", async httpContext =>
                {
                    httpContext.Response.ContentType = "text/javascript";
                    httpContext.Response.StatusCode = 200;
                    await httpContext.Response.WriteAsync(await File.ReadAllTextAsync("App/Code/Local/ReactFrontend/Source/web/dist/index.css"));
                });
            }
        );
        
        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}