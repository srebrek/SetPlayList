using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;

namespace SetPlayList.Integrations.Spotify.Auth;

internal static class SpotifyAuthServiceCollectionExtensions
{
    public static IServiceCollection AddSpotifyAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie()
            .AddOAuth("Spotify", options =>
            {
                options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
                options.TokenEndpoint = "https://accounts.spotify.com/api/token";
                options.UserInformationEndpoint = "https://api.spotify.com/v1/me";

                options.Scope.Add("playlist-modify-public");
                options.Scope.Add("playlist-modify-private");

                options.ClaimActions.MapJsonKey(SpotifyClaims.UserIdType, "id");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = context =>
                    {
                        if (context.AccessToken is not null)
                        {
                            context.Identity?.AddClaim(new Claim(SpotifyClaims.AccessTokenType, context.AccessToken));
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddOptions<OAuthOptions>("Spotify").Configure<IOptions<SpotifyApiSettings>>((options, spotifyOptions) =>
        {
            SpotifyApiSettings settings = spotifyOptions.Value;

            options.ClientId = string.IsNullOrEmpty(settings.ClientId)
                ? "not-configured"
                : settings.ClientId;

            options.ClientSecret = string.IsNullOrEmpty(settings.ClientSecret)
                ? "not-configured"
                : settings.ClientSecret;

            options.CallbackPath = string.IsNullOrEmpty(settings.CallbackPath)
                ? "/signin-spotify"
                : settings.CallbackPath;
        });

        return services;
    }
}
