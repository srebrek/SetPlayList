using Microsoft.AspNetCore.Authentication;

namespace SetPlayList.Integrations.Spotify.Auth;

internal static class SpotifyAuthEndpoints
{
    public static void MapSpotifyAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/auth/login", (string? returnUrl) =>
        {
            string redirectUri = returnUrl is not null && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
                ? returnUrl
                : "/";
            return Results.Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, ["Spotify"]);
        });

        app.MapGet("/auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync();
            return Results.Redirect("/");
        });
    }
}
