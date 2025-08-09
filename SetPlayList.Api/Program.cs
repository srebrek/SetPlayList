using SetPlayList.Api.Clients;
using SetPlayList.Api.Configuration;
using SetPlayList.Api.Services;
using SetPlayList.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpotifyApiSettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.Configure<SetlistFmApiSettings>(builder.Configuration.GetSection("SetlistFm"));
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();
builder.Services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
builder.Services.AddScoped<ISetlistFmApiClient, SetlistFmApiClient>();

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

    return success ? Results.Ok("Token saved.") : Results.Problem("Failed to exchange code for token.");
});

app.MapGet("/setlist/{setlistId}", async (ISetlistFmApiClient setlistFmApiClient, string setlistId) =>
{
    var setlist = await setlistFmApiClient.GetSetlistAsync(setlistId);

    return Results.Ok(setlist);
});

app.Run();
