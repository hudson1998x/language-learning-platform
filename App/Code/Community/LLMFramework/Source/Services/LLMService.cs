using LLE.Kernel.Attributes;
using LLE.LLMFramework.Contracts;
using LLE.LLMFramework.Models;

namespace LLE.LLMFramework.Services;

[Service]
public class LLMService
{
    private static readonly Dictionary<string, ILLMProvider> _providers = new();

    public static void Register<T>(string name) where T : ILLMProvider, new()
    {
        _providers[name] = new T();
    }

    public static IReadOnlyDictionary<string, ILLMProvider> GetRegisteredProviders()
    {
        return _providers;
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
}
