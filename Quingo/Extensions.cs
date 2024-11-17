using Microsoft.AspNetCore.Identity;
using TimeZoneConverter;

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

    public static string ToStringHoursOptional(this TimeSpan span)
    {
        return span.Hours > 0 ? span.ToString(@"hh\:mm\:ss") : span.ToString(@"mm\:ss");
    }
    
    public static string FormatWithTimeZone(this DateTime date, string? timeZone)
    {

        var result = !string.IsNullOrEmpty(timeZone) 
                  && TZConvert.TryGetTimeZoneInfo(timeZone, out var tzInfo)
            ? TimeZoneInfo.ConvertTimeFromUtc(date, tzInfo) 
            : date;
        return $"{result:d} {result:t}";
    }
}
