using LLE.Kernel.Attributes;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Configurations;
using LLE.LLMFramework.Contracts;
using LLE.LLMFramework.Models;

namespace LLE.LLMFramework.Services;

[Service]
public class LLMService
{
    private readonly Dictionary<string, ILLMProvider> _providers = new();

    public void Register<T>(string name) where T : ILLMProvider, new()
    {
        _providers[name] = new T();
    }

    public ILLMProvider? GetDefaultProvider()
    {
        var config = ConfigurationCatalog.GetConfiguration<LLMConfiguration>();

        if (!_providers.TryGetValue(config.PreferredProvider, out var provider))
            return null;

        return provider.IsEnabled ? provider : null;
    }

    public IReadOnlyDictionary<string, ILLMProvider> GetRegisteredProviders()
    {
        return _providers;
    }

    public bool IsLlmAvailable()
    {
        return _providers.Values.Any(p => p.IsEnabled);
    }

    public Dictionary<string, bool> GetProviderStatus()
    {
        return _providers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsEnabled);
    }

    public Dictionary<string, string?> GetProviderLogos()
    {
        return _providers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.LogoUrl);
    }

    public async Task<LLMResponse> SendMessageAsync(
        string providerName,
        string prompt,
        Action<LLMRequest>? configure = null)
    {
        if (!_providers.TryGetValue(providerName, out var provider))
            throw new InvalidOperationException($"Provider '{providerName}' is not registered.");

        var composer = new PromptComposer();
        var request = new LLMRequest { Prompt = prompt };
        configure?.Invoke(request);
        request.Prompt = composer.Compose(request);

        return await provider.GenerateAsync(request);
    }

    public async Task<LLMResponse> SendMessageAsync(string prompt, Action<LLMRequest>? configure = null)
    {
        return await SendMessageAsync(ConfigurationCatalog.GetConfiguration<LLMConfiguration>().PreferredProvider, prompt, configure);
    }
}
