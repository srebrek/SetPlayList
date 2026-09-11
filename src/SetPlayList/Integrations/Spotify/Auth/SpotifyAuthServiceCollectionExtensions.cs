using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
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
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "display_name");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        if (context.AccessToken is not null)
                        {
                            context.Identity?.AddClaim(new Claim(SpotifyClaims.AccessTokenType, context.AccessToken));
                        }

                        using HttpRequestMessage request = new(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        using HttpResponseMessage response = await context.Backchannel.SendAsync(
                            request,
                            HttpCompletionOption.ResponseHeadersRead,
                            context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        using Stream stream = await response.Content.ReadAsStreamAsync(
                            context.HttpContext.RequestAborted);

                        using JsonDocument user = await JsonDocument.ParseAsync(
                            stream, cancellationToken: context.HttpContext.RequestAborted);

                        context.RunClaimActions(user.RootElement);
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
