using System.Net;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NaturalApi;
using TheBleedingDeacons.Unity.Client;
using TheBleedingDeacons.Unity.Models;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// Mocked unit tests for UnityRestSharp using NaturalApi's MockHttpExecutor pattern.
/// Each test injects a MockHttpMessageHandler into UnityRestSharp's HttpClient, verifying
/// that the client builds correct URLs, sends proper headers, and deserializes responses.
/// NaturalApi's fluent assertions validate the mock responses where applicable.
/// </summary>
[TestClass]
public class UnityRestSharpTests
{
	private const string BaseUrl = "https://test.example.com";
	private const string ApiKey = "test-api-key-12345";

	private MockHttpMessageHandler _mockHandler = null!;
	private HttpClient _httpClient = null!;
	private UnityRestSharp _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_mockHandler = new MockHttpMessageHandler();
		_httpClient = new HttpClient(_mockHandler);
		_client = new UnityRestSharp(BaseUrl, ApiKey, _httpClient);
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
	}

	#region Constructor Tests

	[TestMethod]
	public void Constructor_Should_Throw_When_BaseUrl_Is_Null()
	{
		Assert.ThrowsException<ArgumentNullException>(() =>
			new UnityRestSharp(null!, ApiKey));
	}

	[TestMethod]
	public void Constructor_Should_Throw_When_BaseUrl_Is_Empty()
	{
		Assert.ThrowsException<ArgumentException>(() =>
			new UnityRestSharp(string.Empty, ApiKey));
	}

	[TestMethod]
	public void Constructor_Should_Throw_When_ApiKey_Is_Null()
	{
		Assert.ThrowsException<ArgumentNullException>(() =>
			new UnityRestSharp(BaseUrl, null!));
	}

	[TestMethod]
	public void Constructor_Should_Throw_When_ApiKey_Is_Whitespace()
	{
		Assert.ThrowsException<ArgumentException>(() =>
			new UnityRestSharp(BaseUrl, "   "));
	}

	// Headers (Authorization, Accept, User-Agent) are stamped onto every
	// outgoing request via ApplyRequestHeaders() rather than set as defaults on
	// the HttpClient, so the client can share an HttpClient without header
	// accumulation. These tests therefore make a request and inspect the
	// captured HttpRequestMessage rather than HttpClient.DefaultRequestHeaders.
	private void SetupEmptyGroupsResponse()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});
	}

	[TestMethod]
	public async Task Request_Should_Set_Authorization_Header()
	{
		SetupEmptyGroupsResponse();

		await _client.GetGroupsAsync();

		var auth = _mockHandler.LastRequest!.Headers.Authorization;
		Assert.IsNotNull(auth);
		Assert.AreEqual("Bearer", auth.Scheme);
		Assert.AreEqual(ApiKey, auth.Parameter);
	}

	[TestMethod]
	public async Task Request_Should_Set_Accept_Header()
	{
		SetupEmptyGroupsResponse();

		await _client.GetGroupsAsync();

		var acceptHeader = _mockHandler.LastRequest!.Headers.Accept.FirstOrDefault();
		Assert.IsNotNull(acceptHeader);
		Assert.AreEqual("application/json", acceptHeader.MediaType);
	}

	[TestMethod]
	public async Task Request_Should_Set_UserAgent_Header()
	{
		SetupEmptyGroupsResponse();

		await _client.GetGroupsAsync();

		var userAgent = _mockHandler.LastRequest!.Headers.UserAgent.ToString();
		Assert.IsTrue(userAgent.Contains("IntegrityClient/1.0", StringComparison.Ordinal));
	}

	#endregion

	#region Groups - GET /groups

	[TestMethod]
	public async Task GetGroupsAsync_Should_Return_Groups_List()
	{
		// Arrange
		var groups = new List<Group>
		{
			new() { Id = 1, Title = "Serenity Group", Email = "serenity@test.com" },
			new() { Id = 2, Title = "Hope Group", Email = "hope@test.com" }
		};

		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = groups,
			meta = new { total = 2, page = 1, per_page = 100, total_pages = 1 }
		});

		// Act
		var result = await _client.GetGroupsAsync();

		// Assert
		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(2, result.Data.Count);
		Assert.AreEqual("Serenity Group", result.Data[0].Title);
		Assert.AreEqual("Hope Group", result.Data[1].Title);

		// Verify URL
		var request = _mockHandler.LastRequest!;
		Assert.IsTrue(request.RequestUri!.ToString().Contains("/wp-json/integrity/v1/groups", StringComparison.Ordinal));
		Assert.IsTrue(request.RequestUri.ToString().Contains("page=1", StringComparison.Ordinal));
		Assert.IsTrue(request.RequestUri.ToString().Contains("per_page=100", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Include_Search_Parameter()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetGroupsAsync(search: "Serenity");

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("search=Serenity", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Include_DistrictId_Parameter()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetGroupsAsync(districtId: 5);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("district_id=5", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Include_Expand_Meetings_Parameter()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetGroupsAsync(expandMeetings: true);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("expand=meetings", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetGroupAsync_Should_Return_Single_Group()
	{
		var group = new { id = 42, title = "Unity Group", email = "unity@test.com" };

		_mockHandler.SetupResponse("/groups/42", HttpStatusCode.OK, new
		{
			success = true,
			data = group
		});

		var result = await _client.GetGroupAsync(42);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(42, result.Data.Id);
		Assert.AreEqual("Unity Group", result.Data.Title);
	}

	[TestMethod]
	public async Task GetGroupAsync_Should_Append_Expand_Param()
	{
		_mockHandler.SetupResponse("/groups/1", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 1, title = "Test" }
		});

		await _client.GetGroupAsync(1, expandMeetings: true);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("expand=meetings", StringComparison.Ordinal));
	}

	#endregion

	#region Meetings - GET /meetings

	[TestMethod]
	public async Task GetMeetingsAsync_Should_Return_Meetings_List()
	{
		var meetings = new[]
		{
			new { id = 1, name = "Monday Night", day = 1, day_of_week = "Monday", time = "19:00", is_online = false },
			new { id = 2, name = "Friday Noon", day = 5, day_of_week = "Friday", time = "12:00", is_online = true }
		};

		_mockHandler.SetupResponse("/meetings", HttpStatusCode.OK, new
		{
			success = true,
			data = meetings,
			meta = new { total = 2, page = 1, per_page = 100, total_pages = 1 }
		});

		var result = await _client.GetMeetingsAsync();

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(2, result.Data.Count);
		Assert.AreEqual("Monday Night", result.Data[0].Name);
	}

	[TestMethod]
	public async Task GetMeetingsAsync_Should_Filter_By_DayOfWeek()
	{
		_mockHandler.SetupResponse("/meetings", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Meeting>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetMeetingsAsync(dayOfWeek: DayOfWeek.Wednesday);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("day=3", StringComparison.Ordinal)); // Wednesday = 3
	}

	[TestMethod]
	public async Task GetMeetingsAsync_Should_Filter_By_Online()
	{
		_mockHandler.SetupResponse("/meetings", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Meeting>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetMeetingsAsync(online: true);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("online=true", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetMeetingsAsync_Should_Filter_By_GroupId()
	{
		_mockHandler.SetupResponse("/meetings", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Meeting>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetMeetingsAsync(groupId: 7);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("group_id=7", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetMeetingAsync_Should_Return_Single_Meeting()
	{
		_mockHandler.SetupResponse("/meetings/10", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 10, name = "Big Book Study", day = 2, day_of_week = "Tuesday", time = "20:00" }
		});

		var result = await _client.GetMeetingAsync(10);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(10, result.Data.Id);
		Assert.AreEqual("Big Book Study", result.Data.Name);
	}

	#endregion

	#region Positions - GET /positions

	[TestMethod]
	public async Task GetPositionsAsync_Should_Return_Positions_List()
	{
		var positions = new[]
		{
			new { id = 1, long_name = "Chairperson", short_description = "Chair", term_years = 2 },
			new { id = 2, long_name = "Treasurer", short_description = "Treas", term_years = 2 }
		};

		_mockHandler.SetupResponse("/positions", HttpStatusCode.OK, new
		{
			success = true,
			data = positions,
			meta = new { total = 2, page = 1, per_page = 100, total_pages = 1 }
		});

		var result = await _client.GetPositionsAsync();

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(2, result.Data.Count);
		Assert.AreEqual("Chairperson", result.Data[0].LongName);
	}

	[TestMethod]
	public async Task GetPositionsAsync_Should_Include_Search_Parameter()
	{
		_mockHandler.SetupResponse("/positions", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Position>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetPositionsAsync(search: "Chair");

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("search=Chair", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetPositionAsync_Should_Return_Single_Position()
	{
		_mockHandler.SetupResponse("/positions/3", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 3, long_name = "Secretary", short_description = "Sec", term_years = 1 }
		});

		var result = await _client.GetPositionAsync(3);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(3, result.Data.Id);
		Assert.AreEqual("Secretary", result.Data.LongName);
	}

	#endregion

	#region Members - GET /members

	[TestMethod]
	public async Task GetMembersAsync_Should_Return_Members_List()
	{
		var members = new[]
		{
			new { id = 1, private_name = "John D.", anonymous_name = "John D.", email = "john@test.com", home_group_id = 10 },
			new { id = 2, private_name = "Jane S.", anonymous_name = "Jane S.", email = "jane@test.com", home_group_id = 20 }
		};

		_mockHandler.SetupResponse("/members", HttpStatusCode.OK, new
		{
			success = true,
			data = members,
			meta = new { total = 2, page = 1, per_page = 100, total_pages = 1 }
		});

		var result = await _client.GetMembersAsync();

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(2, result.Data.Count);
		Assert.AreEqual("John D.", result.Data[0].PrivateName);
	}

	[TestMethod]
	public async Task GetMembersAsync_Should_Filter_By_HomeGroupId()
	{
		_mockHandler.SetupResponse("/members", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Member>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetMembersAsync(homeGroupId: 42);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("home_group_id=42", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetMembersAsync_Should_Include_Expand_HomeGroup()
	{
		_mockHandler.SetupResponse("/members", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Member>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetMembersAsync(expandHomeGroup: true);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("expand=home_group", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetMemberAsync_Should_Return_Single_Member()
	{
		_mockHandler.SetupResponse("/members/5", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 5, private_name = "Bob R.", anonymous_name = "Bob R.", email = "bob@test.com" }
		});

		var result = await _client.GetMemberAsync(5);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(5, result.Data.Id);
		Assert.AreEqual("Bob R.", result.Data.PrivateName);
	}

	[TestMethod]
	public async Task GetMemberAsync_Should_Append_Expand_Param()
	{
		_mockHandler.SetupResponse("/members/5", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 5, private_name = "Bob R." }
		});

		await _client.GetMemberAsync(5, expandHomeGroup: true);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("expand=home_group", StringComparison.Ordinal));
	}

	#endregion

	#region Members - POST /members/{id}/update

	[TestMethod]
	public async Task UpdateMemberAsync_Should_Send_Post_With_Body()
	{
		_mockHandler.SetupResponse("/members/5/update", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 5, private_name = "Bob R.", anonymous_name = "Updated Bob", email = "bob@test.com" }
		});

		var updateRequest = new UpdateMemberRequest
		{
			AnonymousName = "Updated Bob",
			MobileNumber = "555-1234"
		};

		var result = await _client.UpdateMemberAsync(5, updateRequest);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);

		// Verify it was a POST
		Assert.AreEqual(HttpMethod.Post, _mockHandler.LastRequest!.Method);

		// Verify the URL
		Assert.IsTrue(_mockHandler.LastRequest.RequestUri!.ToString().Contains("/members/5/update", StringComparison.Ordinal));

		// Verify the body was sent
		var body = await _mockHandler.LastRequest.Content!.ReadAsStringAsync();
		Assert.IsTrue(body.Contains("anonymous_name", StringComparison.Ordinal));
		Assert.IsTrue(body.Contains("Updated Bob", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task UpdateMemberAsync_Should_Omit_Null_Fields()
	{
		_mockHandler.SetupResponse("/members/1/update", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 1, private_name = "Test" }
		});

		var updateRequest = new UpdateMemberRequest
		{
			AnonymousName = "Only This Field"

			// All other fields are null → should be omitted
		};

		await _client.UpdateMemberAsync(1, updateRequest);

		var body = await _mockHandler.LastRequest!.Content!.ReadAsStringAsync();
		Assert.IsTrue(body.Contains("anonymous_name", StringComparison.Ordinal));
		Assert.IsFalse(body.Contains("personal_email", StringComparison.Ordinal));
		Assert.IsFalse(body.Contains("mobile_number", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task RecordComplianceAsync_Should_Send_Post_With_Body()
	{
		_mockHandler.SetupResponse("/members/7/compliance", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				id = 7,
				anonymous_name = "Bob R.",
				gdpr_compliance = new
				{
					accepted = true,
					accepted_at = "2026-04-27T15:45:00.000Z",
					version = "2.1",
					method = "api",
					statement = "I agree to the privacy policy.",
				},
			},
		});

		var request = new RecordComplianceRequest
		{
			Accepted = true,
			Version = "2.1",
		};

		var result = await _client.RecordComplianceAsync(7, request);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(HttpMethod.Post, _mockHandler.LastRequest!.Method);
		Assert.IsTrue(_mockHandler.LastRequest.RequestUri!.ToString().Contains("/members/7/compliance", StringComparison.Ordinal));

		var body = await _mockHandler.LastRequest.Content!.ReadAsStringAsync();
		Assert.IsTrue(body.Contains("\"accepted\":true", StringComparison.Ordinal));
		Assert.IsTrue(body.Contains("\"version\":\"2.1\"", StringComparison.Ordinal));

		// Only accepted + version were set on the request; every other field is
		// null and omitted by WhenWritingNull. Note the request no longer carries
		// a "statement" field at all — it was replaced on the wire by policy_id.
		Assert.IsFalse(body.Contains("\"method\"", StringComparison.Ordinal));
		Assert.IsFalse(body.Contains("\"accepted_at\"", StringComparison.Ordinal));
		Assert.IsFalse(body.Contains("statement", StringComparison.Ordinal));
		Assert.IsFalse(body.Contains("policy_id", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task RecordComplianceAsync_Should_Hydrate_GdprCompliance_From_Response()
	{
		_mockHandler.SetupResponse("/members/7/compliance", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				id = 7,
				anonymous_name = "Bob R.",
				gdpr_compliance = new
				{
					accepted = true,
					accepted_at = "2026-04-27T15:45:00.000Z",
					version = "2.1",
					method = "api",
					statement = "I agree to the privacy policy.",
				},
			},
		});

		var request = new RecordComplianceRequest { Accepted = true, Version = "2.1" };
		var result = await _client.RecordComplianceAsync(7, request);

		Assert.IsNotNull(result.Data);
		Assert.IsNotNull(result.Data.GdprCompliance);
		Assert.IsTrue(result.Data.GdprCompliance.Accepted);
		Assert.AreEqual("2.1", result.Data.GdprCompliance.Version);
		Assert.AreEqual("api", result.Data.GdprCompliance.Method);
		Assert.AreEqual("I agree to the privacy policy.", result.Data.GdprCompliance.Statement);
		Assert.IsNotNull(result.Data.GdprCompliance.AcceptedAt);
		Assert.AreEqual(
			new DateTime(2026, 4, 27, 15, 45, 0, DateTimeKind.Utc),
			result.Data.GdprCompliance.AcceptedAt!.Value.ToUniversalTime());
	}

	[TestMethod]
	public async Task RecordComplianceAsync_Should_Treat_Missing_GdprCompliance_As_Null()
	{
		// Older server that pre-dates the gdpr_compliance field — clients
		// built against the new model should hydrate cleanly with null
		// rather than fabricating a "not accepted" state.
		_mockHandler.SetupResponse("/members/3/compliance", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 3, anonymous_name = "Legacy" },
		});

		var result = await _client.RecordComplianceAsync(
			3,
			new RecordComplianceRequest { Accepted = false });

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsNull(result.Data.GdprCompliance);
	}

	[TestMethod]
	public async Task RecordComplianceAsync_Should_Send_Revocation()
	{
		_mockHandler.SetupResponse("/members/9/compliance", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				id = 9,
				gdpr_compliance = new
				{
					accepted = false,
					accepted_at = "2026-04-27T16:00:00.000Z",
					version = string.Empty,
					method = string.Empty,
					statement = string.Empty,
				},
			},
		});

		var result = await _client.RecordComplianceAsync(
			9,
			new RecordComplianceRequest { Accepted = false });

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsNotNull(result.Data.GdprCompliance);
		Assert.IsFalse(result.Data.GdprCompliance.Accepted);
		Assert.AreEqual(string.Empty, result.Data.GdprCompliance.Version);
		Assert.AreEqual(string.Empty, result.Data.GdprCompliance.Method);
		Assert.AreEqual(string.Empty, result.Data.GdprCompliance.Statement);

		var body = await _mockHandler.LastRequest!.Content!.ReadAsStringAsync();
		Assert.IsTrue(body.Contains("\"accepted\":false", StringComparison.Ordinal));
	}

	#endregion

	#region Intergroup Meetings - GET

	[TestMethod]
	public async Task GetIntergroupMeetingsAsync_Should_Return_List()
	{
		_mockHandler.SetupResponse("/intergroup-meetings", HttpStatusCode.OK, new
		{
			success = true,
			data = new[]
			{
				new { id = 1, title = "January Intergroup", date = "2025-01-15" },
				new { id = 2, title = "February Intergroup", date = "2025-02-12" }
			},
			meta = new { total = 2, page = 1, per_page = 100, total_pages = 1 }
		});

		var result = await _client.GetIntergroupMeetingsAsync();

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(2, result.Data.Count);
		Assert.AreEqual("January Intergroup", result.Data[0].Title);
	}

	[TestMethod]
	public async Task GetIntergroupMeetingsAsync_Should_Include_Date_Filters()
	{
		_mockHandler.SetupResponse("/intergroup-meetings", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<IntergroupMeeting>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetIntergroupMeetingsAsync(
			dateFrom: new DateOnly(2025, 1, 1),
			dateTo: new DateOnly(2025, 12, 31));

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("date_from=2025-01-01", StringComparison.Ordinal));
		Assert.IsTrue(url.Contains("date_to=2025-12-31", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetIntergroupMeetingAsync_Should_Return_Single()
	{
		_mockHandler.SetupResponse("/intergroup-meetings/7", HttpStatusCode.OK, new
		{
			success = true,
			data = new { id = 7, title = "March Intergroup", date = "2025-03-12" }
		});

		var result = await _client.GetIntergroupMeetingAsync(7);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual(7, result.Data.Id);
	}

	#endregion

	#region Intergroup Meetings - Registration

	[TestMethod]
	public async Task RegisterGroupAsync_Should_Post_Registration()
	{
		_mockHandler.SetupResponse("/register-group", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				intergroup_meeting_id = 1,
				group_id = 10,
				member_id = 5,
				gsr_name = "John D.",
				gsr_proxy = false,
				registered = true
			}
		});

		var result = await _client.RegisterGroupAsync(
			intergroupMeetingId: 1,
			groupId: 10,
			memberId: 5,
			gsrName: "John D.");

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.Registered);

		// Verify POST method
		Assert.AreEqual(HttpMethod.Post, _mockHandler.LastRequest!.Method);

		// Verify body
		var body = await _mockHandler.LastRequest.Content!.ReadAsStringAsync();
		Assert.IsTrue(body.Contains("group_id", StringComparison.Ordinal));
		Assert.IsTrue(body.Contains("gsr_name", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task RegisterGroupAsync_Should_Include_Proxy_Info()
	{
		_mockHandler.SetupResponse("/register-group", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				intergroup_meeting_id = 1,
				group_id = 10,
				member_id = 5,
				gsr_name = "John D.",
				gsr_proxy = true,
				gsr_proxy_name = "Jane S.",
				registered = true
			}
		});

		await _client.RegisterGroupAsync(
			intergroupMeetingId: 1,
			groupId: 10,
			memberId: 5,
			gsrName: "John D.",
			gsrProxy: true,
			gsrProxyName: "Jane S.");

		var body = await _mockHandler.LastRequest!.Content!.ReadAsStringAsync();
		Assert.IsTrue(body.Contains("gsr_proxy", StringComparison.Ordinal));
		Assert.IsTrue(body.Contains("Jane S.", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task UnregisterGroupAsync_Should_Post_Unregistration()
	{
		_mockHandler.SetupResponse("/unregister-group", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				intergroup_meeting_id = 1,
				group_id = 10,
				registered = false
			}
		});

		var result = await _client.UnregisterGroupAsync(intergroupMeetingId: 1, groupId: 10);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(HttpMethod.Post, _mockHandler.LastRequest!.Method);
	}

	[TestMethod]
	public async Task RegisterOfficerAsync_Should_Post_Registration()
	{
		_mockHandler.SetupResponse("/register-officer", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				intergroup_meeting_id = 1,
				officer_id = 3,
				officer_name = "Bob R.",
				position_name = "Chairperson",
				registered = true
			}
		});

		var result = await _client.RegisterOfficerAsync(
			intergroupMeetingId: 1,
			officerId: 3,
			positionName: "Chairperson",
			officerName: "Bob R.");

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.Registered);
		Assert.AreEqual(HttpMethod.Post, _mockHandler.LastRequest!.Method);
	}

	[TestMethod]
	public async Task UnregisterOfficerAsync_Should_Post_Unregistration()
	{
		_mockHandler.SetupResponse("/unregister-officer", HttpStatusCode.OK, new
		{
			success = true,
			data = new
			{
				intergroup_meeting_id = 1,
				officer_id = 3,
				registered = false
			}
		});

		var result = await _client.UnregisterOfficerAsync(intergroupMeetingId: 1, officerId: 3);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(HttpMethod.Post, _mockHandler.LastRequest!.Method);
	}

	#endregion

	#region Health Check

	[TestMethod]
	public async Task CheckHealthAsync_Should_Return_Health_Status()
	{
		_mockHandler.SetupResponse("/health", HttpStatusCode.OK, new
		{
			status = "ok",
			timestamp = "2025-02-26T12:00:00Z",
			version = "1.0.0",
			unity_available = true
		});

		var result = await _client.CheckHealthAsync();

		Assert.IsNotNull(result);
		Assert.AreEqual("ok", result.Status);
		Assert.IsTrue(result.UnityAvailable);
	}

	[TestMethod]
	public async Task CheckHealthAsync_Should_Return_Null_On_Error()
	{
		_mockHandler.SetupException("/health", "Connection refused");

		var result = await _client.CheckHealthAsync();

		Assert.IsNull(result);
	}

	#endregion

	#region Error Handling

	[TestMethod]
	public async Task GetGroupsAsync_Should_Handle_401_Unauthorized()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.Unauthorized, new
		{
			success = false,
			error = new { code = "unauthorized", message = "Invalid API key" }
		});

		var result = await _client.GetGroupsAsync();

		Assert.IsFalse(result.Success);
		Assert.AreEqual(401, result.StatusCode);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("unauthorized", result.Error.Code);
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Handle_403_Forbidden()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.Forbidden, new
		{
			success = false,
			error = new { code = "forbidden", message = "Insufficient permissions" }
		});

		var result = await _client.GetGroupsAsync();

		Assert.IsFalse(result.Success);
		Assert.AreEqual(403, result.StatusCode);
		Assert.AreEqual("forbidden", result.Error!.Code);
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Handle_404_NotFound()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.NotFound, new
		{
			success = false,
			error = new { code = "not_found", message = "Resource not found" }
		});

		var result = await _client.GetGroupsAsync();

		Assert.IsFalse(result.Success);
		Assert.AreEqual(404, result.StatusCode);
	}

	// 429, 5xx and network errors are transient: the client retries them
	// MaxRetryAttempts times with backoff and, once exhausted, throws
	// RestApiRequestFailed so callers can handle retry exhaustion explicitly
	// (rather than receiving a "successful" error ApiResponse). Rate-limit
	// header parsing on a *successful* response is covered by
	// Should_Parse_RateLimit_Headers.
	[TestMethod]
	public async Task GetGroupsAsync_Should_Throw_After_Retrying_429_RateLimit()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.TooManyRequests, new
		{
			success = false,
			error = new { code = "rate_limited", message = "Too many requests" }
		});

		var ex = await Assert.ThrowsExceptionAsync<RestApiRequestFailed>(
			() => _client.GetGroupsAsync());

		Assert.AreEqual(429, ex.LastStatusCode);
		Assert.AreEqual(5, ex.Attempts);
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Throw_After_Retrying_500_ServerError()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.InternalServerError, new
		{
			success = false,
			error = new { code = "server_error", message = "Internal server error" }
		});

		var ex = await Assert.ThrowsExceptionAsync<RestApiRequestFailed>(
			() => _client.GetGroupsAsync());

		Assert.AreEqual(500, ex.LastStatusCode);
		Assert.AreEqual(5, ex.Attempts);
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Throw_After_Retrying_Network_Error()
	{
		_mockHandler.SetupException("/groups", "Connection timed out");

		var ex = await Assert.ThrowsExceptionAsync<RestApiRequestFailed>(
			() => _client.GetGroupsAsync());

		// No response was ever received, so there is no last status code.
		Assert.IsNull(ex.LastStatusCode);
		Assert.AreEqual(5, ex.Attempts);
		Assert.IsTrue(ex.Reason.Contains("network error", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task GetGroupsAsync_Should_Handle_Malformed_Json()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, "this is not valid json{{{");

		var result = await _client.GetGroupsAsync();

		Assert.IsFalse(result.Success);
		Assert.AreEqual("parse_error", result.Error!.Code);
	}

	#endregion

	#region Rate Limit Headers

	[TestMethod]
	public async Task Should_Parse_RateLimit_Headers()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		}, headers: new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["X-RateLimit-Limit"] = "1000",
			["X-RateLimit-Remaining"] = "999",
			["X-RateLimit-Reset"] = "1740600000"
		});

		var result = await _client.GetGroupsAsync();

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.RateLimit);
		Assert.AreEqual(1000, result.RateLimit.Limit);
		Assert.AreEqual(999, result.RateLimit.Remaining);
		Assert.AreEqual(1740600000L, result.RateLimit.Reset);
	}

	#endregion

	#region Pagination

	[TestMethod]
	public async Task GetGroupsAsync_Should_Support_Pagination()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 250, page = 3, per_page = 50, total_pages = 5 }
		});

		var result = await _client.GetGroupsAsync(page: 3, perPage: 50);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Meta);
		Assert.AreEqual(250, result.Meta.Total);
		Assert.AreEqual(3, result.Meta.Page);
		Assert.AreEqual(50, result.Meta.PerPage);
		Assert.AreEqual(5, result.Meta.TotalPages);

		var url = _mockHandler.LastRequest!.RequestUri!.ToString();
		Assert.IsTrue(url.Contains("page=3", StringComparison.Ordinal));
		Assert.IsTrue(url.Contains("per_page=50", StringComparison.Ordinal));
	}

	#endregion

	#region URL Construction

	[TestMethod]
	public async Task Should_Trim_Trailing_Slash_From_BaseUrl()
	{
		var handler = new MockHttpMessageHandler();
		handler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		var httpClient = new HttpClient(handler);
		using var client = new UnityRestSharp("https://test.example.com/", ApiKey, httpClient);

		await client.GetGroupsAsync();

		var url = handler.LastRequest!.RequestUri!.ToString();
		Assert.IsFalse(url.Contains("test.example.com//", StringComparison.Ordinal));
		Assert.IsTrue(url.StartsWith("https://test.example.com/wp-json/integrity/v1/groups", StringComparison.Ordinal));
	}

	[TestMethod]
	public async Task Should_Encode_Search_Parameters()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		await _client.GetGroupsAsync(search: "test group & friends");

		// AbsoluteUri preserves the percent-encoding; RequestUri.ToString()
		// would decode %20 back to a space and mask the escaping.
		var url = _mockHandler.LastRequest!.RequestUri!.AbsoluteUri;
		Assert.IsTrue(url.Contains("search=test%20group%20%26%20friends", StringComparison.Ordinal) ||
					   url.Contains("search=test+group+%26+friends", StringComparison.Ordinal));
	}

	#endregion

	#region Cancellation

	[TestMethod]
	public async Task GetGroupsAsync_Should_Respect_CancellationToken()
	{
		_mockHandler.SetupResponse("/groups", HttpStatusCode.OK, new
		{
			success = true,
			data = new List<Group>(),
			meta = new { total = 0, page = 1, per_page = 100, total_pages = 0 }
		});

		using var cts = new CancellationTokenSource();
		cts.Cancel();

		// Should throw OperationCanceledException or TaskCanceledException
		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
			await _client.GetGroupsAsync(cancellationToken: cts.Token).ConfigureAwait(false));
	}

	#endregion
}
