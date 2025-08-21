using SetPlayList.Api.Clients;
using SetPlayList.Api.Configuration;
using SetPlayList.Api.Services;
using SetPlayList.Core.Interfaces;
using SetPlayList.Api.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpotifyApiSettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.Configure<SetlistFmApiSettings>(builder.Configuration.GetSection("SetlistFm"));
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();
builder.Services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
builder.Services.AddScoped<ISetlistFmApiClient, SetlistFmApiClient>();
builder.Services.AddScoped<ISpotifyPlaylistService, SpotifyPlaylistService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

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

    //return success ? Results.Ok("Token saved.") : Results.Problem("Failed to exchange code for token.");
    return success ? Results.Redirect("/setplaylist") : Results.Problem("Failed to exchange code for token.");
});

app.MapGet("api/setlist/{setlistId}", async (ISetlistFmApiClient setlistFmApiClient, string setlistId) =>
{
    var (setlist, httpStatusCode) = await setlistFmApiClient.GetSetlistAsync(setlistId);

    return Results.Ok(setlist);
});

app.MapGet("api/playlist-preview/{setlistId}", async (ISpotifyPlaylistService spotifyPlaylistService, ISpotifyAuthService spotifyAuthService, string setlistId, HttpContext httpContext) =>
{
    var accessToken = await spotifyAuthService.GetCurrentAccessTokenAsync(httpContext);
    var result = await spotifyPlaylistService.GeneratePreviewAsync(setlistId, accessToken);
    return result.ProposedPlaylist;
});

app.MapRazorComponents<App>();

app.Run();
