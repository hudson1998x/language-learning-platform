using LLE.Kernel.Attributes;
using LLE.LLMFramework.Contracts;
using LLE.LLMFramework.Models;

namespace LLE.LLMFramework.Services;

[Service]
public class LLMService
{
    private readonly ILLMProvider _provider;
    private readonly PromptComposer _composer;

    public LLMService(ILLMProvider provider, PromptComposer composer)
    {
        _provider = provider;
        _composer = composer;
    }

    public async Task<LLMResponse> AskAsync(string prompt, Action<LLMRequest>? configure = null)
    {
        var request = new LLMRequest
        {
            Prompt = prompt
        };

        configure?.Invoke(request);

        request.Prompt = _composer.Compose(request);

        return await _provider.GenerateAsync(request);
    }
}
