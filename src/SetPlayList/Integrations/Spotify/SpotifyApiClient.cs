using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using SetPlayList.Common;

namespace SetPlayList.Integrations.Spotify;

internal sealed partial class SpotifyApiClient(HttpClient httpClient, ILogger<SpotifyApiClient> logger)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<Result<List<Track>>> SearchTopTracksAsync(
        string artistName,
        string trackName,
        int limit,
        string accessToken,
        CancellationToken ct)
    {
        string query = $"""
            artist:"{artistName}" track:"{trackName}"
            """;

        string url = QueryHelpers.AddQueryString("https://api.spotify.com/v1/search", new Dictionary<string, string?>
        {
            ["q"] = query,
            ["type"] = "track",
            ["limit"] = limit.ToString(CultureInfo.InvariantCulture),
        });

        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Authorization = new("Bearer", accessToken);

        Result<SearchResponse> result = await SendAsync<SearchResponse>(request, "search tracks", ct);
        return result.IsSuccess ? result.Value.Tracks.Items : result.Error;
    }

    public async Task<Result<string>> CreatePlaylistAsync(
        string userId,
        string playlistName,
        string accessToken,
        CancellationToken ct)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"https://api.spotify.com/v1/users/{userId}/playlists");
        request.Headers.Authorization = new("Bearer", accessToken);
        request.Content = JsonContent.Create(new { name = playlistName }, options: s_jsonOptions);

        Result<CreatePlaylistResponse> result = await SendAsync<CreatePlaylistResponse>(request, "create playlist", ct);
        return result.IsSuccess ? result.Value.Id : result.Error;
    }

    public async Task<Result> AddTracksToPlaylistAsync(
        string playlistId,
        List<string> trackIds,
        string accessToken,
        CancellationToken ct)
    {
        List<string> uris = [.. trackIds.Select(id => $"spotify:track:{id}")];
        if (uris.Count is 0)
        {
            return Error.Validation("No tracks selected.");
        }

        using HttpRequestMessage request =
            new(HttpMethod.Post, $"https://api.spotify.com/v1/playlists/{playlistId}/tracks");
        request.Headers.Authorization = new("Bearer", accessToken);
        request.Content = JsonContent.Create(new { uris }, options: s_jsonOptions);

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(ct);
            LogRequestFailed(logger, "add tracks to playlist", response.StatusCode, errorContent);
            return response.StatusCode is HttpStatusCode.Forbidden
                ? Error.Unauthorized()
                : Error.Failure("Failed to add tracks to the playlist on Spotify.");
        }

        return Result.Success();
    }

    private async Task<Result<T>> SendAsync<T>(HttpRequestMessage request, string action, CancellationToken cancellationToken)
        where T : notnull
    {
        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                LogRequestFailed(logger, action, response.StatusCode, content);
                if (response.StatusCode is HttpStatusCode.Forbidden)
                {
                    return Error.Unauthorized();
                }

                return Error.Failure($"Spotify request failed while trying to {action}.");
            }

            T? value = JsonSerializer.Deserialize<T>(content, s_jsonOptions);
            if (value is null)
            {
                LogEmptyResponse(logger, action, content);
                return Error.Failure($"Spotify returned an empty response while trying to {action}.");
            }

            return value;
        }
        catch (HttpRequestException ex)
        {
            LogNetworkError(logger, action, ex);
            return Error.Failure($"Network error while trying to {action}.");
        }
        catch (JsonException ex)
        {
            LogDeserializationError(logger, action, ex);
            return Error.Failure($"Failed to read the Spotify response for {action}.");
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to {Action}. Status: {StatusCode}. Response: {Response}")]
    private static partial void LogRequestFailed(ILogger logger, string action, HttpStatusCode statusCode, string response);

    [LoggerMessage(
        Level = LogLevel.Error, Message = "Spotify response for {Action} deserialized to null. Response: {Response}")]
    private static partial void LogEmptyResponse(ILogger logger, string action, string response);

    [LoggerMessage(Level = LogLevel.Error, Message = "Network error while trying to {Action}.")]
    private static partial void LogNetworkError(ILogger logger, string action, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize the response for {Action}.")]
    private static partial void LogDeserializationError(ILogger logger, string action, Exception exception);
}
