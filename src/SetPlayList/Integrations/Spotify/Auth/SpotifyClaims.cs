using System.Security.Claims;

namespace SetPlayList.Integrations.Spotify.Auth;

internal static class SpotifyClaims
{
    public const string AccessTokenType = "spotify_access_token";
    public const string UserIdType = "spotify_user_id";

    public static string? GetAccessToken(ClaimsPrincipal user) => user.FindFirst(AccessTokenType)?.Value;
    public static string? GetUserId(ClaimsPrincipal user) => user.FindFirst(UserIdType)?.Value;
}
