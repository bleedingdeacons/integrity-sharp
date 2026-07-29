using System.Net;
using TheBleedingDeacons.Unity.Client;
using TheBleedingDeacons.Unity.Models;
using Xunit;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// Covers the error-handling, retry and health-check branches of UnityRestSharp:
/// non-success status codes, non-JSON and malformed bodies, rate-limit header
/// parsing, transient-failure retries with retry exhaustion, and the health
/// endpoint's failure paths.
/// </summary>
public class UnityRestSharpErrorRetryTests : IDisposable
{
	private const string BaseUrl = "https://test.example.com";
	private const string ApiKey = "test-api-key-12345";

	private readonly MockHttpMessageHandler _mockHandler;
	private readonly HttpClient _httpClient;
	private readonly UnityRestSharp _client;

	public UnityRestSharpErrorRetryTests()
	{
		_mockHandler = new MockHttpMessageHandler();
		_httpClient = new HttpClient(_mockHandler);
		_client = new UnityRestSharp(BaseUrl, ApiKey, _httpClient);
	}

	public void Dispose()
	{
		_client.Dispose();
		_httpClient.Dispose();
		GC.SuppressFinalize(this);
	}

	private static readonly Dictionary<string, string> ImmediateRetry = new(StringComparer.Ordinal) { ["Retry-After"] = "0" };

