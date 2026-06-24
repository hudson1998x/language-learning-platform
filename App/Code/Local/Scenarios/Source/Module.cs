using LLE.Auth.Events;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Security;
using LLE.Pages;
using LLE.ReactFrontend.Events;
using LLE.TypeScript.Events;
using LLE.UiIR;

namespace LLE.Scenarios;

public class ScenarioModule : IModuleLoader
{
    public Task AppStart()
    {
        

        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IPageRepository>().Concurrent(async pageRepository =>
        {
            var page = new Page()
            {
                Title = "Scenarios",
                Key = "scenarios",
                Url = "/scenarios"
            };
            
            page.From(new VNode("@page/scenarios-page", [], []));
            
            await pageRepository.CreateAsync(page, UserContext.Guest, DataOptions.Bypass);

            return pageRepository;
        });
        
        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(registry =>
        {
            registry.AddAutoImport("./App/Code/Local/Scenarios/Source/web/Pages/Scenarios/index.tsx");
            registry.AddAutoImport("./App/Code/Local/Scenarios/Source/web/Pages/ScenarioSession/index.tsx");
        });
        
        Eventing.Eventing.Of<RolesEventTable>().Ready.Concurrent(async roleRepository =>
        {
            var adminRole = await roleRepository.FindByKeyAsync("admin", UserContext.Guest,  DataOptions.Bypass);
            var userRole = await roleRepository.FindByKeyAsync("user", UserContext.Guest, DataOptions.Bypass);
            
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_read", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_update", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_delete", PermissionLevel.OwnedOnly);
            
            return roleRepository;
        });
        
        Features.LoadFeatures();
        
        return Noop();
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}