using LLE.Auth.Events;
using LLE.Kernel.AutoEntity;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Security;
using LLE.MusicTranslation.Media.Albums;
using LLE.MusicTranslation.Media.Artists;
using LLE.MusicTranslation.Media.Tracks;
using LLE.Pages;
using LLE.ReactFrontend.Events;
using LLE.TypeScript.Events;
using LLE.UiIR;

namespace LLE.MusicTranslation;

public class MusicTranslationModule : IModuleLoader
{
    public Task AppStart()
    {
        AutoEntityFeature.AutoFeature<Album, IAlbumRepository>();
        AutoEntityFeature.AutoFeature<Artist, IArtistRepository>();
        AutoEntityFeature.AutoFeature<Track, ITrackRepository>();
        
        Features.LoadFeatures();

        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(generator =>
        {
            generator.AddAutoImport("./App/Code/Community/MusicTranslation/Source/web/TranslationPage/index.tsx");
        });
        
        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IPageRepository>().Concurrent(async pageRepository =>
        {

            var page = new Page()
            {
                Title = "Music Translation",
                Key = "MusicTranslation",
                Url = "/musiclyrics"
            };
            
            page.From(new VNode("@page/music-translation-index", [], []));
            
            await pageRepository.CreateAsync(page, UserContext.Guest, DataOptions.Bypass);

            return pageRepository;
        });
        
        Eventing.Eventing.Of<RolesEventTable>().Ready.Concurrent(async roleRepository =>
        {
            var adminRole = await roleRepository.FindByKeyAsync("admin", UserContext.Guest,  DataOptions.Bypass);
            var userRole = await roleRepository.FindByKeyAsync("user", UserContext.Guest, DataOptions.Bypass);
            
            PolicyEnforcer.SetRule(adminRole.Id, "Artist_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Artist_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Artist_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Artist_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Artist_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Artist_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Artist_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Artist_delete", PermissionLevel.FullPermission);
            
            PolicyEnforcer.SetRule(adminRole.Id, "Track_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Track_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Track_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Track_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Track_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Track_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Track_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Track_delete", PermissionLevel.FullPermission);
            
            PolicyEnforcer.SetRule(adminRole.Id, "Album_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Album_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Album_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Album_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Album_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Album_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Album_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Album_delete", PermissionLevel.FullPermission);
            
            
            
            return roleRepository;
        });
        
        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();
    public Task Install() => Noop();
    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}