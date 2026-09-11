using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;
using SetPlayList.Components;
using SetPlayList.Features.PlaylistPreview;
using SetPlayList.Features.SpotifyAuth;
using SetPlayList.Integrations.SetlistFm;
using SetPlayList.Integrations.Spotify;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpotifyApiSettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.Configure<SetlistFmApiSettings>(builder.Configuration.GetSection("SetlistFm"));

builder.Services.AddHttpClient<SpotifyApiClient>();
builder.Services.AddHttpClient<SetlistFmApiClient>();
builder.Services.AddScoped<PlaylistPreviewService>();

builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

SpotifyApiSettings spotifySettings = builder.Configuration.GetSection("Spotify").Get<SpotifyApiSettings>() ?? new SpotifyApiSettings();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddOAuth("Spotify", options =>
    {
        // OAuthOptions.Validate() rejects an empty ClientId/ClientSecret, and that validation runs for
        // every request (not just /auth/login) once the scheme is registered. Fall back to a placeholder
        // so the rest of the app still renders when Spotify credentials haven't been configured yet;
        // an actual login attempt will simply fail against Spotify's API instead of crashing every page.
        options.ClientId = string.IsNullOrEmpty(spotifySettings.ClientId) ? "not-configured" : spotifySettings.ClientId;
        options.ClientSecret = string.IsNullOrEmpty(spotifySettings.ClientSecret) ? "not-configured" : spotifySettings.ClientSecret;
        options.CallbackPath = string.IsNullOrEmpty(spotifySettings.RedirectUri)
            ? "/signin-spotify"
            : new Uri(spotifySettings.RedirectUri).AbsolutePath;

        options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
        options.TokenEndpoint = "https://accounts.spotify.com/api/token";
        options.SaveTokens = true;
        options.Scope.Add("playlist-modify-public");
        options.Scope.Add("playlist-modify-private");
        options.Scope.Add("user-read-private");

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = context =>
            {
                if (context.AccessToken is not null)
                {
                    context.Identity?.AddClaim(new Claim(SpotifyAccessToken.ClaimType, context.AccessToken));
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

WebApplication app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapSpotifyAuthEndpoints();
app.MapHealthChecks("/health");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
