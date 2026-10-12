using System.Net;
using System.Text.Json;
using TheBleedingDeacons.Unity.Client;
using TheBleedingDeacons.Unity.Models;
using Xunit;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// Tests for batching write requests through WordPress core's /wp-json/batch/v1.
/// </summary>
public class UnityRestSharpBatchTests : IDisposable
{
	private const string BaseUrl = "https://test.example.com";
	private const string ApiKey = "test-api-key-12345";

	private readonly MockHttpMessageHandler _mockHandler;
	private readonly HttpClient _httpClient;
	private readonly UnityRestSharp _client;

	public UnityRestSharpBatchTests()
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

	[Fact]
	public async Task SendAsync_Posts_Every_Queued_Request_To_Core_Batch_Endpoint()
	{
		_mockHandler.SetupResponse("/wp-json/batch/v1", (HttpStatusCode)207, """{"responses":[]}""");

		await _client.CreateBatch()
			.UpdateMember(5, new UpdateMemberRequest { AnonymousName = "Jane S." })
			.RegisterGroup(7, groupId: 12, memberId: 3, gsrName: "John D.")
			.UnregisterOfficer(7, officerId: 9)
			.SendAsync();

		var request = Assert.Single(_mockHandler.CapturedRequests);
		var sentBody = await request.Content!.ReadAsStringAsync();

		Assert.Equal(HttpMethod.Post, request.Method);
		Assert.Equal($"{BaseUrl}/wp-json/batch/v1", request.RequestUri!.ToString());
		Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
		Assert.Equal(ApiKey, request.Headers.Authorization?.Parameter);

		using var doc = JsonDocument.Parse(sentBody);
		var root = doc.RootElement;
		Assert.Equal("normal", root.GetProperty("validation").GetString());

		var requests = root.GetProperty("requests");
		Assert.Equal(3, requests.GetArrayLength());

		Assert.Equal("POST", requests[0].GetProperty("method").GetString());
		Assert.Equal("/integrity/v1/members/5/update", requests[0].GetProperty("path").GetString());
		Assert.Equal("Jane S.", requests[0].GetProperty("body").GetProperty("anonymous_name").GetString());

		Assert.Equal("/integrity/v1/intergroup-meetings/7/register-group", requests[1].GetProperty("path").GetString());
		Assert.Equal(12, requests[1].GetProperty("body").GetProperty("group_id").GetInt32());
		Assert.Equal("John D.", requests[1].GetProperty("body").GetProperty("gsr_name").GetString());

		Assert.Equal("/integrity/v1/intergroup-meetings/7/unregister-officer", requests[2].GetProperty("path").GetString());
		Assert.Equal(9, requests[2].GetProperty("body").GetProperty("officer_id").GetInt32());
	}

	[Fact]
	public async Task SendAsync_Asks_For_All_Or_Nothing_Validation_When_Requested()
	{
		_mockHandler.SetupResponse("/wp-json/batch/v1", (HttpStatusCode)207, """{"responses":[]}""");

		await _client.CreateBatch(requireAllValid: true).UnregisterGroup(7, 12).SendAsync();

		var sentBody = await _mockHandler.LastRequest!.Content!.ReadAsStringAsync();
		using var doc = JsonDocument.Parse(sentBody);
		Assert.Equal("require-all-validate", doc.RootElement.GetProperty("validation").GetString());
	}

	[Fact]
	public async Task SendAsync_Returns_Typed_Data_And_Per_Request_Errors_In_Order()
	{
		_mockHandler.SetupResponse("/wp-json/batch/v1", (HttpStatusCode)207, """
			{"responses":[
				{"body":{"success":true,"data":{"id":5,"anonymous_name":"Jane S."}},"status":200,
				 "headers":{"X-RateLimit-Limit":100,"X-RateLimit-Remaining":98,"X-RateLimit-Reset":1760000000}},
				{"body":{"code":"insufficient_permissions","message":"This API key does not have permission to access: intergroup-meetings:write","data":{"status":403}},"status":403,"headers":[]},
				{"body":{"success":false,"error":{"code":"not_found","message":"Officer not registered"}},"status":404,"headers":{"X-RateLimit-Limit":"100","X-RateLimit-Remaining":"97","X-RateLimit-Reset":"1760000000"}}
			]}
			""");

		var result = await _client.CreateBatch()
			.UpdateMember(5, new UpdateMemberRequest { AnonymousName = "Jane S." })
			.RegisterGroup(7, 12, 3, "John D.")
			.UnregisterOfficer(7, 9)
			.SendAsync();

		Assert.False(result.Success);
		Assert.False(result.ValidationFailed);
		Assert.Equal(207, result.StatusCode);
		Assert.Equal(3, result.Responses.Count);

		var member = result.Responses[0]!;
		Assert.True(member.Success);
		Assert.Equal(200, member.StatusCode);
		Assert.Equal(5, member.DataAs<Member>()!.Id);
		Assert.Equal(98, member.RateLimit!.Remaining);

		var refused = result.Responses[1]!;
		Assert.False(refused.Success);
		Assert.Equal(403, refused.StatusCode);
		Assert.Equal("insufficient_permissions", refused.Error!.Code);
		Assert.Null(refused.RateLimit);

		var notFound = result.Responses[2]!;
		Assert.Equal("not_found", notFound.Error!.Code);
		Assert.Equal(97, notFound.RateLimit!.Remaining);

		Assert.Equal(new[] { 1, 2 }, result.Failures().Select(f => f.Index));
	}

