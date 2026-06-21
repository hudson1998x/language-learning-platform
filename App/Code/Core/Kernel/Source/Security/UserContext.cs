using Microsoft.AspNetCore.Http;

namespace LLE.Kernel.Security;

public class UserContext
{
    public Guid? UserId { get; init; }
    public Guid? RoleId { get; init; }

    public static readonly UserContext Guest = new();

    public static UserContext FromHttpContext(HttpContext ctx)
    {
        var uid = ctx.Session.GetString("UserId");
        var rid = ctx.Session.GetString("RoleId");
        return new UserContext
        {
            UserId = uid is not null ? Guid.Parse(uid) : null,
            RoleId = rid is not null ? Guid.Parse(rid) : null
        };
    }
}
