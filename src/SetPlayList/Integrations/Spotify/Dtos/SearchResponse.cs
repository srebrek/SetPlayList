namespace SetPlayList.Integrations.Spotify.Dtos;

internal sealed record SearchResponse(TracksContainer Tracks);

internal sealed record TracksContainer(List<Track> Items);
