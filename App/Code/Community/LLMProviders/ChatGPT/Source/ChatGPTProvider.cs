using System.Net.Http.Headers;
using System.Net.Http.Json;
using LLE.AppAdmin.Events;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Contracts;
using LLE.LLMFramework.Models;
using LLE.LLMProviders.ChatGPT.Models;

namespace LLE.LLMProviders.ChatGPT;

public class ChatGPTProvider : ILLMProvider
{
    private HttpClient _httpClient;
    private readonly ChatGPTConfiguration _config;
    private bool _isAvailable;

    public ChatGPTProvider()
    {
        _config = ConfigurationCatalog.GetConfiguration<ChatGPTConfiguration>();
        _httpClient = CreateClient();

        _ = CheckAvailabilityAsync();

        Eventing.Eventing.Of<ConfigurationEvents>().Changed.Concurrent(payload =>
        {
            if (payload is ChatGPTConfiguration)
            {
                _httpClient.Dispose();
                _httpClient = CreateClient();
                _ = CheckAvailabilityAsync();
            }
        });
    }

    public bool IsEnabled => _config.Enabled && _isAvailable;
    public string? LogoUrl => "/media/chatgpt/logo.png";
    public string? Description => "Powerful cloud AI with excellent language understanding and natural conversations.";

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

        var messages = request.History.Select(h => new ChatGPTChatMessage
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

        messages.Add(new ChatGPTChatMessage
        {
            Role = "user",
            Content = request.Prompt
        });

        var chatRequest = new ChatGPTChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = false,
            MaxTokens = request.MaxTokens
        };

        var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", chatRequest);
        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<ChatGPTChatResponse>();

        return new LLMResponse
        {
            Content = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty,
            ProviderName = "ChatGPT",
            Model = model,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(chatResponse?.Created ?? 0).UtcDateTime
        };
    }
}
