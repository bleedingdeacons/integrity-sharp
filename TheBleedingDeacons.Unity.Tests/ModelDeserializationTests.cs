using System.Text.Json;
using TheBleedingDeacons.Unity.Models;
using Xunit;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// Deserialisation coverage for the model DTOs that are not exercised by the
/// end-to-end client tests (Location, Contact, ContributionOptions, member
/// requests and the intergroup-meeting attendee).
/// </summary>
public class ModelDeserializationTests
{
	private static readonly JsonSerializerOptions Options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
	};

	[Fact]
	public void Location_DeserializesAllFields()
	{
		const string Json = """
		{
			"id": 7,
			"name": "Church Hall",
			"address": "1 High St",
			"city": "Bristol",
			"state": "Avon",
			"postal_code": "BS1 1AA",
			"country": "UK",
			"region": "South West",
			"notes": "Rear entrance",
			"link": "https://example.test/loc/7",
			"latitude": 51.45,
			"longitude": -2.58,
			"timezone": "Europe/London",
			"formatted_address": "1 High St, Bristol",
			"updated": "2026-01-02T03:04:05Z"
		}
		""";

		var location = JsonSerializer.Deserialize<Location>(Json, Options);

		Assert.NotNull(location);
		Assert.Equal(7, location!.Id);
		Assert.Equal("Church Hall", location.Name);
		Assert.Equal("Bristol", location.City);
		Assert.Equal(51.45, location.Latitude);
		Assert.Equal("Europe/London", location.Timezone);
		Assert.Equal("1 High St, Bristol", location.FormattedAddress);
		Assert.NotNull(location.Updated);
	}

	[Fact]
	public void Contact_DeserializesAndTreatsEmptyUpdatedAsNull()
	{
		const string Json = """{"name":"Alice","email":"a@b.test","phone":"0700","updated":""}""";

		var contact = JsonSerializer.Deserialize<Contact>(Json, Options);

		Assert.NotNull(contact);
		Assert.Equal("Alice", contact!.Name);
		Assert.Equal("a@b.test", contact.Email);
		Assert.Equal("0700", contact.Phone);
		Assert.Null(contact.Updated);
	}

	[Fact]
	public void ContributionOptions_Deserializes()
	{
		const string Json = """{"venmo":"@grp","paypal":"grp","square":"$grp","has_options":true}""";

		var options = JsonSerializer.Deserialize<ContributionOptions>(Json, Options);

		Assert.NotNull(options);
		Assert.Equal("@grp", options!.Venmo);
		Assert.Equal("grp", options.Paypal);
		Assert.Equal("$grp", options.Square);
		Assert.True(options.HasOptions);
	}

	[Fact]
	public void IntergroupMeetingAttendee_Deserializes()
	{
		const string Json = """{"id":12,"name":"Bob"}""";

		var attendee = JsonSerializer.Deserialize<IntergroupMeetingAttendee>(Json, Options);

		Assert.NotNull(attendee);
		Assert.Equal(12, attendee!.Id);
		Assert.Equal("Bob", attendee.Name);
	}

	[Fact]
	public void CreateMemberRequest_SerializesOnlyPopulatedOptionalFields()
	{
		var request = new CreateMemberRequest
		{
			AnonymousName = "John D",
			PersonalEmail = "john@example.test",
			HomeGroupId = 5,
			IsGsr = true,
		};

		var json = JsonSerializer.Serialize(request, Options);

		Assert.Contains("\"anonymous_name\":\"John D\"", json, StringComparison.Ordinal);
		Assert.Contains("\"personal_email\":\"john@example.test\"", json, StringComparison.Ordinal);
		Assert.Contains("\"home_group_id\":5", json, StringComparison.Ordinal);
		Assert.Contains("\"is_gsr\":true", json, StringComparison.Ordinal);

		// Null optionals are omitted (JsonIgnoreCondition.WhenWritingNull).
		Assert.DoesNotContain("mobile_number", json, StringComparison.Ordinal);
		Assert.DoesNotContain("intergroup_position_id", json, StringComparison.Ordinal);
	}

	[Fact]
	public void Member_DeserializesAllFieldsIncludingNestedHomeGroup()
	{
		const string Json = """
		{
			"id": 3,
			"private_name": "John Smith",
			"anonymous_name": "John S",
			"email": "int@example.test",
			"personal_email": "john@example.test",
			"mobile_number": "0700 900000",
			"show_anonymous_name": true,
			"show_member_profile": false,
			"anonymous_profile": "A responder",
			"home_group_id": 5,
			"home_group_name": "Tuesday Group",
			"home_group": { "id": 5, "title": "Tuesday Group" },
			"is_gsr": true,
			"meeting_po": "PO1",
			"intergroup_position_id": 9,
			"intergroup_position_name": "Treasurer",
			"intergroup_position_rotation": "2026-09-01",
			"link": "https://example.test/m/3",
			"updated": "2026-01-02T03:04:05Z",
			"gdpr_compliance": { "accepted": true, "version": "1.0" }
		}
		""";

		var member = JsonSerializer.Deserialize<Member>(Json, Options);

		Assert.NotNull(member);
		Assert.Equal("John S", member!.AnonymousName);
		Assert.Equal("john@example.test", member.PersonalEmail);
		Assert.True(member.ShowAnonymousName);
		Assert.Equal(5, member.HomeGroupId);
		Assert.Equal("Tuesday Group", member.HomeGroup?.Title);
		Assert.True(member.IsGsr);
		Assert.Equal("Treasurer", member.IntergroupPositionName);
		Assert.NotNull(member.GdprCompliance);
	}

	[Fact]
	public void Meeting_DeserializesWithLocationAndContacts()
	{
		const string Json = """
		{
			"id": 11,
			"name": "Monday Nooners",
			"slug": "monday-nooners",
			"location": { "id": 2, "name": "Church Hall", "city": "Bristol" },
			"url": "https://example.test/mtg/11",
			"day": 1,
			"day_of_week": "Monday",
			"time": "12:00",
			"end_time": "13:00",
			"types": ["O", "D"],
			"state": "Avon",
			"is_online": false,
			"online_link": "",
			"online_notes": "",
			"contacts": [ { "name": "Alice", "email": "a@b.test", "phone": "0700" } ],
			"updated": "2026-01-02T03:04:05Z"
		}
		""";

		var meeting = JsonSerializer.Deserialize<Meeting>(Json, Options);

		Assert.NotNull(meeting);
		Assert.Equal("Monday Nooners", meeting!.Name);
		Assert.Equal("Bristol", meeting.Location?.City);
		Assert.Equal(2, meeting.Types.Count);
		Assert.Single(meeting.Contacts);
		Assert.Equal("Monday", meeting.DayOfWeek);
	}

	[Fact]
	public void IntergroupMeeting_DeserializesWithAttendees()
	{
		const string Json = """
		{
			"id": 4,
			"title": "March Intergroup",
			"date": "2026-03-15",
			"group_attendee_ids": [1, 2],
			"group_attendees": [ { "id": 1, "name": "Group A" } ],
			"officers_attending_ids": [7],
			"officers_attending": [ { "id": 7, "name": "Treasurer" } ],
			"updated": ""
		}
		""";

		var im = JsonSerializer.Deserialize<IntergroupMeeting>(Json, Options);

		Assert.NotNull(im);
		Assert.Equal("March Intergroup", im!.Title);
		Assert.Equal(2, im.GroupAttendeeIds.Count);
		Assert.Single(im.GroupAttendees);
		Assert.Single(im.OfficersAttending);
		Assert.Null(im.Updated);
	}

	[Fact]
	public void Position_DeserializesAllFields()
	{
		const string Json = """
		{
			"id": 6,
			"long_name": "Intergroup Treasurer",
			"short_description": "Handles funds",
			"summary": "Manages the intergroup's finances.",
			"email": "treasurer@example.test",
			"minimum_sobriety": 2,
			"term_years": 3,
			"link": "https://example.test/pos/6",
			"updated": "2026-01-02T03:04:05Z"
		}
		""";

		var position = JsonSerializer.Deserialize<Position>(Json, Options);

		Assert.NotNull(position);
		Assert.Equal("Intergroup Treasurer", position!.LongName);
		Assert.Equal(2, position.MinimumSobriety);
		Assert.Equal(3, position.TermYears);
	}

	[Fact]
	public void Group_DeserializesWithNestedMeetingsAndContributionOptions()
	{
		const string Json = """
		{
			"id": 8,
			"title": "Bristol Central",
			"email": "grp@example.test",
			"phone": "0117 000000",
			"website": "https://example.test",
			"link": "https://example.test/g/8",
			"notes": "Wheelchair access",
			"district_id": 3,
			"last_contact": "2025-12-01",
			"meeting_ids": [11, 12],
			"meetings": [ { "id": 11, "name": "Monday Nooners" } ],
			"contacts": [ { "name": "Bob", "email": "b@x.test", "phone": "0701" } ],
			"contribution_options": { "venmo": "@bc", "has_options": true },
			"updated": "2026-01-02T03:04:05Z"
		}
		""";

		var group = JsonSerializer.Deserialize<Group>(Json, Options);

		Assert.NotNull(group);
		Assert.Equal("Bristol Central", group!.Title);
		Assert.Equal(3, group.DistrictId);
		Assert.Equal("2025-12-01", group.LastContact);
		Assert.Equal(2, group.MeetingIds.Count);
		Assert.Single(group.Meetings);
		Assert.True(group.ContributionOptions?.HasOptions);
	}

	[Fact]
	public void CreateMemberRequest_RoundTripsThroughDeserialization()
	{
		const string Json = """
		{
			"anonymous_name": "Jane R",
			"mobile_number": "0700 900000",
			"intergroup_position_id": 9,
			"intergroup_position_rotation": "2026-09-01"
		}
		""";

		var request = JsonSerializer.Deserialize<CreateMemberRequest>(Json, Options);

		Assert.NotNull(request);
		Assert.Equal("Jane R", request!.AnonymousName);
		Assert.Equal("0700 900000", request.MobileNumber);
		Assert.Equal(9, request.IntergroupPositionId);
		Assert.Equal("2026-09-01", request.IntergroupPositionRotation);
	}
}
