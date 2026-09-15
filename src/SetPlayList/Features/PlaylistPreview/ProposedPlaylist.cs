namespace SetPlayList.Features.PlaylistPreview;

// Public only because [PersistentState] requires a public property getter (Blazor reflects on it
// across the prerender/circuit boundary); the public-API analyzers below don't apply, this type never
// leaves the assembly for real consumers.
#pragma warning disable CA1515 // type could be made internal
#pragma warning disable CA1002 // use Collection<T> instead of List<T>
#pragma warning disable CA1054 // Uri parameter instead of string
#pragma warning disable CA1056 // Uri property instead of string

public sealed class ProposedPlaylist
{
    public string Name { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public List<ProposedTrack> Tracks { get; init; } = [];
}

public sealed class ProposedTrack
{
    public required Song OriginalSong { get; init; }
    public List<SpotifyTrackOption> Options { get; init; } = [];
    public string? SelectedTrackId { get; set; }
}

public sealed record SpotifyTrackOption(
    string Id,
    string Name,
    List<string> Artists,
    string AlbumName,
    string AlbumImageUrl);

public sealed record Song(string Name, string? With, string? OriginalArtist, bool IsTape);

#pragma warning restore CA1056
#pragma warning restore CA1054
#pragma warning restore CA1002
#pragma warning restore CA1515