	// ── GET error branches ──────────────────────────────────────────────────
	[Theory]
	[InlineData(HttpStatusCode.BadRequest)]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.Forbidden)]
	[InlineData(HttpStatusCode.Conflict)]
	[InlineData(HttpStatusCode.UnprocessableEntity)]
	public async Task GetGroups_NonSuccessJson_ReturnsParsedError(HttpStatusCode status)
	{
		_mockHandler.SetupResponse("/groups", status, """{"success":false,"error":{"code":"denied","message":"Nope"}}""");

		var result = await _client.GetGroupsAsync();

		Assert.False(result.Success);
		Assert.Equal("denied", result.Error?.Code);
		Assert.Equal((int)status, result.StatusCode);
	}

	[Fact]
	public async Task GetGroups_NonJsonErrorBody_ReturnsUnexpectedResponse()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.BadRequest, "Server Error — not JSON");

		var result = await _client.GetGroupsAsync();

		Assert.False(result.Success);
		Assert.Equal("unexpected_response", result.Error?.Code);
	}

	[Fact]
	public async Task GetGroups_JsonWithoutErrorField_ReturnsUnknownError()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.BadRequest, """{"foo":1}""");

		var result = await _client.GetGroupsAsync();

		Assert.False(result.Success);
		Assert.Equal("unknown_error", result.Error?.Code);
	}

	[Fact]
	public async Task GetGroups_MalformedSuccessBody_ReturnsParseError()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, "{ this is not valid json");

		var result = await _client.GetGroupsAsync();

		Assert.False(result.Success);
		Assert.Equal("parse_error", result.Error?.Code);
	}

	[Fact]
	public async Task GetGroups_RateLimitHeaders_AreParsed()
	{
		var headers = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["X-RateLimit-Limit"] = "500",
			["X-RateLimit-Remaining"] = "499",
			["X-RateLimit-Reset"] = "1893456000",
		};
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, """{"success":true,"data":[]}""", headers);

		var result = await _client.GetGroupsAsync();

		Assert.True(result.Success);
		Assert.Equal(500, result.RateLimit?.Limit);
		Assert.Equal(499, result.RateLimit?.Remaining);
		Assert.Equal(1893456000L, result.RateLimit?.Reset);
	}

	// ── POST error branches ─────────────────────────────────────────────────
	[Theory]
	[InlineData(HttpStatusCode.BadRequest)]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.Forbidden)]
	[InlineData(HttpStatusCode.Conflict)]
	public async Task CreateMember_NonSuccessJson_ReturnsParsedError(HttpStatusCode status)
	{
		_mockHandler.SetupResponse("/members/create", status, """{"success":false,"error":{"code":"validation","message":"Bad"}}""");

		var result = await _client.CreateMemberAsync(new CreateMemberRequest { AnonymousName = "John D" });

		Assert.False(result.Success);
		Assert.Equal("validation", result.Error?.Code);
		Assert.Equal((int)status, result.StatusCode);
	}

	[Fact]
	public async Task CreateMember_NonJsonErrorBody_ReturnsUnexpectedResponse()
	{
		_mockHandler.SetupResponse("/members/create", HttpStatusCode.BadRequest, "WAF blocked this");

		var result = await _client.CreateMemberAsync(new CreateMemberRequest { AnonymousName = "John D" });

		Assert.Equal("unexpected_response", result.Error?.Code);
	}

	[Fact]
	public async Task CreateMember_MalformedSuccessBody_ReturnsParseError()
	{
		_mockHandler.SetupResponse("/members/create", HttpStatusCode.OK, "{ broken");

		var result = await _client.CreateMemberAsync(new CreateMemberRequest { AnonymousName = "John D" });

		Assert.Equal("parse_error", result.Error?.Code);
	}

	[Fact]
	public async Task CreateMember_Success_ReturnsMember()
	{
		_mockHandler.SetupResponse("/members/create", HttpStatusCode.Created, """{"success":true,"data":{"id":42,"anonymous_name":"John D"}}""");

		var result = await _client.CreateMemberAsync(new CreateMemberRequest { AnonymousName = "John D" });

		Assert.True(result.Success);
		Assert.Equal(42, result.Data?.Id);
	}

	// ── Retry / SendWithRetry ───────────────────────────────────────────────
	[Theory]
	[InlineData(HttpStatusCode.ServiceUnavailable)]
	[InlineData(HttpStatusCode.TooManyRequests)]
	[InlineData(HttpStatusCode.RequestTimeout)]
	public async Task Get_PersistentTransientStatus_ExhaustsRetriesAndThrows(HttpStatusCode status)
	{
		_mockHandler.SetupResponse("/groups", status, """{"success":false}""", ImmediateRetry);

		await Assert.ThrowsAsync<RestApiRequestFailed>(() => _client.GetGroupsAsync());
	}

	[Fact]
	public async Task Get_TransientThenSuccess_RetriesAndSucceeds()
	{
		using var handler = new SequenceHttpMessageHandler();
		handler.Enqueue(HttpStatusCode.ServiceUnavailable, """{"success":false}""", ImmediateRetry);
		handler.Enqueue(HttpStatusCode.OK, """{"success":true,"data":[]}""");
		using var httpClient = new HttpClient(handler);
		using var client = new UnityRestSharp(BaseUrl, ApiKey, httpClient);

		var result = await client.GetGroupsAsync();

		Assert.True(result.Success);
		Assert.Equal(2, handler.CallCount);
	}

	[Fact]
	public async Task RestApiRequestFailed_CarriesRequestContext()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.ServiceUnavailable, """{"success":false}""", ImmediateRetry);

		var ex = await Assert.ThrowsAsync<RestApiRequestFailed>(() => _client.GetGroupsAsync());

		Assert.Equal("GET", ex.Method);
		Assert.Equal(5, ex.Attempts);
		Assert.Contains("/groups", ex.Url, StringComparison.Ordinal);
	}

	// ── Query parameter branches ────────────────────────────────────────────
	[Fact]
	public async Task GetMeetings_OnlineFilter_AddsOnlineQueryParam()
	{
		_mockHandler.SetupResponse("/meetings", HttpStatusCode.OK, """{"success":true,"data":[]}""");

		await _client.GetMeetingsAsync(online: true);

		Assert.Contains("online=true", _mockHandler.LastRequest?.RequestUri?.ToString() ?? string.Empty, StringComparison.Ordinal);
	}

	[Fact]
	public async Task GetMembers_HomeGroupFilter_AddsHomeGroupQueryParam()
	{
		_mockHandler.SetupResponse("/members", HttpStatusCode.OK, """{"success":true,"data":[]}""");

		await _client.GetMembersAsync(homeGroupId: 5);

		Assert.Contains("home_group_id=5", _mockHandler.LastRequest?.RequestUri?.ToString() ?? string.Empty, StringComparison.Ordinal);
	}

	// ── Health ──────────────────────────────────────────────────────────────
	[Fact]
	public async Task CheckHealth_Success_ReturnsHealth()
	{
		_mockHandler.SetupResponse("/health", HttpStatusCode.OK, """{"status":"ok","timestamp":"2026-01-01T00:00:00Z","version":"1.0","unity_available":true}""");

		var health = await _client.CheckHealthAsync();

		Assert.NotNull(health);
		Assert.Equal("ok", health!.Status);
		Assert.True(health.UnityAvailable);
	}

	[Fact]
	public async Task CheckHealth_Forbidden_ReturnsNull()
	{
		_mockHandler.SetupResponse("/health", HttpStatusCode.Forbidden, """{"success":false,"error":{"code":"forbidden","message":"No"}}""");

		Assert.Null(await _client.CheckHealthAsync());
	}

	[Fact]
	public async Task CheckHealth_PersistentServerError_ReturnsNull()
	{
		_mockHandler.SetupResponse("/health", HttpStatusCode.InternalServerError, "boom", ImmediateRetry);

		// Retry exhaustion throws RestApiRequestFailed, which CheckHealth swallows.
		Assert.Null(await _client.CheckHealthAsync());
	}

	[Fact]
	public async Task CheckHealth_MalformedBody_ReturnsNull()
	{
		_mockHandler.SetupResponse("/health", HttpStatusCode.OK, "{ not json");

		Assert.Null(await _client.CheckHealthAsync());
	}
}
