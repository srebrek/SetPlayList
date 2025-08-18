using SetPlayList.Core.Models;
using System.Net;

namespace SetPlayList.Core.Interfaces;

public interface ISpotifyPlaylistService
{
    Task<(ProposedPlaylist ProposedPlaylist, HttpStatusCode httpStatusCode)> GeneratePreviewAsync(string setlistUrl, string accessToken);
    Task CreatePlaylistOnSpotifyAsync(ProposedPlaylist finalPlaylist, string accessToken);
}
