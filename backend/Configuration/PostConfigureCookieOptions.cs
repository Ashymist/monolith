using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

public class PostConfigureCookieOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _sessionStoreService;

    public PostConfigureCookieOptions(ITicketStore sessionStore)
    {
        _sessionStoreService = sessionStore;
    }

    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        options.SessionStore = _sessionStoreService;
    }


}