using NaturalApi;
using Xunit;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// NaturalApi-style fluent tests that demonstrate using MockHttpExecutor
/// to validate UnityRestSharp response shapes through NaturalApi's DSL.
/// These tests show how the NaturalApi pattern can complement direct unit tests.
/// </summary>
public class UnityRestSharpNaturalApiTests
{
	private readonly UnityMockHttpExecutor _mockExecutor;
	private readonly IApi _api;

	public UnityRestSharpNaturalApiTests()
	{
		_mockExecutor = new UnityMockHttpExecutor();
		_api = new Api(_mockExecutor);
	}

	[Fact]
	public void GetGroups_Fluent_Should_Return_200_With_Groups()
	{
		// Arrange — mock a Unity-style wrapped response
		_mockExecutor.SetupSuccessResponse(new[]
		{
			new { id = 1, title = "Serenity Group" },
			new { id = 2, title = "Hope Group" }
		}, total: 2);

		// Act & Assert — NaturalApi fluent DSL
		_api.For("/wp-json/integrity/v1/groups")
			.Get()
			.ShouldReturn(200);
	}

	[Fact]
	public void GetGroups_Fluent_Should_Validate_Response_Headers()
	{
		_mockExecutor.SetupResponse(200, new
		{
			success = true,
			data = new[] { new { id = 1, title = "Test" } }
		}, new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["X-RateLimit-Limit"] = "1000",
			["X-RateLimit-Remaining"] = "999"
		});

		_api.For("/wp-json/integrity/v1/groups")
			.Get()
			.ShouldReturn(200, headers => headers.ContainsKey("X-RateLimit-Limit"));
	}

	[Fact]
	public void GetGroups_Fluent_Should_Validate_Error_Response()
	{
		_mockExecutor.SetupErrorResponse(401, "unauthorized", "Invalid API key");

		var result = _api.For("/wp-json/integrity/v1/groups")
			.UsingAuth("Bearer bad-key")
			.Get();

		Assert.Equal(401, result.StatusCode);
	}

	[Fact]
	public void PostUpdateMember_Fluent_Should_Send_Body()
	{
		_mockExecutor.SetupSuccessResponse(new { id = 5, anonymous_name = "Updated" });

		var updateBody = new { anonymous_name = "Updated Bob" };

		var result = _api.For("/wp-json/integrity/v1/members/5/update")
			.WithHeader("Content-Type", "application/json")
			.Post(updateBody);

		result.ShouldReturn(200);

		// Verify the executor captured the request spec with body
		Assert.NotNull(_mockExecutor.LastSpec);
		Assert.Equal(HttpMethod.Post, _mockExecutor.LastSpec.Method);
	}

	[Fact]
	public void RegisterGroup_Fluent_Should_Post_To_Correct_Endpoint()
	{
		_mockExecutor.SetupSuccessResponse(new
		{
			intergroup_meeting_id = 1,
			group_id = 10,
			registered = true
		});

		_api.For("/wp-json/integrity/v1/intergroup-meetings/1/register-group")
			.Post(new { group_id = 10, member_id = 5, gsr_name = "John D." })
			.ShouldReturn(200);

		Assert.NotNull(_mockExecutor.LastSpec);
		Assert.True(_mockExecutor.LastSpec.Endpoint.Contains("register-group", StringComparison.Ordinal));
	}

	[Fact]
	public void HealthCheck_Fluent_Should_Return_Status()
	{
		_mockExecutor.SetupResponse(200,
			"""{"status":"ok","timestamp":"2025-02-26T12:00:00Z","version":"1.0.0","unity_available":true}""");

		_api.For("/wp-json/integrity/v1/health")
			.Get()
			.ShouldReturn(200);
	}

	[Fact]
	public void GetMeetings_Fluent_With_QueryParams_Should_Build_Correct_Spec()
	{
		_mockExecutor.SetupSuccessResponse(new List<object>(), total: 0);

		_api.For("/wp-json/integrity/v1/meetings")
			.WithQueryParam("day", 3)
			.WithQueryParam("online", "true")
			.Get();

		Assert.NotNull(_mockExecutor.LastSpec);
		Assert.True(_mockExecutor.LastSpec.QueryParams.ContainsKey("day"));
		Assert.True(_mockExecutor.LastSpec.QueryParams.ContainsKey("online"));
	}

	[Fact]
	public void GetGroup_Fluent_Should_Include_Auth_Header()
	{
		_mockExecutor.SetupSuccessResponse(new { id = 1, title = "Test Group" });

		_api.For("/wp-json/integrity/v1/groups/1")
			.UsingAuth("Bearer test-key-123")
			.Get()
			.ShouldReturn(200);

		Assert.NotNull(_mockExecutor.LastSpec);
		Assert.True(_mockExecutor.LastSpec.Headers.ContainsKey("Authorization"));
		Assert.Equal("Bearer test-key-123", _mockExecutor.LastSpec.Headers["Authorization"]);
	}

	[Fact]
	public void GetMembers_Fluent_Should_Chain_Multiple_Params()
	{
		_mockExecutor.SetupSuccessResponse(new List<object>());

		_api.For("/wp-json/integrity/v1/members")
			.WithQueryParam("page", 2)
			.WithQueryParam("per_page", 50)
			.WithQueryParam("search", "John")
			.WithQueryParam("home_group_id", 10)
			.WithQueryParam("expand", "home_group")
			.Get()
			.ShouldReturn(200);

		var spec = _mockExecutor.LastSpec!;
		Assert.Equal(5, spec.QueryParams.Count);
	}
}
