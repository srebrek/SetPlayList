using Microsoft.AspNetCore.Authentication;

namespace SetPlayList.Integrations.Spotify.Auth;

internal static class SpotifyAuthEndpoints
{
    public static void MapSpotifyAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/auth/login", () =>
            Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, ["Spotify"]));

        app.MapGet("/auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync();
            return Results.Redirect("/");
        });
    }
}
