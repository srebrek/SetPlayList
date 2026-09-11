using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SetPlayList.Common;

namespace SetPlayList.Integrations.SetlistFm;

internal sealed partial class SetlistFmApiClient(HttpClient httpClient, IOptions<SetlistFmApiSettings> settings, ILogger<SetlistFmApiClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<Setlist>> GetSetlistAsync(string setlistId, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "https://api.setlist.fm/rest/1.0/setlist/" + setlistId);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("x-api-key", settings.Value.ClientSecret);

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    LogNotFound(logger, setlistId, content);
                    return Error.NotFound($"No setlist found for id '{setlistId}'.");
                }

                LogRequestFailed(logger, setlistId, response.StatusCode, content);
                return Error.Failure("Failed to retrieve the setlist from setlist.fm.");
            }

            Setlist? setlist = JsonSerializer.Deserialize<Setlist>(content, JsonOptions);
            if (setlist is null)
            {
                LogEmptyResponse(logger, setlistId, content);
                return Error.Failure("setlist.fm returned an empty response.");
            }

            return setlist;
        }
        catch (HttpRequestException ex)
        {
            LogNetworkError(logger, setlistId, ex);
            return Error.Failure("Network error while contacting setlist.fm.");
        }
        catch (JsonException ex)
        {
            LogDeserializationError(logger, setlistId, ex);
            return Error.Failure("Failed to read the setlist.fm response.");
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Setlist {SetlistId} was not found. Response: {Response}")]
    private static partial void LogNotFound(ILogger logger, string setlistId, string response);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve setlist {SetlistId}. Status: {StatusCode}. Response: {Response}")]
    private static partial void LogRequestFailed(ILogger logger, string setlistId, HttpStatusCode statusCode, string response);

    [LoggerMessage(Level = LogLevel.Error, Message = "Setlist {SetlistId} deserialized to null. Response: {Response}")]
    private static partial void LogEmptyResponse(ILogger logger, string setlistId, string response);

    [LoggerMessage(Level = LogLevel.Error, Message = "Network error while retrieving setlist {SetlistId}.")]
    private static partial void LogNetworkError(ILogger logger, string setlistId, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize setlist {SetlistId}.")]
    private static partial void LogDeserializationError(ILogger logger, string setlistId, Exception exception);
}
