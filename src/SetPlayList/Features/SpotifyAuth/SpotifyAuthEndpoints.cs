using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SetPlayList.Features.SpotifyAuth;

internal static class SpotifyAuthEndpoints
{
    public static void MapSpotifyAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/auth/login", (HttpContext context) =>
            Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, ["Spotify"]));

        app.MapGet("/auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });
    }
}
