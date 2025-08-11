namespace SetPlayList.Core.DTOs.Spotify;

public record SearchResponse(
    TracksContainer Tracks
    );

public record TracksContainer(List<Track> Items);