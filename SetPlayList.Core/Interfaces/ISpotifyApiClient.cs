using SetPlayList.Core.DTOs.Spotify;
using System.Net;

namespace SetPlayList.Core.Interfaces;
public interface ISpotifyApiClient
{
    string GetAuthorizationUrl(string state);
    Task<(AuthToken? authToken, HttpStatusCode httpStatusCode)> ExchangeCodeForTokenAsync(string code);
    Task<(List<Track>? tracks, HttpStatusCode httpStatusCode)> SearchTopTracksAsync(string artistName, string trackName, int limit, string accessToken);
    Task<(string? userId, HttpStatusCode httpStatusCode)> GetCurrentUserIdAsync(string accessToken);
    Task<(string? playlistId, HttpStatusCode httpStatusCode)> CreatePlaylistAsync(string userId, string playlistName, string accessToken);
    Task<HttpStatusCode> AddTracksToPlaylistAsync(string userId, List<string> ids, string accessToken);
}
