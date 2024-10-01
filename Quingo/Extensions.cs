using Microsoft.AspNetCore.Identity;

namespace Quingo;

public static class Extensions
{
    public static void MatchListSize<T>(this List<T> list, int size, Func<T> createNewFunc)
    {
        if (list.Count < size)
        {
            list.AddRange(Enumerable.Repeat((object)default!, size - list.Count).Select(_ => createNewFunc()));
        }
        else if (list.Count > size)
        {
            list.RemoveRange(size, list.Count - size);
        }
    }

    public static async Task EnsureRolesCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var exRole = await roleManager.FindByNameAsync("admin");
        if (exRole != null) return;

        await roleManager.CreateAsync(new IdentityRole { Name = "admin" });
    }
}
