namespace RaceDay.Web.Services;


public class ApiCookieHandler : DelegatingHandler
{
    public const string SessionKey = "ApiCookie";
    public const string ApiCookieName = "RaceDay.Session";

    private readonly IHttpContextAccessor _accessor;

    public ApiCookieHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookieValue = _accessor.HttpContext?.Session.GetString(SessionKey);
        if (!string.IsNullOrEmpty(cookieValue))
            request.Headers.TryAddWithoutValidation("Cookie", $"{ApiCookieName}={cookieValue}");

        return base.SendAsync(request, cancellationToken);
    }
}