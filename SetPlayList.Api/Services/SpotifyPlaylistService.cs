using SetPlayList.Core.DTOs.Spotify;
using SetPlayList.Core.Interfaces;
using SetPlayList.Core.Models;
using System.Net;

namespace SetPlayList.Api.Services;

public class SpotifyPlaylistService(
    ISpotifyApiClient spotifyApiClient,
    ISetlistFmApiClient setlistFmApiClient,
    ISpotifyAuthService spotifyAuthService,
    ILogger<SpotifyPlaylistService> logger)
    : ISpotifyPlaylistService
{
    private readonly ISpotifyApiClient _spotifyApiClient = spotifyApiClient;
    private readonly ISetlistFmApiClient _setlistFmApiClient = setlistFmApiClient;
    private readonly ISpotifyAuthService _spotifyAuthService = spotifyAuthService;
    private readonly ILogger<SpotifyPlaylistService> _logger = logger;
    private const int _previewTrackCount = 3;

    public async Task<(ProposedPlaylist ProposedPlaylist, HttpStatusCode httpStatusCode)> GeneratePreviewAsync(string setlistId, string accessToken)
    {
        var (setlist, httpStatusCode) = await _setlistFmApiClient.GetSetlistAsync(setlistId);
        if (setlist is null || httpStatusCode != HttpStatusCode.OK)
        {
            throw new NotImplementedException();
        }

        var artistName = setlist.Artist.Name;
        if (string.IsNullOrWhiteSpace(artistName))
        {
            throw new NotImplementedException();
        }

        List<ProposedTrack> proposedTracks = new();
        ProposedPlaylist proposedPlaylist = new(
            "temporary name",
            proposedTracks);

        List<Task<(List<Track>? tracks, HttpStatusCode httpStatusCode)>> tasks = new();
        foreach (var set in setlist.Sets.Set)
        {
            foreach (var song in set.Song)
            {
                var songName = song.Name;
                if (string.IsNullOrWhiteSpace(songName))
                {
                    throw new NotImplementedException();
                }

                proposedTracks.Add(new ProposedTrack(new Song(song.Name, song.With?.Name, song.Cover?.Name, song.Tape), new(), null));
                tasks.Add(_spotifyApiClient.SearchTopTracksAsync(song.Cover?.Name ?? artistName, songName, _previewTrackCount, accessToken));
            }
        }

        var propositions = await Task.WhenAll(tasks);
        foreach (var (proposition, proposedTrack) in propositions.Zip(proposedTracks, (f, s) => (f, s)))
        {
            if (proposition.tracks is null || proposition.httpStatusCode != HttpStatusCode.OK)
            {
                throw new NotImplementedException();
            }

            foreach (var track in proposition.tracks)
            {
                proposedTrack.SpotifyOptions.Add(new(
                    track.Id,
                    track.Name,
                    track.Artists.Select(artist => artist.Name).ToList(),
                    track.Album.Name,
                    track.Album.Images.First().Url));
            }
            proposedTrack.SelectedTrack = proposedTrack.SpotifyOptions.FirstOrDefault();
        }

        return (proposedPlaylist, HttpStatusCode.OK);
    }

    public async Task CreatePlaylistOnSpotifyAsync(ProposedPlaylist finalPlaylist, string accessToken)
    {
        HttpStatusCode httpStatusCode;
        (var userId, httpStatusCode) = await _spotifyApiClient.GetCurrentUserIdAsync(accessToken);
        (var playlistId, httpStatusCode) = await _spotifyApiClient.CreatePlaylistAsync(userId, finalPlaylist.Name, accessToken);
        var trackIds = finalPlaylist.Tracks.Select(track => track.SelectedTrackId).ToList();
        httpStatusCode = await _spotifyApiClient.AddTracksToPlaylistAsync(playlistId, trackIds, accessToken);
    }
}
