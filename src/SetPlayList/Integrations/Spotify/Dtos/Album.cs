namespace SetPlayList.Integrations.Spotify.Dtos;

internal sealed record Album(
    string Name,
    List<Image> Images);
