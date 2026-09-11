namespace SetPlayList.Integrations.Spotify;

internal sealed record SpotifyApiSettings
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string CallbackPath { get; init; } = string.Empty;
}
