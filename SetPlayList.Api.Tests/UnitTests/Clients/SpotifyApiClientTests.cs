using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using SetPlayList.Api.Clients;
using SetPlayList.Api.Configuration;
using SetPlayList.Api.Tests.UnitTests.Utilities;
using SetPlayList.Core.DTOs.Spotify;
using System.Net;
using System.Text.Json;

namespace SetPlayList.Api.Tests.UnitTests.Clients;

public class SpotifyApiClientTests
{
    private readonly Mock<IOptions<SpotifyApiSettings>> _settingsMock;
    private readonly Mock<ILogger<SpotifyApiClient>> _loggerMock;
    private readonly MockHttpMessageHandler _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly SpotifyApiClient _sut;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public SpotifyApiClientTests()
    {
        _settingsMock = new Mock<IOptions<SpotifyApiSettings>>();
        _ = _settingsMock.Setup(s => s.Value).Returns(new SpotifyApiSettings
        {
            ClientId = "test_client_id",
            ClientSecret = "test_client_secret",
            RedirectUri = "https://localhost/callback"
        });

        _loggerMock = new Mock<ILogger<SpotifyApiClient>>();
        _httpMessageHandlerMock = new MockHttpMessageHandler();
        _httpClient = _httpMessageHandlerMock.ToHttpClient();
        _sut = new SpotifyApiClient(_httpClient, _settingsMock.Object, _loggerMock.Object);
    }

    #region GetAuthorizationUrl Tests

    [Fact]
    public void GetAuthorizationUrl_ValidState_ReturnsCorrectlyFormattedUrl()
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
    public async Task ExchangeCodeForTokenAsync_ValidCode_ReturnsTokenResponse()
    {
        // Arrange
        var expectedToken = "BQD_test_token";
        var validCode = "valid_auth_code";
        var responseDto = new AuthToken(
            expectedToken,
            "Bearer",
            "playlist-modify-public playlist-modify-private user-read-private",
            3600,
            "xyz_refresh_token");
        var responseJson = JsonSerializer.Serialize(responseDto, _jsonSerializerOptions);

        _ = _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.OK, "application/json", responseJson);

        // Act
        var (authToken, httpStatusCode) = await _sut.ExchangeCodeForTokenAsync(validCode);

        // Assert
        Assert.NotNull(authToken);
        Assert.Equal(expectedToken, authToken.AccessToken);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Information, "Successfully exchanged authorization code");
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_InvalidCode_ReturnsNullAnd502r()
    {
        // Arrange
        var invalidCode = "invalid_auth_code";
        var errorResponseJson = "{\"error\":\"invalid_grant\",\"error_description\":\"Invalid authorization code\"}";

        _ = _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.BadRequest, "application/json", errorResponseJson);

        // Act
        var (authToken, httpStatusCode) = await _sut.ExchangeCodeForTokenAsync(invalidCode);

        // Assert
        Assert.Null(authToken);
        Assert.Equal(HttpStatusCode.BadGateway, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "Failed to exchange authorization code for token");
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_ApiResponseIsInvalidJson_ReturnsNullAnd502()
    {
        // Arrange
        var invalidJson = "this is not valid json {";

        _ = _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.OK, "application/json", invalidJson);

        // Act
        var (authToken, httpStatusCode) = await _sut.ExchangeCodeForTokenAsync("some_code");

        // Assert
        Assert.Null(authToken);
        Assert.Equal(HttpStatusCode.BadGateway, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "Failed to deserialize the token response", typeof(JsonException));
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_ApiResponseIsMissingToken_ShouldReturnNullAnd502()
    {
        // Arrange
        var incompleteJson = "{\"token_type\":\"Bearer\",\"expires_in\":3600}";

        _ = _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Respond(HttpStatusCode.OK, "application/json", incompleteJson);

        // Act
        var (authToken, httpStatusCode) = await _sut.ExchangeCodeForTokenAsync("some_code");

        // Assert
        Assert.Null(authToken);
        Assert.Equal(HttpStatusCode.BadGateway, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "response body was empty or did not contain an access token");
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_NetworkErrorOccurs_ReturnsNullAnd502()
    {
        // Arrange
        _ = _httpMessageHandlerMock
            .When(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            .Throw(new HttpRequestException("Simulated network failure."));

        // Act
        var (authToken, httpStatusCode) = await _sut.ExchangeCodeForTokenAsync("any_code");

        // Assert
        Assert.Null(authToken);
        Assert.Equal(HttpStatusCode.BadGateway, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "A network error occurred", typeof(HttpRequestException));
    }

    #endregion
}
