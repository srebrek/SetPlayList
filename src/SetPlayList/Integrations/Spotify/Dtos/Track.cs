namespace SetPlayList.Integrations.Spotify.Dtos;

internal sealed record Track(
    string Id,
    Album Album,
    List<Artist> Artists,
    string Name);
