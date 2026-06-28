using System.Net.Http.Json;
using LLE.AppAdmin.Events;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Contracts;
using LLE.LLMFramework.Models;
using LLE.LLMProviders.Ollama.Models;

namespace LLE.LLMProviders.Ollama;

public class OllamaProvider : ILLMProvider
{
    private HttpClient _httpClient;
    private readonly OllamaConfiguration _config;
    private bool _isAvailable;

    public OllamaProvider()
    {
        _config = ConfigurationCatalog.GetConfiguration<OllamaConfiguration>();
        _httpClient = new HttpClient { BaseAddress = new Uri(_config.BaseUrl) };

        _ = CheckAvailabilityAsync();

        Eventing.Eventing.Of<ConfigurationEvents>().Changed.Concurrent(payload =>
        {
            if (payload is OllamaConfiguration)
            {
                _httpClient.Dispose();
                _httpClient = new HttpClient { BaseAddress = new Uri(_config.BaseUrl) };
                _ = CheckAvailabilityAsync();
            }
        });
    }

    public bool IsEnabled => _config.Enabled && _isAvailable;
    public string? LogoUrl => "/media/ollama/logo.png";

    private async Task CheckAvailabilityAsync()
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var response = await _httpClient.GetAsync("/api/tags", timeout.Token);
            _isAvailable = response.IsSuccessStatusCode;
        }
        catch
        {
            _isAvailable = false;
        }
    }

    public async Task<LLMResponse> GenerateAsync(LLMRequest request)
    {
        var model = request.Model ?? _config.ModelName;

        var messages = request.History.Select(h => new OllamaChatMessage
        {
            Role = h.Role switch
            {
                MessageRole.System => "system",
                MessageRole.User => "user",
                MessageRole.Assistant => "assistant",
                _ => "user"
            },
            Content = h.Content
        }).ToList();

        messages.Add(new OllamaChatMessage
        {
            Role = "user",
            Content = request.Prompt
        });

        var ollamaRequest = new OllamaChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = false,
            Options = request.MaxTokens.HasValue
                ? new OllamaOptions { NumPredict = request.MaxTokens }
                : null
        };

        var response = await _httpClient.PostAsJsonAsync("/api/chat", ollamaRequest);
        response.EnsureSuccessStatusCode();

        var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

        return new LLMResponse
        {
            Content = ollamaResponse?.Message?.Content ?? string.Empty,
            ProviderName = "Ollama",
            Model = model,
            CreatedAt = ollamaResponse?.CreatedAt ?? DateTime.UtcNow
        };
    }
}
