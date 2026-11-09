using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Services;

public interface IApiClient
{
    Task<ApiResult<T>> GetAsync<T>(string url);
    Task<ApiResult<T>> PostAsync<T>(string url, object? body = null);
    Task<ApiResult<T>> PutAsync<T>(string url, object body);
    Task<ApiResult> DeleteAsync(string url);
 Task<ApiResult> PostNoContentAsync(string url, object? body = null);

    
    Task<(ApiResult<T> Result, string? ApiCookie)> LoginAsync<T>(object body);
}

    public class ApiClient : IApiClient
{
    private const string UnavailableMessage = "RaceDay is not available right now. Please try again in a moment.";
    private const string GenericMessage = "Something went wrong. Please try again.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        SendAsync<T>(HttpMethod.Get, url, null);

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null) =>
        SendAsync<T>(HttpMethod.Post, url, body);

    public Task<ApiResult<T>> PutAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Put, url, body);

    public async Task<ApiResult> DeleteAsync(string url) =>
        await SendAsync<object>(HttpMethod.Delete, url, null);

    public async Task<ApiResult> PostNoContentAsync(string url, object? body = null) =>
        await SendAsync<object>(HttpMethod.Post, url, body);

    public async Task<(ApiResult<T> Result, string? ApiCookie)> LoginAsync<T>(object body)
    {
        string? apiCookie = null;

        var result = await SendAsync<T>(HttpMethod.Post, "api/auth/login", body, response =>
        {
            if (response.IsSuccessStatusCode &&
                response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                apiCookie = ExtractCookie(values, ApiCookieHandler.ApiCookieName);
            }
        });

        return (result, apiCookie);
    }

    private async Task<ApiResult<T>> SendAsync<T>(
        HttpMethod method, string url, object? body, Action<HttpResponseMessage>? inspect = null)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (body != null)
                request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);

            using var response = await _http.SendAsync(request);
            inspect?.Invoke(response);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return new ApiResult<T> { Success = true, StatusCode = response.StatusCode };

                var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                return new ApiResult<T> { Success = true, StatusCode = response.StatusCode, Data = data };
            }

            return Failure<T>(response.StatusCode, await ReadErrorAsync(response));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Could not reach the API for {Method} {Url}", method, url);
            return Failure<T>(HttpStatusCode.ServiceUnavailable, UnavailableMessage);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "The API sent a response that could not be read for {Method} {Url}", method, url);
            return Failure<T>(HttpStatusCode.InternalServerError, GenericMessage);
        }
    }

    private static ApiResult<T> Failure<T>(HttpStatusCode code, string message) =>
        new() { Success = false, StatusCode = code, ErrorMessage = message };

    
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    
                    if (root.TryGetProperty("message", out var message) &&
                        message.ValueKind == JsonValueKind.String)
                        return message.GetString()!;

                    
                    if (root.TryGetProperty("errors", out var errors) &&
                        errors.ValueKind == JsonValueKind.Object)
                    {
                        var messages = errors.EnumerateObject()
                            .Where(p => p.Value.ValueKind == JsonValueKind.Array)
                            .SelectMany(p => p.Value.EnumerateArray())
                            .Where(e => e.ValueKind == JsonValueKind.String)
                            .Select(e => e.GetString())
                            .Where(s => !string.IsNullOrWhiteSpace(s));

                        var joined = string.Join(" ", messages);
                        if (joined.Length > 0) return joined;
                    }
                }
            }
        }
        catch (JsonException)
        {
           
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Please log in to continue.",
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "We could not find what you were looking for.",
            _ => GenericMessage
        };
    }
private static string? ExtractCookie(IEnumerable<string> setCookieHeaders, string name)
    {
        var prefix = name + "=";
        foreach (var header in setCookieHeaders)
        {
            if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            var end = header.IndexOf(';');
            return end > 0 ? header[prefix.Length..end] : header[prefix.Length..];
        }
        return null;
    }
}