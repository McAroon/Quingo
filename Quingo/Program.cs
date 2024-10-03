using Amazon.S3;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Quingo;
using Quingo.Application.State;
using Quingo.Components;
using Quingo.Components.Account;
using Quingo.Infrastructure.Database;
using Quingo.Infrastructure.Files;
using Quingo.Scripts;
using Quingo.Shared;
using Quingo.Shared.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
//.AddInteractiveWebAssemblyComponents();

builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

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

builder.Services.AddSingleton<GameStateService>();
builder.Services.AddScoped<GenerateStandardBingo>();
builder.Services.AddSingleton<TempUserStorage>();

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
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
//.AddInteractiveWebAssemblyRenderMode()

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGet("/scripts/generate-bingo", async (GenerateStandardBingo script) =>
{
    await script.Execute();
    return Results.Ok();
});

await app.EnsureRolesCreated();

app.Run();
