using Microsoft.AspNetCore.Authentication;

namespace SetPlayList.Integrations.Spotify.Auth;

internal static class SpotifyAuthEndpoints
{
    public static void MapSpotifyAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/auth/login", (string? returnUrl) =>
        {
            string redirectUri = IsLocalUrl(returnUrl) ? returnUrl! : "/";
            return Results.Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, ["Spotify"]);
        });

        app.MapPost("/auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync();
            return Results.Redirect("/");
        });

        // Rejects protocol-relative URLs ("//evil.com", read by the browser as
        // "https://evil.com") that Uri.IsWellFormedUriString(..., Relative) alone lets through.
        static bool IsLocalUrl(string? url) =>
            !string.IsNullOrEmpty(url)
            && url[0] == '/'
            && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
    }
}
