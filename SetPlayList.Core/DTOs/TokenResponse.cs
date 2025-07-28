namespace SetPlayList.Core.DTOs;
public record TokenResponse(string AccessToken, string TokenType, string scopes, int ExpiresIn, string RefreshToken);
