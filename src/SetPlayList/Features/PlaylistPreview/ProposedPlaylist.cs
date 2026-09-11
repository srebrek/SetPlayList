namespace SetPlayList.Features.PlaylistPreview;

internal sealed class ProposedPlaylist
{
    public string Name { get; set; } = string.Empty;
    public List<ProposedTrack> Tracks { get; init; } = [];
}

internal sealed class ProposedTrack
{
    public required Song OriginalSong { get; init; }
    public List<SpotifyTrackOption> Options { get; init; } = [];
    public string? SelectedTrackId { get; set; }
}

internal sealed record SpotifyTrackOption(
    string Id,
    string Name,
    List<string> Artists,
    string AlbumName,
    string AlbumImageUrl);

internal sealed record Song(string Name, string? With, string? OriginalArtist, bool IsTape);
