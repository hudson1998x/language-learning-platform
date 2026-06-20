using System.Diagnostics;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.TypeScript.Builders;
using LLE.TypeScript.Events;

namespace LLE.TypeScript;

public class TypeScriptModule : IModuleLoader
{
    public async Task AppStart()
    {
        var tsconfig = new TsConfigBuilder();
        var packageJson = new PackageJsonBuilder();

        await Eventing.Eventing.Of<TypeScriptEvents>().TsConfig.DispatchAsync(tsconfig);
        await Eventing.Eventing.Of<NodeEvents>().PackageJson.DispatchAsync(packageJson);
        
        tsconfig.CompilerOptions.Paths.TryAdd("@api/*", ["./App/Api/*"]);
        
        await File.WriteAllTextAsync("tsconfig.json", tsconfig.ToString());
        await File.WriteAllTextAsync("package.json", packageJson.ToString());


        await RunNpmInstallAsync();
    }
    
    private static async Task RunNpmInstallAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "npm.cmd" : "npm",
            Arguments = "i",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = startInfo;

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) Console.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) Console.Error.WriteLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"npm install failed with exit code {process.ExitCode}.");
        }
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}