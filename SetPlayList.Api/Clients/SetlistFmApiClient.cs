using Microsoft.Extensions.Options;
using SetPlayList.Api.Configuration;
using SetPlayList.Core.DTOs.SetlistFm;
using SetPlayList.Core.Interfaces;
using System.Net;
using System.Text.Json;

namespace SetPlayList.Api.Clients;

public class SetlistFmApiClient(
    HttpClient httpClient,
    IOptions<SetlistFmApiSettings> settings,
    ILogger<SetlistFmApiClient> logger)
    : ISetlistFmApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly SetlistFmApiSettings _settings = settings.Value;
    private readonly ILogger<SetlistFmApiClient> _logger = logger;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<(Setlist? setlist, HttpStatusCode httpStatusCode)> GetSetlistAsync(string setlistId)
    {
        var setlistRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.setlist.fm/rest/1.0/setlist/" + setlistId);
        setlistRequest.Headers.Add("Accept", "application/json");
        setlistRequest.Headers.Add("x-api-key", _settings.ClientSecret);

        try
        {
            var response = await _httpClient.SendAsync(setlistRequest);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Failed to retrieve the setlist. Status: {StatusCode}. Response: {ErrorResponse}",
                    response.StatusCode, responseContent);
                    return (null, HttpStatusCode.NotFound);
                }
                else
                {
                    _logger.LogError("Failed to retrieve the setlist. Status: {StatusCode}. Response: {ErrorResponse}",
                        response.StatusCode, responseContent);
                    return (null, HttpStatusCode.BadGateway);
                }
            }

            try
            {
                var setlist = await response.Content.ReadFromJsonAsync<Setlist>(_jsonSerializerOptions);
                _logger.Log(LogLevel.Information, "Successfully retrieved setlist with ID: {setlistId}", setlistId);
                return (setlist, HttpStatusCode.OK);
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(ex, "Failed to deserialize the setlist response from SetlistFm. Response content: {ResponseContent}", responseContent);
                return (null, HttpStatusCode.BadGateway);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "A network error occurred while trying to retrieve setlist.");
            return (null, HttpStatusCode.BadGateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            return (null, HttpStatusCode.InternalServerError);
        }
    }
}

