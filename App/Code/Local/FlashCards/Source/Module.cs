using LLE.Auth.Events;
using LLE.FlashCards.FlashCards;
using LLE.Kernel.AutoEntity;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Security;
using LLE.Pages;
using LLE.UiIR;

namespace LLE.FlashCards;

public class FlashCardsModule : IModuleLoader
{
    public Task AppStart()
    {
        AutoEntityFeature.AutoFeature<FlashCard, IFlashCardRepository>();
        
        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IPageRepository>().Concurrent(async pageRepository =>
        {

            var page = new Page()
            {
                Title = "Flash Cards",
                Key = "FlashCards",
                Url = "/flashcards"
            };
            
            page.From(new VNode("@component/Pages/FlashCards", [], []));
            
            await pageRepository.CreateAsync(page, UserContext.Guest, DataOptions.Bypass);

            return pageRepository;
        });

        Eventing.Eventing.Of<RolesEventTable>().Ready.Concurrent(async roleRepository =>
        {
            var adminRole = await roleRepository.FindByKeyAsync("admin", UserContext.Guest,  DataOptions.Bypass);
            var userRole = await roleRepository.FindByKeyAsync("user", UserContext.Guest, DataOptions.Bypass);
            
            PolicyEnforcer.SetRule(adminRole.Id, "FlashCard_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "FlashCard_read", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "FlashCard_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "FlashCard_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "FlashCard_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "FlashCard_update", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "FlashCard_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "FlashCard_delete", PermissionLevel.OwnedOnly);
            
            
            
            return roleRepository;
        });

        return Noop();
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}