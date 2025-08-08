using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using SetPlayList.Api.Services;
using SetPlayList.Api.Tests.UnitTests.Clients;
using SetPlayList.Api.Tests.UnitTests.Utilities;
using SetPlayList.Core.DTOs.Spotify;
using SetPlayList.Core.Interfaces;
using System.Net;

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
    public async Task HandleAuthorizationCallbackAsync_ValidStateAndApiClientSucceeds_ReturnsTrueAndSetsTokenCookie()
    {
        // Arrange
        var state = "valid_state_123";
        var code = "valid_code";
        var testAccessToken = "test_access_token";
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers.Cookie = new CookieHeaderValue("spotify_auth_state", state).ToString();

        var token = new AuthToken(testAccessToken, "default-scope", "Bearer", 3600, "refresh");
        _apiClientMock
            .Setup(c => c.ExchangeCodeForTokenAsync(code))
            .ReturnsAsync((token, HttpStatusCode.OK));

        // Act
        var result = await _sut.HandleAuthorizationCallbackAsync(httpContext, code, state);

        // Assert
        Assert.True(result);

        var setCookieHeader = httpContext.Response.Headers[HeaderNames.SetCookie].ToString();
        Assert.Contains($"spotify_token={testAccessToken}", setCookieHeader);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Information, "Successfully handled Spotify authorization callback");
    }

    [Fact]
    public async Task HandleAuthorizationCallbackAsync_InvalidState_ReturnsFalse()
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

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Warning, "state mismatch or missing");
    }

    [Fact]
    public async Task HandleAuthorizationCallbackAsync_ApiClientFails_ReturnsFalse()
    {
        // Arrange
        var state = "valid_state";
        var code = "code_that_will_fail";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = new CookieHeaderValue("spotify_auth_state", state).ToString();
        _apiClientMock
            .Setup(c => c.ExchangeCodeForTokenAsync(code))
            .ReturnsAsync((null, HttpStatusCode.BadGateway));

        // Act
        var result = await _sut.HandleAuthorizationCallbackAsync(httpContext, code, state);

        // Assert
        Assert.False(result);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "a token could not be obtained");
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_TokenCookieExists_DeletesCookie()
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

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Information, "User is logging out");
    }

    [Fact]
    public async Task LogoutAsync_TokenCookieDoesNotExist_DoesNothing()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        await _sut.LogoutAsync(httpContext);

        // Assert
        var setCookieHeader = httpContext.Response.Headers[HeaderNames.SetCookie].ToString();
        Assert.Empty(setCookieHeader);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Warning, "no authentication cookie was found");
    }

    #endregion
}
