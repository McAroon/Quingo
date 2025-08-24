using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Quingo.Scripts;
using TimeZoneConverter;

namespace Quingo;

public static class Extensions
{
    public static void MatchListSize<T>(this List<T> list, int size, Func<int, T> createNewFunc)
    {
        if (list.Count < size)
        {
            list.AddRange(Enumerable.Repeat((object)default!, size - list.Count).Select((_, i) => createNewFunc(i + list.Count)));
        }
        else if (list.Count > size)
        {
            list.RemoveRange(size, list.Count - size);
        }
    }

    public static async Task SeedData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        await EnsureRolesCreated(scope);
        await GenerateStandardBingo(scope);
        return;

        async Task EnsureRolesCreated(IServiceScope serviceScope)
        {
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await EnsureRoleCreated("admin");
            await EnsureRoleCreated("editor");
            return;

            async Task EnsureRoleCreated(string role)
            {
                var exRole = await roleManager.FindByNameAsync(role);
                if (exRole != null) return;

                await roleManager.CreateAsync(new IdentityRole { Name = role });
            }
        }

        async Task GenerateStandardBingo(IServiceScope serviceScope)
        {
            var script = serviceScope.ServiceProvider.GetRequiredService<GenerateStandardBingo>();
            await script.Execute();
        }
    }

    public static string ToTimerString(this TimeSpan value, TimeSpan maxValue)
    {
        if (maxValue.Hours > 0)
        {
            return value.ToString(@"hh\:mm\:ss");
        }

        if (maxValue.Minutes > 0)
        {
            return value.ToString(@"mm\:ss");
        }

        return value.Seconds.ToString();
    }
    
    public static string FormatWithTimeZone(this DateTime date, string? timeZone)
    {

        var result = !string.IsNullOrEmpty(timeZone) 
                  && TZConvert.TryGetTimeZoneInfo(timeZone, out var tzInfo)
            ? TimeZoneInfo.ConvertTimeFromUtc(date, tzInfo) 
            : date;

        return result.ToString("g", CultureInfo.CurrentUICulture);
    }
    
    public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300)
    {
        CancellationTokenSource? cancelTokenSource = null;

        return arg =>
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();

            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        func(arg);
                    }
                }, TaskScheduler.Default);
        };
    }
    
    public static Action Debounce(this Action func, int milliseconds = 300)
    {
        CancellationTokenSource? cancelTokenSource = null;

        return () =>
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();

            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        func();
                    }
                }, TaskScheduler.Default);
        };
    }
}
