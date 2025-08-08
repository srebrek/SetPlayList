using SetPlayList.Core.DTOs.Spotify;
using System.Net;

namespace SetPlayList.Core.Interfaces;
public interface ISpotifyApiClient
{
    string GetAuthorizationUrl(string state);
    Task<(AuthToken? authToken, HttpStatusCode httpStatusCode)> ExchangeCodeForTokenAsync(string code);
}
