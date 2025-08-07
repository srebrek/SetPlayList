namespace SetPlayList.Core.DTOs.Spotify;
public record TokenResponse(
    string AccessToken, 
    string TokenType, 
    string Scope, 
    int ExpiresIn, 
    string RefreshToken);
