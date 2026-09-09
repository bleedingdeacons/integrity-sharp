using System.Net;
using System.Text.Json;
using TheBleedingDeacons.Unity.Client;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// A mock HttpMessageHandler that captures requests and returns configurable responses.
/// Used to intercept the HttpClient calls made by UnityRestSharp without hitting a real server.
/// </summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
	private readonly Dictionary<string, MockResponse> _responses = new(StringComparer.OrdinalIgnoreCase);
	private readonly List<HttpRequestMessage> _capturedRequests = [];
	private MockResponse _defaultResponse;

	public IReadOnlyList<HttpRequestMessage> CapturedRequests => _capturedRequests.AsReadOnly();

	public HttpRequestMessage? LastRequest => _capturedRequests.Count > 0 ? _capturedRequests[^1] : null;

	public MockHttpMessageHandler()
	{
		_defaultResponse = new MockResponse(HttpStatusCode.NotFound, """{"success":false,"error":{"code":"not_found","message":"No mock configured"}}""", null, null, "application/json");
	}

	/// <summary>
	/// Configures a mock response for a URL pattern (matched via Contains).
	/// </summary>
	public void SetupResponse(string urlPattern, HttpStatusCode statusCode, object responseBody, Dictionary<string, string>? headers = null, string? contentType = "application/json")
	{
		var json = responseBody is string s ? s : JsonSerializer.Serialize(responseBody, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		});

		_responses[urlPattern] = new MockResponse(statusCode, json, headers, null, contentType);
	}

	/// <summary>
	/// Sets a default response for any URL that doesn't match a configured pattern.
	/// </summary>
	public void SetupDefaultResponse(HttpStatusCode statusCode, object responseBody)
	{
		var json = responseBody is string s ? s : JsonSerializer.Serialize(responseBody, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		});
		_defaultResponse = new MockResponse(statusCode, json, null, null, "application/json");
	}

	/// <summary>
	/// Configures a mock to throw an HttpRequestException for a URL pattern.
	/// </summary>
	public void SetupException(string urlPattern, string message = "Network error")
	{
		_responses[urlPattern] = new MockResponse(HttpStatusCode.InternalServerError, string.Empty, null, new HttpRequestException(message), "application/json");
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		_capturedRequests.Add(request);

		var url = request.RequestUri?.ToString() ?? string.Empty;

		// Find matching response
		foreach (var kvp in _responses)
		{
			if (url.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
			{
				var mock = kvp.Value;

				if (mock.Exception != null)
					throw mock.Exception;

				var content = new StringContent(mock.Body, System.Text.Encoding.UTF8, "application/json");

				// A null ContentType serves no Content-Type header at all. That
				// is the case UnityRestSharp used to treat as "probably a WAF"
				// and retry five times, and it needs to be reachable from a test.
				content.Headers.ContentType = mock.ContentType is null
					? null
					: new System.Net.Http.Headers.MediaTypeHeaderValue(mock.ContentType) { CharSet = "utf-8" };

				var response = new HttpResponseMessage(mock.StatusCode)
				{
					Content = content,
					RequestMessage = request
				};

				if (mock.Headers != null)
				{
					foreach (var header in mock.Headers)
						response.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}

				return Task.FromResult(response);
			}
		}

		// Return default response
		var defaultResp = new HttpResponseMessage(_defaultResponse.StatusCode)
		{
			Content = new StringContent(_defaultResponse.Body, System.Text.Encoding.UTF8, "application/json"),
			RequestMessage = request
		};
		return Task.FromResult(defaultResp);
	}

	private record MockResponse(
		HttpStatusCode StatusCode,
		string Body,
		Dictionary<string, string>? Headers,
		Exception? Exception,
		string? ContentType);
}
