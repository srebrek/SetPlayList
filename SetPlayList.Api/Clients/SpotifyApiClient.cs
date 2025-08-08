using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SetPlayList.Api.Configuration;
using SetPlayList.Core.DTOs.Spotify;
using SetPlayList.Core.Interfaces;
using System.Net;
using System.Text.Json;

namespace SetPlayList.Api.Clients;

public class SpotifyApiClient(
    HttpClient httpClient, 
    IOptions<SpotifyApiSettings> settings, 
    ILogger<SpotifyApiClient> logger) 
    : ISpotifyApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly SpotifyApiSettings _settings = settings.Value;
    private readonly ILogger<SpotifyApiClient> _logger = logger;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public string GetAuthorizationUrl(string state)
    {
        var scopes = "playlist-modify-public playlist-modify-private user-read-private";
        return "https://accounts.spotify.com/authorize" +
               $"?client_id={_settings.ClientId}" +
               "&response_type=code" +
               $"&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}" +
               $"&scope={Uri.EscapeDataString(scopes)}" +
               $"&state={state}";
    }

    public async Task<(AuthToken? authToken, HttpStatusCode httpStatusCode)> ExchangeCodeForTokenAsync(string code)
    {
        var authString = $"{_settings.ClientId}:{_settings.ClientSecret}";
        var authBytes = System.Text.Encoding.UTF8.GetBytes(authString);
        var base64AuthString = Convert.ToBase64String(authBytes);

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        tokenRequest.Headers.Authorization = new("Basic", base64AuthString);
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _settings.RedirectUri
        });

        try
        {
            var response = await _httpClient.SendAsync(tokenRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to exchange authorization code for token. Status: {StatusCode}. Response: {ErrorResponse}",
                    response.StatusCode, errorContent);
                return (null, HttpStatusCode.BadGateway);
            }

            try
            {
                var authToken = await response.Content.ReadFromJsonAsync<AuthToken>(_jsonSerializerOptions);
                if (authToken is null || string.IsNullOrWhiteSpace(authToken.AccessToken))
                {
                    _logger.LogError("Spotify API response for code exchange was successful (2xx), but the response body was empty or did not contain an access token.");
                    return (null, HttpStatusCode.BadGateway);
                }

                _logger.LogInformation("Successfully exchanged authorization code for an access token.");
                return (authToken, HttpStatusCode.OK);
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(ex, "Failed to deserialize the token response from Spotify. Response content: {ResponseContent}", responseContent);
                return (null, HttpStatusCode.BadGateway);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "A network error occurred while trying to exchange the authorization code for a token.");
            return (null, HttpStatusCode.BadGateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            return (null, HttpStatusCode.InternalServerError);
        }
    }
}
