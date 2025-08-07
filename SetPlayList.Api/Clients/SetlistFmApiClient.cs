using Microsoft.Extensions.Options;
using SetPlayList.Api.Configuration;
using SetPlayList.Core.DTOs.SetlistFm;
using SetPlayList.Core.Interfaces;
using System.Net;
using System.Text.Json;

namespace SetPlayList.Api.Clients;

public class SetlistFmApiClient : ISetlistFmApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SetlistFmApiSettings _settings;
    private readonly ILogger<SetlistFmApiClient> _logger;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);


    public SetlistFmApiClient(HttpClient httpClient, IOptions<SetlistFmApiSettings> settings, ILogger<SetlistFmApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Setlist?> GetSetlistAsync(string setlistId)
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
                    return null;
                }
                else
                {
                    _logger.LogError("Failed to retrieve the setlist. Status: {StatusCode}. Response: {ErrorResponse}",
                        response.StatusCode, responseContent);
                    return null;
                }
            }

            try
            {
                var setlistResponse = await response.Content.ReadFromJsonAsync<Setlist>(_jsonSerializerOptions);
                _logger.Log(LogLevel.Information, "Successfully retrieved setlist with ID: {setlistId}", setlistId);
                return setlistResponse;
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(ex, "Failed to deserialize the setlist response from SetlistFm. Response content: {ResponseContent}", responseContent);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "A network error occurred while trying to retrieve setlist.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            return null;
        }
    }
}

