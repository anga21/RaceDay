using System.Net;

namespace RaceDay.Web.Services;

public class ApiResult
{
    public bool Success { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }
}