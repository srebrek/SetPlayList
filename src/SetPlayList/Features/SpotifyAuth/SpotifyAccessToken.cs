using System.Security.Claims;

namespace SetPlayList.Features.SpotifyAuth;

internal static class SpotifyAccessToken
{
    public const string ClaimType = "spotify_access_token";

    public static string? Get(ClaimsPrincipal user) => user.FindFirst(ClaimType)?.Value;
}
