namespace SetPlayList.Integrations.SetlistFm;

internal sealed record SetlistFmApiSettings
{
    public string ClientSecret { get; init; } = string.Empty;
}
