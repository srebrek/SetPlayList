namespace SetPlayList.Api.Services;

public class UserStateService
{
    public string? AccessToken { get; set; }

    public UserStateService(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            // Sprawdzamy, czy nasze middleware zostawiło dla nas token w schowku.
            if (httpContext.Items.TryGetValue("AccessToken", out var token) && token is string accessToken)
            {
                // Jeśli tak, inicjalizujemy serwis.
                AccessToken = accessToken;
            }
        }
        else
        {
            AccessToken = "nullHC";
        }
    }
}

public class TokenCookieReaderMiddleware
{
    private readonly RequestDelegate _next;

    public TokenCookieReaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Sprawdzamy, czy to żądanie ma cookie z tokenem
        if (context.Request.Cookies.TryGetValue("spotify_token", out var tokenFromCookie) && !string.IsNullOrEmpty(tokenFromCookie))
        {
            // Zapisujemy token do tymczasowego schowka dla tego JEDNEGO żądania.
            // Klucz "AccessToken" jest dowolny, byle był spójny.
            context.Items["AccessToken"] = tokenFromCookie;
        }

        // Zawsze przekazujemy żądanie dalej
        await _next(context);
    }
}