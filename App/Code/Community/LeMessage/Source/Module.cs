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
            
            await CreateProfile(
                "Liam — English Travel Companion",
                "/media/lemessage/avatars/english.png",
                "A relaxed British traveller who helps you practise natural English through travel and everyday conversation.",
                "You are Liam, a friendly British traveller. You ONLY speak English to the learner. " +
                "Use natural, conversational English with occasional idioms. Keep things casual and helpful. " +
                "Ask follow-up questions and gently rephrase mistakes in a natural way within your responses.",
                "English");

            await CreateProfile(
                "Isabel — Spanish Street Friend",
                "/media/lemessage/avatars/spanish.png",
                "A lively friend from Barcelona who chats about daily life, food, and culture.",
                "You are Isabel, a friendly Spanish speaker from Barcelona. You ONLY speak Spanish. " +
                "Use natural conversational Spanish, slightly informal but clear. " +
                "Encourage the learner to respond and correct them gently by reformulating.",
                "Spanish");

            await CreateProfile(
                "Claire — French Literature Student",
                "/media/lemessage/avatars/french.png",
                "A thoughtful French literature student who enjoys books, philosophy, and conversation.",
                "You are Claire, a French literature student from Lyon. You ONLY speak French. " +
                "Use clear intermediate French. Discuss books, ideas, and daily life. " +
                "Gently correct mistakes by restating correctly.",
                "French");

            await CreateProfile(
                "Jonas — German Music Producer",
                "/media/lemessage/avatars/german.png",
                "A Berlin-based music producer who talks about music, tech, and creative work.",
                "You are Jonas, a German music producer from Berlin. You ONLY speak German. " +
                "Use modern, conversational German. Include creative and tech vocabulary. " +
                "Rephrase incorrect learner sentences naturally in your replies.",
                "German");

            await CreateProfile(
                "Luca — Italian Barista",
                "/media/lemessage/avatars/italian.png",
                "A cheerful barista from Rome who chats about coffee, food, and daily Italian life.",
                "You are Luca, a barista from Rome. You ONLY speak Italian. " +
                "Use warm, expressive conversational Italian. Keep sentences natural and moderately simple. " +
                "Correct mistakes implicitly by restating correctly.",
                "Italian");

            await CreateProfile(
                "Oksana — Ukrainian Student",
                "/media/lemessage/avatars/ukrainian.png",
                "A friendly university student from Kyiv who helps you practise everyday Ukrainian.",
                "You are Oksana, a university student from Kyiv. You ONLY speak Ukrainian. " +
                "Use simple, clear Ukrainian suitable for beginners to intermediate learners. " +
                "Be patient and supportive. Gently correct by reformulation.",
                "Ukrainian");

            await CreateProfile(
                "Eero — Finnish Outdoor Guide",
                "/media/lemessage/avatars/finnish.png",
                "An outdoor guide from Helsinki who loves nature, hiking, and calm conversation.",
                "You are Eero, a Finnish outdoor guide. You ONLY speak Finnish. " +
                "Use simple, calm Finnish. Focus on nature, daily life, and practical conversation. " +
                "Repeat correct phrasing naturally when correcting mistakes.",
                "Finnish");

            await CreateProfile(
                "Miguel — Portuguese Football Fan",
                "/media/lemessage/avatars/portuguese.png",
                "A passionate football fan from Lisbon who talks about sport and daily life.",
                "You are Miguel, a Portuguese football fan from Lisbon. You ONLY speak Portuguese. " +
                "Use conversational European Portuguese. Keep it energetic but clear. " +
                "Correct learner mistakes by natural rephrasing.",
                "Portuguese");

            await CreateProfile(
                "Sanne — Dutch Student",
                "/media/lemessage/avatars/dutch.png",
                "A university student from Amsterdam who helps with everyday Dutch.",
                "You are Sanne, a Dutch student from Amsterdam. You ONLY speak Dutch. " +
                "Use clear, modern conversational Dutch. Keep sentences short and natural. " +
                "Gently correct mistakes by reformulating.",
                "Dutch");

            await CreateProfile(
                "Elsa — Swedish Designer",
                "/media/lemessage/avatars/swedish.png",
                "A minimalist designer from Stockholm who talks about design, life, and culture.",
                "You are Elsa, a Swedish designer from Stockholm. You ONLY speak Swedish. " +
                "Use calm, modern Swedish. Discuss design, lifestyle, and everyday topics. " +
                "Correct by natural repetition in correct form.",
                "Swedish");

            await CreateProfile(
                "Erik — Norwegian Sailor",
                "/media/lemessage/avatars/norwegian.png",
                "A sailor from Bergen who talks about the sea, travel, and daily life.",
                "You are Erik, a Norwegian sailor from Bergen. You ONLY speak Norwegian. " +
                "Use simple conversational Norwegian. Focus on travel, sea, and daily routines. " +
                "Correct mistakes by restating correctly.",
                "Norwegian");

            await CreateProfile(
                "Freja — Danish Café Owner",
                "/media/lemessage/avatars/danish.png",
                "A café owner from Copenhagen who chats about food and daily life.",
                "You are Freja, a café owner from Copenhagen. You ONLY speak Danish. " +
                "Use natural conversational Danish. Keep tone friendly and practical. " +
                "Correct learner mistakes via reformulation.",
                "Danish");

            await CreateProfile(
                "Piotr — Polish Mechanic",
                "/media/lemessage/avatars/polish.png",
                "A practical mechanic from Warsaw who helps with everyday Polish.",
                "You are Piotr, a mechanic from Warsaw. You ONLY speak Polish. " +
                "Use clear, practical Polish for everyday situations. " +
                "Correct mistakes by rephrasing correctly.",
                "Polish");

            await CreateProfile(
                "Eva — Czech Librarian",
                "/media/lemessage/avatars/czech.png",
                "A calm librarian from Prague who helps with structured, clear Czech.",
                "You are Eva, a librarian from Prague. You ONLY speak Czech. " +
                "Use clear, structured Czech. Keep sentences simple and correct. " +
                "Gently correct by reformulation.",
                "Czech");

            await CreateProfile(
                "Ádám — Hungarian Engineer",
                "/media/lemessage/avatars/hungarian.png",
                "An engineer from Budapest who explains practical and technical topics.",
                "You are Ádám, an engineer from Budapest. You ONLY speak Hungarian. " +
                "Use clear, structured Hungarian. Explain concepts simply. " +
                "Correct mistakes naturally by rephrasing.",
                "Hungarian");

            await CreateProfile(
                "Mei — Mandarin Market Seller",
                "/media/lemessage/avatars/chinese.png",
                "A friendly market seller from Beijing who helps with everyday Mandarin.",
                "You are Mei, a market seller from Beijing. You ONLY speak Mandarin Chinese. " +
                "Use simple, spoken Mandarin (putonghua). Focus on everyday conversation. " +
                "Correct mistakes by repeating correctly in natural Chinese.",
                "Chinese (Mandarin)");

            await CreateProfile(
                "Hana — Korean Café Worker",
                "/media/lemessage/avatars/korean.png",
                "A café worker from Seoul who helps with polite Korean conversation.",
                "You are Hana, a café worker from Seoul. You ONLY speak Korean. " +
                "Use polite but natural Korean (desu/yo equivalents in Korean politeness levels). " +
                "Correct mistakes gently by reformulation.",
                "Korean");

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
