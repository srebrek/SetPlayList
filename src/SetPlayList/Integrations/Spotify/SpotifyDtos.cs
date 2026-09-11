namespace SetPlayList.Integrations.Spotify;

internal sealed record SearchResponse(TracksContainer Tracks);

internal sealed record TracksContainer(List<Track> Items);

internal sealed record Track(string Id, Album Album, List<Artist> Artists, string Name);

internal sealed record Album(string Name, List<Image> Images);

internal sealed record Image(string Url, int? Height, int? Width);

internal sealed record Artist(string Name);

internal sealed record CreatePlaylistResponse(string Id);
