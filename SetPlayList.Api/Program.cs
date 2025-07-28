using SetPlayList.Api.Configuration;
using SetPlayList.Api.Spotify;
using SetPlayList.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpotifyApiSettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();
builder.Services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();

var app = builder.Build();

app.MapGet("/auth/spotify/login", (HttpContext context, ISpotifyAuthService spotifyAuthService) =>
{
    var authUrl = spotifyAuthService.GetAuthorizationUrl(context);
    return Results.Redirect(authUrl);
});

app.MapGet("/auth/spotify/callback", async (HttpContext context, ISpotifyAuthService spotifyAuthService, string? code, string? state, string? error) =>
{
    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
    {
        return Results.BadRequest("Missing code or state.");
    }

    var success = await spotifyAuthService.HandleAuthorizationCallbackAsync(context, code, state);

    if (!success)
    {
        return Results.Problem("Failed to exchange code for token.");
    }

    return Results.Ok("Token saved.");
});

app.Run();
