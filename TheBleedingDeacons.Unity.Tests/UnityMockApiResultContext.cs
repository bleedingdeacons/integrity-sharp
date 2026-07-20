using System.Text.Json;
using NaturalApi;
using NaturalApi.Reporter;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// Mock API result context that properly deserializes UnityRestSharp's ApiResponse wrapper
/// while supporting NaturalApi's fluent ShouldReturn assertions.
/// </summary>
internal sealed class UnityMockApiResultContext : IApiResultContext
{
    public HttpResponseMessage Response { get; }

    public int StatusCode { get; }

    public IDictionary<string, string> Headers { get; }

    public string RawBody { get; }

    public long Duration { get; set; }

    private readonly IHttpExecutor _httpExecutor;

    public UnityMockApiResultContext(
        HttpResponseMessage response,
        string responseBody,
        IDictionary<string, string> headers,
        IHttpExecutor httpExecutor)
    {
        Response = response;
        StatusCode = (int)response.StatusCode;
        Headers = headers;
        RawBody = responseBody;
        _httpExecutor = httpExecutor;
        Duration = 0;
    }

    public T BodyAs<T>()
    {
        if (typeof(T) == typeof(string))
            return (T)(object)RawBody;

        try
        {
            return JsonSerializer.Deserialize<T>(RawBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            }) ?? default!;
        }
        catch
        {
            return default!;
        }
    }

    private ApiAssertionException CreateAssertionException(string message, string expectation, string actual)
    {
        var endpoint = "mock-endpoint";
        var verb = "GET";
        return new ApiAssertionException(message, expectation, actual, endpoint, verb, RawBody);
    }

    public IApiResultContext ShouldReturn<T>(
        int? status = null,
        Func<T, bool>? bodyValidator = null,
        Func<IDictionary<string, string>, bool>? headers = null)
    {
        if (status.HasValue && StatusCode != status.Value)
            throw CreateAssertionException(
                $"Expected status {status.Value} but got {StatusCode}",
                $"Status {status.Value}",
                $"Status {StatusCode}");

        if (bodyValidator != null)
        {
            var body = BodyAs<T>();
            if (!bodyValidator(body))
                throw CreateAssertionException(
                    $"Body validation failed for type {typeof(T).Name}",
                    $"Body of type {typeof(T).Name} passing validator",
                    "Validator returned false");
        }

        if (headers != null && !headers(Headers))
            throw CreateAssertionException(
                "Header validation failed",
                "Headers passing validator",
                "Validator returned false");

        return this;
    }

    public IApiResultContext ShouldReturn(int status)
    {
        if (StatusCode != status)
            throw CreateAssertionException(
                $"Expected status {status} but got {StatusCode}",
                $"Status {status}",
                $"Status {StatusCode}");
        return this;
    }

    public IApiResultContext ShouldReturn<T>(Func<T, bool> bodyValidator)
    {
        var body = BodyAs<T>();
        if (!bodyValidator(body))
            throw CreateAssertionException(
                $"Body validation failed for type {typeof(T).Name}",
                $"Body of type {typeof(T).Name} passing validator",
                "Validator returned false");
        return this;
    }

    public IApiResultContext ShouldReturn(int status, Func<IDictionary<string, string>, bool> headers)
    {
        if (StatusCode != status)
            throw CreateAssertionException(
                $"Expected status {status} but got {StatusCode}",
                $"Status {status}",
                $"Status {StatusCode}");
        if (!headers(Headers))
            throw CreateAssertionException(
                "Header validation failed",
                "Headers passing validator",
                "Validator returned false");
        return this;
    }

    public T ShouldReturn<T>() => BodyAs<T>();

    public IApiResultContext Then(Action<IApiResult> next)
    {
        // Use fully-qualified name to resolve ambiguity with TheBleedingDeacons.Unity.Client.ApiResponse<T>
        var result = new NaturalApi.ApiResponse<object>(this, _httpExecutor);
        next?.Invoke(result);
        return this;
    }

    public string? GetCookie(string name) => null;
}
