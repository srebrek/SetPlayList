using SetPlayList.Core.DTOs;

namespace SetPlayList.Core.Interfaces;
public interface ISpotifyApiClient
{
    string GetAuthorizationUrl(string state);
    Task<TokenResponse?> ExchangeCodeForTokenAsync(string code);
}
