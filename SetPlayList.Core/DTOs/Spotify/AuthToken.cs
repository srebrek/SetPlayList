namespace SetPlayList.Core.DTOs.Spotify;
public record AuthToken(
    string AccessToken, 
    string TokenType, 
    string Scope, 
    int ExpiresIn, 
    string RefreshToken);
