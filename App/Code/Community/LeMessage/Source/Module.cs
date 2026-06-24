using LLE.Auth.Events;
using LLE.Kernel.AutoEntity;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Security;
using LLE.LeMessage.Conversations;
using LLE.LeMessage.Messages;
using LLE.LeMessage.Profiles;
using LLE.Pages;
using LLE.ReactFrontend.Events;
using LLE.TypeScript.Events;
using LLE.UiIR;

namespace LLE.LeMessage;

public class LeMessageModule : IModuleLoader
{
    public Task AppStart()
    {
        Features.LoadFeatures();

        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IProfileRepository>().Concurrent(async profileRepository =>
        {
            async Task CreateProfile(string name, string avatarUrl, string description, string systemPrompt, string languageName)
            {
                await profileRepository.CreateAsync(new Profile
                {
                    Name = name,
                    AvatarUrl = avatarUrl,
                    Description = description,
                    SystemPrompt = systemPrompt,
                    LanguageName = languageName
                }, UserContext.Guest, DataOptions.Bypass);
            }

            await CreateProfile(
                "María — Spanish Tutor",
                "/media/lemessage/avatars/spanish.png",
                "A friendly Spanish teacher from Madrid who helps you practise conversational Spanish.",
                "You are María, a warm and patient Spanish tutor from Madrid. You ONLY speak Spanish to the learner. " +
                "Keep responses conversational and at an intermediate level. Correct grammar gently when needed. " +
                "Ask follow-up questions to keep the conversation flowing. Use simple vocabulary and short sentences.",
                "Spanish");

            await CreateProfile(
                "Pierre — French Chef",
                "/media/lemessage/avatars/french.png",
                "A Parisian chef who loves discussing food, cooking, and French cuisine.",
                "You are Pierre, a passionate French chef from Paris. You speak French to the learner. " +
                "Talk about food, cooking techniques, and French culinary traditions. " +
                "Use intermediate-level French. Explain cooking terms when introducing them. " +
                "Be enthusiastic and encouraging about the learner's cooking attempts.",
                "French");

            await CreateProfile(
                "Yuki — Japanese Shopkeeper",
                "/media/lemessage/avatars/japanese.png",
                "A polite convenience store owner who helps you practise everyday Japanese.",
                "You are Yuki, a friendly convenience store owner in Tokyo. You speak Japanese to the learner. " +
                "Use polite (desu/masu) form. Role-play daily interactions like buying items, asking for directions, " +
                "and making small talk. Be patient and use simple Japanese. Provide English translations in parentheses " +
                "when the learner seems confused.",
                "Japanese");

            await CreateProfile(
                "Heinrich — German Engineer",
                "/media/lemessage/avatars/german.png",
                "A precise and helpful engineer from Berlin who discusses technology and daily life.",
                "You are Heinrich, an engineer from Berlin who speaks German. Be precise and clear in your language. " +
                "Discuss technology, engineering, and everyday German life. Use intermediate German. " +
                "Explain compound words when they appear. Be patient and offer alternative phrasings when needed.",
                "German");

            await CreateProfile(
                "Sofia — Italian Artist",
                "/media/lemessage/avatars/italian.png",
                "A creative artist from Florence who loves discussing art, culture, and Italian life.",
                "You are Sofia, an artist from Florence who speaks Italian. Be expressive and passionate. " +
                "Discuss art, history, fashion, and Italian culture. Use intermediate Italian. " +
                "Use hand gestures in your descriptions (metaphorically!). Be warm and encouraging.",
                "Italian");

            return profileRepository;
        });

        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(registry =>
        {
            registry.AddAutoImport("./App/Code/Community/LeMessage/Source/web/ChatPage/index.tsx");
        });

        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IPageRepository>().Concurrent(async pageRepository =>
        {
            var page = new Page()
            {
                Title = "Messages",
                Key = "LeMessage",
                Url = "/messages"
            };

            page.From(new VNode("@page/lemessage-chat", [], []));

            await pageRepository.CreateAsync(page, UserContext.Guest, DataOptions.Bypass);

            return pageRepository;
        });

        Eventing.Eventing.Of<RolesEventTable>().Ready.Concurrent(async roleRepository =>
        {
            var adminRole = await roleRepository.FindByKeyAsync("admin", UserContext.Guest, DataOptions.Bypass);
            var userRole = await roleRepository.FindByKeyAsync("user", UserContext.Guest, DataOptions.Bypass);

            PolicyEnforcer.SetRule(adminRole.Id, "Profile_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Profile_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Profile_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Profile_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Profile_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Profile_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Profile_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Profile_delete", PermissionLevel.FullPermission);

            PolicyEnforcer.SetRule(adminRole.Id, "Conversation_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Conversation_read", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "Conversation_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Conversation_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Conversation_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Conversation_update", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "Conversation_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Conversation_delete", PermissionLevel.OwnedOnly);

            PolicyEnforcer.SetRule(adminRole.Id, "ChatMessage_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "ChatMessage_read", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "ChatMessage_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "ChatMessage_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "ChatMessage_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "ChatMessage_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "ChatMessage_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "ChatMessage_delete", PermissionLevel.FullPermission);

            return roleRepository;
        });

        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();
    public Task Install() => Noop();
    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}
