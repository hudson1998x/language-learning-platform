using System.Net.Http.Headers;
using System.Net.Http.Json;
using LLE.AppAdmin.Events;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Contracts;
using LLE.LLMFramework.Models;
using LLE.LLMProviders.MistralChat.Models;

namespace LLE.LLMProviders.MistralChat;

public class MistralProvider : ILLMProvider
{
    private HttpClient _httpClient;
    private readonly MistralConfiguration _config;
    private bool _isAvailable;

    public MistralProvider()
    {
        _config = ConfigurationCatalog.GetConfiguration<MistralConfiguration>();
        _httpClient = CreateClient();

        _ = CheckAvailabilityAsync();

        Eventing.Eventing.Of<ConfigurationEvents>().Changed.Concurrent(payload =>
        {
            if (payload is MistralConfiguration)
            {
                _httpClient.Dispose();
                _httpClient = CreateClient();
                _ = CheckAvailabilityAsync();
            }
        });
    }

    public bool IsEnabled => _config.Enabled && _isAvailable;
    public string? LogoUrl => "/media/mistral/logo.png";
    public string? Description => "Fast, affordable AI with excellent multilingual performance and flexible models.";

    private HttpClient CreateClient()
    {
        var client = new HttpClient { BaseAddress = new Uri(_config.BaseUrl) };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        return client;
    }

    private async Task CheckAvailabilityAsync()
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync("/v1/models", timeout.Token);
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

        var messages = request.History.Select(h => new MistralChatMessage
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

        messages.Add(new MistralChatMessage
        {
            Role = "user",
            Content = request.Prompt
        });

        var mistralRequest = new MistralChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = false,
            MaxTokens = request.MaxTokens
        };

        var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", mistralRequest);
        response.EnsureSuccessStatusCode();

        var mistralResponse = await response.Content.ReadFromJsonAsync<MistralChatResponse>();

        return new LLMResponse
        {
            Content = mistralResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty,
            ProviderName = "MistralChat",
            Model = model,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(mistralResponse?.Created ?? 0).UtcDateTime
        };
    }
}
