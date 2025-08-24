using Amazon.S3;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MudBlazor.Services;
using Quingo;
using Quingo.Application.Core;
using Quingo.Application.Packs.Services;
using Quingo.Application.SignalR;
using Quingo.Components;
using Quingo.Components.Account;
using Quingo.Infrastructure;
using Quingo.Infrastructure.Database;
using Quingo.Infrastructure.Database.Repos;
using Quingo.Infrastructure.Files;
using Quingo.Scripts;
using Quingo.Shared;
using Quingo.Shared.Constants;
using Quingo.Shared.Entities;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
//.AddInteractiveWebAssemblyComponents();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();

// builder.Logging.AddConfiguration(
//     builder.Configuration.GetSection("Logging"));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("QuingoDB") ?? throw new InvalidOperationException("Connection string 'QuingoDB' not found.");
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.EnrichNpgsqlDbContext<ApplicationDbContext>();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddSignalR();

builder.Services.AddSingleton<GameService>();
builder.Services.AddScoped<TournamentLobbyService>();
builder.Services.AddScoped<PlayoffService>();
builder.Services.AddScoped<GenerateStandardBingo>();
builder.Services.AddSingleton<TempUserStorage>();
builder.Services.AddScoped<PackRepo>();
builder.Services.AddScoped<PackNodeService>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<UserConnectionTracker>();
builder.Services.AddScoped<CircuitHandler, TrackingCircuitHandler>();

// S3
var s3Settings = builder.Configuration.GetSection(nameof(S3Settings)).Get<S3Settings>() ?? new();
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3Config = new AmazonS3Config
    {
        ServiceURL = s3Settings.Endpoint,
        ForcePathStyle = true,
    };

    return new AmazonS3Client(s3Settings.AccessKey, s3Settings.SecretKey, s3Config);
});

builder.Services.AddScoped<FileStoreService>();
builder.Services.Configure<FileStoreSettings>(builder.Configuration.GetSection(nameof(FileStoreSettings)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapHub<LobbyHub>(SignalRConstants.LobbyHubPath);
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
//.AddInteractiveWebAssemblyRenderMode()

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.SeedData();

app.Run();
