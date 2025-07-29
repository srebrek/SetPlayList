namespace SetPlayList.Core.DTOs;
public record TokenResponse(
    string AccessToken, 
    string TokenType, 
    string Scope, 
    int ExpiresIn, 
    string RefreshToken);