	[Fact]
	public async Task SendAsync_Reports_A_Rejected_All_Or_Nothing_Batch()
	{
		_mockHandler.SetupResponse("/wp-json/batch/v1", (HttpStatusCode)207, """
			{"failed":"validation","responses":[
				null,
				{"body":{"code":"rest_missing_callback_param","message":"Missing parameter(s): group_id","data":{"status":400}},"status":400,"headers":{}}
			]}
			""");

		var result = await _client.CreateBatch(requireAllValid: true)
			.UnregisterGroup(7, 12)
			.UnregisterGroup(7, 0)
			.SendAsync();

		Assert.False(result.Success);
		Assert.True(result.ValidationFailed);
		Assert.Null(result.Responses[0]);
		Assert.Equal("rest_missing_callback_param", result.Responses[1]!.Error!.Code);
	}

	[Fact]
	public async Task SendAsync_Succeeds_Only_When_Every_Request_Succeeds()
	{
		_mockHandler.SetupResponse("/wp-json/batch/v1", (HttpStatusCode)207, """
			{"responses":[
				{"body":{"success":true,"data":{"intergroup_meeting_id":7,"group_id":12}},"status":200,"headers":{}},
				{"body":{"success":true,"data":{"intergroup_meeting_id":7,"group_id":13}},"status":200,"headers":{}}
			]}
			""");

		var result = await _client.CreateBatch()
			.UnregisterGroup(7, 12)
			.UnregisterGroup(7, 13)
			.SendAsync();

		Assert.True(result.Success);
		Assert.Empty(result.Failures());
		Assert.IsType<IntergroupMeetingGroupRegistration>(result.Responses[0]!.Data);
	}

	[Fact]
	public async Task SendAsync_Surfaces_A_Refused_Batch_As_A_Batch_Error()
	{
		_mockHandler.SetupResponse("/wp-json/batch/v1", HttpStatusCode.BadRequest,
			"""{"code":"rest_invalid_param","message":"Invalid parameter(s): requests","data":{"status":400}}""");

		var result = await _client.CreateBatch().UnregisterGroup(7, 12).SendAsync();

		Assert.False(result.Success);
		Assert.Equal(400, result.StatusCode);
		Assert.Equal("rest_invalid_param", result.Error!.Code);
		Assert.Empty(result.Responses);
	}

	[Fact]
	public async Task SendAsync_On_An_Empty_Batch_Makes_No_Request()
	{
		var result = await _client.CreateBatch().SendAsync();

		Assert.True(result.Success);
		Assert.Empty(result.Responses);
		Assert.Empty(_mockHandler.CapturedRequests);
	}

	[Fact]
	public void Queueing_Past_The_Limit_Throws()
	{
		var batch = _client.CreateBatch(maxSize: 2)
			.UnregisterGroup(1, 1)
			.UnregisterGroup(1, 2);

		Assert.Equal(2, batch.Count);
		Assert.Throws<InvalidOperationException>(() => batch.UnregisterGroup(1, 3));
	}

	[Fact]
	public void CreateBatch_Defaults_To_Core_Limit_Of_25()
	{
		var batch = _client.CreateBatch();

		for (var i = 0; i < IntegrityBatch.DefaultMaxSize; i++)
			batch.UnregisterGroup(1, i);

		Assert.Equal(25, batch.Count);
		Assert.Throws<InvalidOperationException>(() => batch.UnregisterGroup(1, 99));
	}
}
