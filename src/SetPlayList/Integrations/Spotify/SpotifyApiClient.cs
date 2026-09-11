using System.Net;
using System.Text.Json;
using SetPlayList.Common;
using SetPlayList.Integrations.Spotify.Dtos;

namespace SetPlayList.Integrations.Spotify;

internal sealed partial class SpotifyApiClient(HttpClient httpClient, ILogger<SpotifyApiClient> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<SpotifyApiClient> _logger = logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private const string ExtendedQuotaModeMessage =
        "Spotify now requires apps to be approved for Extended Quota Mode before they can create playlists on behalf of users. " +
        "This app is capped at Development Mode (25 allow-listed users). See the SetPlayList showcase at https://TODO-DOMAIN#setPlayList for a full walkthrough.";

    public async Task<Result<List<Track>>> SearchTopTracksAsync(string artistName, string trackName, int limit, string accessToken, CancellationToken cancellationToken)
    {
        string query = $"artist:\"{artistName}\" track:\"{trackName}\"";
        string url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit={limit}";

        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Authorization = new("Bearer", accessToken);

        Result<SearchResponse> result = await SendAsync<SearchResponse>(request, "search tracks", cancellationToken);
        return result.IsSuccess ? result.Value.Tracks.Items : result.Error!;
    }

    public async Task<Result<string>> GetCurrentUserIdAsync(string accessToken, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "https://api.spotify.com/v1/me");
        request.Headers.Authorization = new("Bearer", accessToken);

        Result<UserResponse> result = await SendAsync<UserResponse>(request, "get current user", cancellationToken);
        return result.IsSuccess ? result.Value.Id : result.Error!;
    }

    public async Task<Result<string>> CreatePlaylistAsync(string userId, string playlistName, string accessToken, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"https://api.spotify.com/v1/users/{userId}/playlists");
        request.Headers.Authorization = new("Bearer", accessToken);
        request.Content = JsonContent.Create(new { name = playlistName }, options: JsonOptions);

        Result<CreatePlaylistResponse> result = await SendAsync<CreatePlaylistResponse>(request, "create playlist", cancellationToken);
        return result.IsSuccess ? result.Value.Id : result.Error!;
    }

    public async Task<Result<bool>> AddTracksToPlaylistAsync(string playlistId, List<string> trackIds, string accessToken, CancellationToken cancellationToken)
    {
        List<string> uris = trackIds.Select(id => $"spotify:track:{id}").ToList();
        if (uris.Count == 0)
        {
            return Error.Validation("No tracks selected.");
        }

        using HttpRequestMessage request = new(HttpMethod.Post, $"https://api.spotify.com/v1/playlists/{playlistId}/tracks");
        request.Headers.Authorization = new("Bearer", accessToken);
        request.Content = JsonContent.Create(new { uris }, options: JsonOptions);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        LogRequestFailed(_logger, "add tracks to playlist", response.StatusCode, errorContent);
        return response.StatusCode == HttpStatusCode.Forbidden
            ? Error.Unauthorized(ExtendedQuotaModeMessage)
            : Error.Failure("Failed to add tracks to the playlist on Spotify.");
    }

    private async Task<Result<T>> SendAsync<T>(HttpRequestMessage request, string action, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                LogRequestFailed(_logger, action, response.StatusCode, content);
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return Error.Unauthorized(ExtendedQuotaModeMessage);
                }

                return Error.Failure($"Spotify request failed while trying to {action}.");
            }

            T? value = JsonSerializer.Deserialize<T>(content, JsonOptions);
            if (value is null)
            {
                LogEmptyResponse(_logger, action, content);
                return Error.Failure($"Spotify returned an empty response while trying to {action}.");
            }

            return value;
        }
        catch (HttpRequestException ex)
        {
            LogNetworkError(_logger, action, ex);
            return Error.Failure($"Network error while trying to {action}.");
        }
        catch (JsonException ex)
        {
            LogDeserializationError(_logger, action, ex);
            return Error.Failure($"Failed to read the Spotify response for {action}.");
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to {Action}. Status: {StatusCode}. Response: {Response}")]
    private static partial void LogRequestFailed(ILogger logger, string action, HttpStatusCode statusCode, string response);

    [LoggerMessage(Level = LogLevel.Error, Message = "Spotify response for {Action} deserialized to null. Response: {Response}")]
    private static partial void LogEmptyResponse(ILogger logger, string action, string response);

    [LoggerMessage(Level = LogLevel.Error, Message = "Network error while trying to {Action}.")]
    private static partial void LogNetworkError(ILogger logger, string action, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize the response for {Action}.")]
    private static partial void LogDeserializationError(ILogger logger, string action, Exception exception);
}
