using Moq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;
using SetPlayList.Api.Configuration;
using SetPlayList.Api.Clients;
using SetPlayList.Core.DTOs;

namespace SetPlayList.Api.Tests.UnitTests.Clients;

public class SpotifyApiClientTests
{
    private readonly Mock<IOptions<SpotifyApiSettings>> _settingsMock;
    private readonly Mock<ILogger<SpotifyApiClient>> _loggerMock;
    private readonly MockHttpMessageHandler _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly SpotifyApiClient _sut;

    public SpotifyApiClientTests()
    {
        _settingsMock = new Mock<IOptions<SpotifyApiSettings>>();
        _loggerMock = new Mock<ILogger<SpotifyApiClient>>();

        _settingsMock.Setup(s => s.Value).Returns(new SpotifyApiSettings
        {
            ClientId = "test_client_id",
            ClientSecret = "test_client_secret",
            RedirectUri = "https://localhost/callback"
        });

        _httpMessageHandlerMock = new MockHttpMessageHandler();
        _httpClient = _httpMessageHandlerMock.ToHttpClient();

        _sut = new SpotifyApiClient(_httpClient, _settingsMock.Object, _loggerMock.Object);
    }

    #region GetAuthorizationUrl Tests

    [Fact]
    public void GetAuthorizationUrl_GivenValidState_ShouldReturnCorrectlyFormattedUrl()
    {
        // Arrange
        var state = "my-unique-state-123";
        var expectedClientId = "test_client_id";
        var expectedRedirectUri = "https://localhost/callback";

        // Act
        var resultUrl = _sut.GetAuthorizationUrl(state);

        // Assert
        Assert.NotNull(resultUrl);
        Assert.StartsWith("https://accounts.spotify.com/authorize", resultUrl);
        Assert.Contains($"client_id={expectedClientId}", resultUrl);
        Assert.Contains($"redirect_uri={Uri.EscapeDataString(expectedRedirectUri)}", resultUrl);
        Assert.Contains($"state={state}", resultUrl);
        Assert.Contains("scope=", resultUrl);
    }

    #endregion

    #region ExchangeCodeForTokenAsync Tests

    [Fact]
    public async Task ExchangeCodeForTokenAsync_WhenApiReturnsSuccess_ShouldReturnTokenResponse()
    {
        // Arrange
        var expectedToken = "BQD_test_token";
        var validCode = "valid_auth_code";
        var responseDto = new TokenResponse(
            expectedToken, 
            "Bearer", 
            "playlist-modify-public playlist-modify-private user-read-private", 
            3600, 
            "xyz_refresh_token");
        var responseJson = JsonSerializer.Serialize(responseDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.OK, "application/json", responseJson);

        // Act
        var result = await _sut.ExchangeCodeForTokenAsync(validCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.AccessToken);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Information, "Successfully exchanged authorization code");
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_WhenApiReturnsNonSuccessStatusCode_ShouldReturnNullAndLogError()
    {
        // Arrange
        var invalidCode = "invalid_auth_code";
        var errorResponseJson = "{\"error\":\"invalid_grant\",\"error_description\":\"Invalid authorization code\"}";

        _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.BadRequest, "application/json", errorResponseJson);

        // Act
        var result = await _sut.ExchangeCodeForTokenAsync(invalidCode);

        // Assert
        Assert.Null(result);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "Failed to exchange authorization code for token");
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_WhenApiResponseIsInvalidJson_ShouldReturnNullAndLogJsonException()
    {
        // Arrange
        var invalidJson = "this is not valid json {";

        _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.OK, "application/json", invalidJson);

        // Act
        var result = await _sut.ExchangeCodeForTokenAsync("some_code");

        // Assert
        Assert.Null(result);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "Failed to deserialize the token response", typeof(JsonException));
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_WhenApiResponseIsMissingToken_ShouldReturnNullAndLogWarning()
    {
        // Arrange
        var incompleteJson = "{\"token_type\":\"Bearer\",\"expires_in\":3600}";

        _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.OK, "application/json", incompleteJson);

        // Act
        var result = await _sut.ExchangeCodeForTokenAsync("some_code");

        // Assert
        Assert.Null(result);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Warning, "response body was empty or did not contain an access token");
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_WhenNetworkErrorOccurs_ShouldReturnNullAndLogHttpRequestException()
    {
        // Arrange
        _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Throw(new HttpRequestException("Simulated network failure."));

        // Act
        var result = await _sut.ExchangeCodeForTokenAsync("any_code");

        // Assert
        Assert.Null(result);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "A network error occurred", typeof(HttpRequestException));
    }

    #endregion
}

public static class LoggerTestExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel expectedLevel, string expectedMessageSubstring, Type? expectedExceptionType = null)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == expectedLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessageSubstring)),
                It.Is<Exception>((ex, t) => ex == null || ex.GetType() == expectedExceptionType),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}