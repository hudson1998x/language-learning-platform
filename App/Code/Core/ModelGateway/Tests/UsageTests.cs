using LLE.ModelGateway.Contracts;
using LLE.ModelGateway.Enums;
using LLE.ModelGateway.Models;
using LLE.Dependencies.Providers;

namespace LLE.ModelGateway.Tests
{
    /// <summary>
    /// Fake deterministic implementation for testing ModelGateway usage.
    /// </summary>
    [Provider]
    public class FakeModelGateway : IModelGateway
    {
        public Task<ChatSession> ChatAsync(ChatSession request, CancellationToken cancellationToken = default)
        {
            var response = new ChatSession();

            var lastUserMessage = request.Messages
                .LastOrDefault(m => m.Role == ChatMessageRole.User);

            response.AddMessage($"Echo: {lastUserMessage?.Message}", ChatMessageRole.Assistant);

            return Task.FromResult(response);
        }

        public Task<ChatModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChatModelInfo
            {
                Name = "fake-model",
                Provider = "unit-test",
                BackendType = ModelBackendType.LocalServer,
                ContextWindow = 4096,
                MaxOutputTokens = 1024,
                SupportsStreaming = false,
                SupportsToolCalling = false
            });
        }
    }

    public class ModelGatewayUsageTests
    {
        [Fact]
        public void Provider_ReturnsSameInstance_ForSameType()
        {
            var a = Provider.Get<FakeModelGateway>();
            var b = Provider.Get<FakeModelGateway>();

            Assert.Same(a, b);
        }

        [Fact]
        public void ChatSession_AddMessage_PreservesOrder()
        {
            var session = new ChatSession();

            session.AddMessage("first", ChatMessageRole.User);
            session.AddMessage("second", ChatMessageRole.User);
            session.AddMessage("third", ChatMessageRole.User);

            var messages = session.Messages.ToList();

            Assert.Equal(3, messages.Count);
            Assert.Equal("first", messages[0].Message);
            Assert.Equal("second", messages[1].Message);
            Assert.Equal("third", messages[2].Message);
        }

        [Fact]
        public void ChatSession_RemoveMessage_ById_Works()
        {
            var session = new ChatSession();

            session.AddMessage("hello", ChatMessageRole.User);

            var id = session.Messages.First().Id;

            session.RemoveMessage(id);

            Assert.Empty(session.Messages);
        }

        [Fact]
        public async Task FakeModelGateway_EchoesLastUserMessage()
        {
            var model = Provider.Get<FakeModelGateway>();

            var session = new ChatSession();
            session.AddMessage("hello world", ChatMessageRole.User);
            session.AddMessage("ignore me", ChatMessageRole.System);

            var result = await model.ChatAsync(session);

            var assistant = result.Messages.Last();

            Assert.Equal(ChatMessageRole.Assistant, assistant.Role);
            Assert.Equal("Echo: hello world", assistant.Message);
        }

        [Fact]
        public async Task ChatSession_EndToEnd_FlowWorks()
        {
            var model = Provider.Get<FakeModelGateway>();

            var session = new ChatSession();

            session.AddMessage("What is 2+2?", ChatMessageRole.User);

            var response = await model.ChatAsync(session);

            session.AddMessage(response.Messages.Last().Message, ChatMessageRole.Assistant);

            Assert.Equal(2, session.Messages.Count);
            Assert.Contains(session.Messages, m => m.Role == ChatMessageRole.User);
            Assert.Contains(session.Messages, m => m.Role == ChatMessageRole.Assistant);
        }

        [Fact]
        public async Task GetModelInfo_ReturnsValidMetadata()
        {
            var model = Provider.Get<FakeModelGateway>();

            var info = await model.GetModelInfoAsync();

            Assert.Equal("fake-model", info.Name);
            Assert.Equal(ModelBackendType.LocalServer, info.BackendType);
            Assert.True(info.ContextWindow > 0);
        }
    }
}