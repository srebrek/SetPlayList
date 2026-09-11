using System.Globalization;
using SetPlayList.Common;
using SetPlayList.Integrations.SetlistFm;
using SetPlayList.Integrations.SetlistFm.Dtos;
using SetPlayList.Integrations.Spotify;
using SetPlayList.Integrations.Spotify.Dtos;

namespace SetPlayList.Features.PlaylistPreview;

internal sealed partial class PlaylistPreviewService(
    SetlistFmApiClient setlistFmApiClient,
    SpotifyApiClient spotifyApiClient,
    ILogger<PlaylistPreviewService> logger)
{
    private const int PreviewTrackCount = 3;

    private readonly SetlistFmApiClient _setlistFmApiClient = setlistFmApiClient;
    private readonly SpotifyApiClient _spotifyApiClient = spotifyApiClient;
    private readonly ILogger<PlaylistPreviewService> _logger = logger;

    public async Task<Result<ProposedPlaylist>> GeneratePreviewAsync(string setlistId, string accessToken, CancellationToken cancellationToken)
    {
        Result<Setlist> setlistResult = await _setlistFmApiClient.GetSetlistAsync(setlistId, cancellationToken);
        if (setlistResult.IsFailure)
        {
            return setlistResult.Error!;
        }

        Setlist setlist = setlistResult.Value;
        ProposedPlaylist playlist = new()
        {
            Tracks = setlist.Sets.Set
                .SelectMany(set => set.Song)
                .Select(song => new ProposedTrack
                {
                    OriginalSong = new Song(song.Name, song.With?.Name, song.Cover?.Name, song.Tape),
                })
                .ToList(),
        };

        _ = DateOnly.TryParseExact(setlist.EventDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date);
        List<string?> titleParts = [setlist.Artist.Name, setlist.Venue.City.Name, date.Year.ToString(CultureInfo.InvariantCulture)];
        playlist.Name = string.Join(" - ", titleParts.Where(part => !string.IsNullOrEmpty(part)));

        IEnumerable<Task<Result<List<Track>>>> searches = playlist.Tracks.Select(track => _spotifyApiClient.SearchTopTracksAsync(
            track.OriginalSong.OriginalArtist ?? setlist.Artist.Name,
            track.OriginalSong.Name,
            PreviewTrackCount,
            accessToken,
            cancellationToken));
        Result<List<Track>>[] searchResults = await Task.WhenAll(searches);

        foreach ((Result<List<Track>> searchResult, ProposedTrack track) in searchResults.Zip(playlist.Tracks))
        {
            if (searchResult.IsFailure)
            {
                LogNoMatches(_logger, track.OriginalSong.Name, searchResult.Error!.Message);
                continue;
            }

            track.Options.AddRange(searchResult.Value.Select(t => new SpotifyTrackOption(
                t.Id,
                t.Name,
                t.Artists.Select(a => a.Name).ToList(),
                t.Album.Name,
                t.Album.Images.Count > 0 ? t.Album.Images[0].Url : string.Empty)));
            track.SelectedTrackId = track.Options.Count > 0 ? track.Options[0].Id : null;
        }

        return playlist;
    }

    public async Task<Result<bool>> CreatePlaylistOnSpotifyAsync(ProposedPlaylist playlist, string accessToken, CancellationToken cancellationToken)
    {
        Result<string> userIdResult = await _spotifyApiClient.GetCurrentUserIdAsync(accessToken, cancellationToken);
        if (userIdResult.IsFailure)
        {
            return userIdResult.Error!;
        }

        Result<string> playlistIdResult = await _spotifyApiClient.CreatePlaylistAsync(userIdResult.Value, playlist.Name, accessToken, cancellationToken);
        if (playlistIdResult.IsFailure)
        {
            return playlistIdResult.Error!;
        }

        List<string> trackIds = playlist.Tracks
            .Select(track => track.SelectedTrackId)
            .Where(id => id is not null)
            .Select(id => id!)
            .ToList();

        return await _spotifyApiClient.AddTracksToPlaylistAsync(playlistIdResult.Value, trackIds, accessToken, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No Spotify matches for '{Song}': {Error}")]
    private static partial void LogNoMatches(ILogger logger, string song, string error);
}
