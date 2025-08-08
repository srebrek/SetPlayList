using SetPlayList.Core.Interfaces;
using System.Net;

namespace SetPlayList.Api.Services;

public class SpotifyAuthService(ISpotifyApiClient spotifyApiClient, ILogger<SpotifyAuthService> logger) : ISpotifyAuthService
{
    private readonly ISpotifyApiClient _spotifyApiClient = spotifyApiClient;
    private readonly ILogger<SpotifyAuthService> _logger = logger;
    private readonly string _stateCookieName = "spotify_auth_state";
    private readonly string _tokenCookieName = "spotify_token";
    private readonly int _cookieExpirationTime = 10; // Minute

    public string GetAuthorizationUrl(HttpContext context)
    {
        var state = Guid.NewGuid().ToString("N");
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_cookieExpirationTime)
        };
        context.Response.Cookies.Append(_stateCookieName, state, cookieOptions);

        return _spotifyApiClient.GetAuthorizationUrl(state);
    }

    public Task<string?> GetCurrentAccessTokenAsync(HttpContext context)
    {
        var token = context.Request.Cookies[_tokenCookieName];
        return Task.FromResult(token);
    }

    public async Task<bool> HandleAuthorizationCallbackAsync(HttpContext context, string code, string state)
    {
        var savedState = context.Request.Cookies[_stateCookieName];
        context.Response.Cookies.Delete(_stateCookieName);

        if (string.IsNullOrEmpty(savedState) || savedState != state)
        {
            _logger.LogWarning("Spotify authorization callback failed: state mismatch or missing. This could indicate a CSRF attack attempt. Provided state: {ProvidedState}, expected (from cookie): {ExpectedState}",
            state, savedState ?? "null");
            return false;
        }

        var (authToken, httpStatusCode) = await _spotifyApiClient.ExchangeCodeForTokenAsync(code);
        if (httpStatusCode != HttpStatusCode.OK || authToken is null || string.IsNullOrEmpty(authToken.AccessToken))
        {
            _logger.LogError("Failed to handle Spotify authorization callback because a token could not be obtained from the API client.");
            return false;
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
        };
        context.Response.Cookies.Append(_tokenCookieName, authToken.AccessToken, cookieOptions);

        _logger.LogInformation("Successfully handled Spotify authorization callback and set authentication cookie for a user.");

        return true;
    }

    public Task LogoutAsync(HttpContext context)
    {
        if (context.Request.Cookies.ContainsKey(_tokenCookieName))
        {
            _logger.LogInformation("User is logging out. Deleting authentication cookie.");
            context.Response.Cookies.Delete(_tokenCookieName);
        }
        else
        {
            _logger.LogWarning("Logout endpoint was called, but no authentication cookie was found.");
        }

        return Task.CompletedTask;
    }

    // TODO: Add token refresh
}
