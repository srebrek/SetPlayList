using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;
using SetPlayList.Components;
using SetPlayList.Features.PlaylistPreview;
using SetPlayList.Integrations.Spotify.Auth;
using SetPlayList.Integrations.SetlistFm;
using SetPlayList.Integrations.Spotify;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSpotifyIntegration(builder.Configuration);
builder.Services.AddSetlistFmIntegration(builder.Configuration);
builder.Services.AddScoped<PlaylistPreviewService>();

builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddSpotifyAuthentication();
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
