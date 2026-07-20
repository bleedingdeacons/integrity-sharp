using System.Net;
using System.Text.Json;
using NaturalApi;
using NaturalApi.Reporter;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// A MockHttpExecutor tailored for testing UnityRestSharp endpoints via NaturalApi's fluent DSL.
/// Wraps mock API responses in NaturalApi's IApiResultContext so ShouldReturn() works naturally.
/// </summary>
internal sealed class UnityMockHttpExecutor : IHttpExecutor
{
    public ApiRequestSpec? LastSpec { get; private set; }

    private int _statusCode = 200;
    private string _responseBody = """{"success":true,"data":null}""";
    private IDictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.Ordinal);

    private INaturalReporter _reporter = new NullReporter();

    public INaturalReporter Reporter { get => _reporter; set => _reporter = value ?? new NullReporter(); }

    public void SetupResponse(int statusCode, object responseBody, IDictionary<string, string>? headers = null)
    {
        _statusCode = statusCode;
        _responseBody = responseBody is string s ? s : JsonSerializer.Serialize(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        _headers = headers ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public void SetupSuccessResponse<T>(T data, int total = 1, int page = 1, int perPage = 100) where T : class
    {
        var wrapped = new
        {
            success = true,
            data,
            meta = new { total, page, per_page = perPage, total_pages = (int)Math.Ceiling((double)total / perPage) }
        };
        SetupResponse(200, wrapped);
    }

    public void SetupErrorResponse(int statusCode, string code, string message)
    {
        var error = new { success = false, error = new { code, message } };
        SetupResponse(statusCode, error);
    }

    public IApiResultContext Execute(ApiRequestSpec spec)
    {
        LastSpec = spec;

        var response = new HttpResponseMessage((HttpStatusCode)_statusCode)
        {
            Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
        };

        foreach (var header in _headers)
            response.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return new UnityMockApiResultContext(response, _responseBody, _headers, this);
    }
}
