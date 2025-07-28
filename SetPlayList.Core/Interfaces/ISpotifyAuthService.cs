using Microsoft.AspNetCore.Http;

namespace SetPlayList.Core.Interfaces;
public interface ISpotifyAuthService
{
    string GetAuthorizationUrl(HttpContext context);
    Task<bool> HandleAuthorizationCallbackAsync(HttpContext context, string code, string state);
    Task<string?> GetCurrentAccessTokenAsync(HttpContext context);
    Task LogoutAsync(HttpContext context);
}
