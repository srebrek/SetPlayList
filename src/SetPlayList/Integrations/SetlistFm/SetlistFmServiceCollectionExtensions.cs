namespace SetPlayList.Integrations.SetlistFm;

internal static class SetlistFmServiceCollectionExtensions
{
    public static IServiceCollection AddSetlistFmIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SetlistFmApiSettings>(configuration.GetSection("SetlistFm"));
        services.AddHttpClient<SetlistFmApiClient>();

        return services;
    }
}
