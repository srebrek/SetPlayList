namespace SetPlayList.Core.DTOs.Spotify;

public record Track(
    string Id,
    Album Album,
    List<Artist> Artists,
    string Name
    );
