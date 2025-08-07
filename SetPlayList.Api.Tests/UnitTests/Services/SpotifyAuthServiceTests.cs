using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using SetPlayList.Api.Services;
using SetPlayList.Api.Tests.UnitTests.Clients;
using SetPlayList.Core.DTOs.Spotify;
using SetPlayList.Core.Interfaces;

namespace SetPlayList.Api.Tests.UnitTests.Services;

public class SpotifyAuthServiceTests
{
    private readonly Mock<ISpotifyApiClient> _apiClientMock;
    private readonly Mock<ILogger<SpotifyAuthService>> _loggerMock;
    private readonly SpotifyAuthService _sut;

    public SpotifyAuthServiceTests()
    {
        _apiClientMock = new Mock<ISpotifyApiClient>();
        _loggerMock = new Mock<ILogger<SpotifyAuthService>>();
        _sut = new SpotifyAuthService(_apiClientMock.Object, _loggerMock.Object);
    }

    #region GetAuthorizationUrl Tests

    [Fact]
    public void GetAuthorizationUrl_ShouldSetStateCookieAndReturnUrlFromApiClient()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var expectedUrl = "https://spotify.com/auth/123";

        _apiClientMock
            .Setup(c => c.GetAuthorizationUrl(It.IsAny<string>()))
            .Returns(expectedUrl);

        // Act
        var resultUrl = _sut.GetAuthorizationUrl(httpContext);

        // Assert
        Assert.Equal(expectedUrl, resultUrl);
        var setCookieHeader = httpContext.Response.Headers[HeaderNames.SetCookie].ToString();
        Assert.NotNull(setCookieHeader);
        Assert.Contains("spotify_auth_state=", setCookieHeader);
        Assert.Contains("httponly", setCookieHeader);
        Assert.Contains("secure", setCookieHeader);
    }

    #endregion

    #region HandleAuthorizationCallbackAsync Tests

    [Fact]
    public async Task HandleAuthorizationCallbackAsync_WhenStateIsValidAndApiClientSucceeds_ShouldReturnTrueAndSetTokenCookie()
    {
        // Arrange
        var state = "valid_state_123";
        var code = "valid_code";
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers.Cookie = new CookieHeaderValue("spotify_auth_state", state).ToString();

        var tokenResponse = new TokenResponse("test_access_token", "default-scope", "Bearer", 3600, "refresh");
        _apiClientMock
            .Setup(c => c.ExchangeCodeForTokenAsync(code))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _sut.HandleAuthorizationCallbackAsync(httpContext, code, state);

        // Assert
        Assert.True(result);

        var setCookieHeader = httpContext.Response.Headers[HeaderNames.SetCookie].ToString();
        Assert.Contains("spotify_token=test_access_token", setCookieHeader);

        _loggerMock.VerifyLog(LogLevel.Information, "Successfully handled Spotify authorization callback");
    }

    [Fact]
    public async Task HandleAuthorizationCallbackAsync_WhenStateIsInvalid_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var providedState = "invalid_state";
        var expectedState = "correct_state";
        var code = "any_code";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = new CookieHeaderValue("spotify_auth_state", expectedState).ToString();

        // Act
        var result = await _sut.HandleAuthorizationCallbackAsync(httpContext, code, providedState);

        // Assert
        Assert.False(result);
        _loggerMock.VerifyLog(LogLevel.Warning, "state mismatch or missing");
    }

    [Fact]
    public async Task HandleAuthorizationCallbackAsync_WhenApiClientFails_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var state = "valid_state";
        var code = "code_that_will_fail";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = new CookieHeaderValue("spotify_auth_state", state).ToString();

        // Konfigurujemy mocka, aby symulował błąd
        _apiClientMock
            .Setup(c => c.ExchangeCodeForTokenAsync(code))
            .ReturnsAsync((TokenResponse?)null);

        // Act
        var result = await _sut.HandleAuthorizationCallbackAsync(httpContext, code, state);

        // Assert
        Assert.False(result);
        _loggerMock.VerifyLog(LogLevel.Error, "a token could not be obtained");
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_WhenTokenCookieExists_ShouldDeleteCookieAndLogInformation()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = new CookieHeaderValue("spotify_token", "any_token").ToString();

        // Act
        await _sut.LogoutAsync(httpContext);

        // Assert
        var setCookieHeader = httpContext.Response.Headers[HeaderNames.SetCookie].ToString();
        Assert.Contains("spotify_token=;", setCookieHeader);
        Assert.Contains("expires=", setCookieHeader);

        _loggerMock.VerifyLog(LogLevel.Information, "User is logging out");
    }

    [Fact]
    public async Task LogoutAsync_WhenTokenCookieDoesNotExist_ShouldDoNothingAndLogWarning()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        await _sut.LogoutAsync(httpContext);

        // Assert
        var setCookieHeader = httpContext.Response.Headers[HeaderNames.SetCookie].ToString();
        Assert.Empty(setCookieHeader);

        _loggerMock.VerifyLog(LogLevel.Warning, "no authentication cookie was found");
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