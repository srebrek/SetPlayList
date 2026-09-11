namespace SetPlayList.Integrations.Spotify;

internal static class SpotifyServiceCollectionExtensions
{
    public static IServiceCollection AddSpotifyIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SpotifyApiSettings>(configuration.GetSection("Spotify"));
        services.AddHttpClient<SpotifyApiClient>();

        return services;
    }
}
