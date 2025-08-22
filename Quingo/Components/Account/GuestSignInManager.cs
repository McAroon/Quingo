using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Quingo.Shared.Entities;

namespace Quingo.Components.Account;

public class GuestSignInManager
{
    private readonly IHttpContextAccessor _contextAccessor;
    private HttpContext? _context;

    public GuestSignInManager(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor,
        IHttpContextAccessor contextAccessor)
    {
        UserManager = userManager;
        Options = optionsAccessor.Value;
        _contextAccessor = contextAccessor;
    }

    public UserManager<ApplicationUser> UserManager { get; set; }

    public IdentityOptions Options { get; private set; }

    
    public HttpContext Context
    {
        get
        {
            var context = _context ?? _contextAccessor?.HttpContext;
            if (context == null)
            {
                throw new InvalidOperationException("HttpContext must not be null.");
            }

            return context;
        }
        set { _context = value; }
    }

    public const string AuthenticationScheme = "Identity.Guest";

    public async Task<SignInResult> GuestSignInAsync(string userName)
    {
        Claim[] claims =
        [
            new(Options.ClaimsIdentity.UserIdClaimType, Guid.NewGuid().ToString()),
            new(Options.ClaimsIdentity.UserNameClaimType, userName),
            new(Options.ClaimsIdentity.RoleClaimType, "guest"),
        ];
        var id = new ClaimsIdentity(AuthenticationScheme,
            Options.ClaimsIdentity.UserNameClaimType,
            Options.ClaimsIdentity.RoleClaimType);
        id.AddClaims(claims);
        var userPrincipal = new ClaimsPrincipal(id);
        
        await Context.SignInAsync(AuthenticationScheme, userPrincipal, new AuthenticationProperties());
        Context.User = userPrincipal;

        return SignInResult.Success;
    }
}