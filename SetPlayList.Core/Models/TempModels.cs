using SetPlayList.Core.DTOs.SetlistFm;

namespace SetPlayList.Core.Models;

public record ProposedPlaylist(
    string Name,
    List<ProposedTrack> Tracks);

public record ProposedTrack(
    Song OriginalSong,
    List<SpotifyTrack> SpotifyOptions,
    SpotifyTrack? SelectedTrack);

public record SpotifyTrack(
    string Id,
    string Name,
    List<string> Artists,
    string AlbumName,
    string? AlbumImageUrl);