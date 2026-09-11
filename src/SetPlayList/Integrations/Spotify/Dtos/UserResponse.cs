namespace SetPlayList.Integrations.Spotify.Dtos;

internal sealed record UserResponse(string DisplayName, string Id);

internal sealed record CreatePlaylistResponse(string Id);
