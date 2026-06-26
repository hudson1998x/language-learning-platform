using System.Collections.Concurrent;
using System.Text.Json;
using LLE.Eventing;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Registry;

namespace LLE.Kernel;

public static partial class ApplicationLoader
{
    private static readonly ConcurrentBag<IModuleLoader> ModuleLoaders = [];
    private static readonly ConcurrentDictionary<string, ApplicationState> InstallState = [];

    private const string StatePath = "var/state/modules.json";

    static ApplicationLoader()
    {
        LoadState();
    }

    public static void AddModule(IModuleLoader moduleLoader)
    {
        ModuleLoaders.Add(moduleLoader);
    }

    public static async Task StartLifecycle()
    {
        foreach (var module in ModuleLoaders)
        {
            var key = GetModuleKey(module);

            if (!InstallState.TryGetValue(key, out var state))
            {
                state = new ApplicationState();
                InstallState[key] = state;
            }

            if (!state.IsInstalled)
            {
                await module.Install();
                state.IsInstalled = true;
                state.IsEnabled = true;
                PersistState();
            }

            if (!state.IsEnabled)
            {
                continue;
            }
        }

        var enabledModules = ModuleLoaders
            .Where(IsEnabled).ToArray();
        
        ScanModuleAssemblies(enabledModules);
        
        await Task.WhenAll(
            enabledModules.Select(m => m.AppStart()));

        await Eventing.Eventing.Of<ApplicationEvents>().AllStarted.DispatchAsync(new object());

        // at this point, all modules are loaded.
    }

    public static async Task StopLifecycle()
    {
        var enabledModules = ModuleLoaders
            .Where(IsEnabled);

        await Task.WhenAll(
            enabledModules.Select(m => m.AppStop()));
    }

    private static bool AppInstalled(IModuleLoader loader)
    {
        var key = GetModuleKey(loader);

        return InstallState.TryGetValue(key, out var state)
               && state.IsInstalled;
    }

    private static bool IsEnabled(IModuleLoader loader)
    {
        var key = GetModuleKey(loader);

        return InstallState.TryGetValue(key, out var state)
               && state is { IsInstalled: true, IsEnabled: true };
    }

    private static string GetModuleKey(IModuleLoader loader)
    {
        return loader.GetType().FullName ?? loader.GetType().Name;
    }

    private static void LoadState()
    {
        try
        {
            if (!File.Exists(StatePath))
                return;

            var json = File.ReadAllText(StatePath);

            var state = JsonSerializer.Deserialize<Dictionary<string, ApplicationState>>(json);

            if (state is null)
                return;

            foreach (var kvp in state)
            {
                InstallState[kvp.Key] = kvp.Value;
            }
        }
        catch
        {
            // swallow intentionally for bootstrap resilience
        }
    }

    private static void PersistState()
    {
        try
        {
            var dir = Path.GetDirectoryName(StatePath);

            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(
                InstallState,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(StatePath, json);
        }
        catch
        {
            // intentionally ignore persistence failures during runtime
        }
    }
}

internal class ApplicationState
{
    public bool IsEnabled { get; set; } = false;

    public bool IsInstalled { get; set; }
}