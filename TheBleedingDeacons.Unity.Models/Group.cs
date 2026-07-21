// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace TheBleedingDeacons.Unity.Models
{
	/// <summary>
	/// Represents a group in the Unity system.
	/// </summary>
	public class Group
	{
		/// <summary>
		/// Gets the group's unique identifier.
		/// </summary>
		public int Id { get; init; }

		/// <summary>
		/// Gets the group title.
		/// </summary>
		public string Title { get; init; } = string.Empty;

		/// <summary>
		/// Gets the group's contact email address.
		/// </summary>
		public string Email { get; init; } = string.Empty;

		/// <summary>
		/// Gets the group's contact phone number.
		/// </summary>
		public string Phone { get; init; } = string.Empty;

		/// <summary>
		/// Gets the group's website URL.
		/// </summary>
		public string Website { get; init; } = string.Empty;

		/// <summary>
		/// Gets the canonical link to the group.
		/// </summary>
		public string Link { get; init; } = string.Empty;

		/// <summary>
		/// Gets free-form notes about the group.
		/// </summary>
		public string Notes { get; init; } = string.Empty;

		/// <summary>
		/// Gets the identifier of the district the group belongs to, if any.
		/// </summary>
		public int? DistrictId { get; init; }

		/// <summary>
		/// Gets the date the group was last contacted.
		/// </summary>
		public string? LastContact { get; init; }

		/// <summary>
		/// Meeting IDs associated with this group (when expand=meetings is not used).
		/// This will be populated when the API returns meeting_ids.
		/// </summary>
		public List<int> MeetingIds { get; init; } = [];

		/// <summary>
		/// Full meeting objects associated with this group (when expand=meetings is used).
		/// This will be populated when the API returns meetings.
		/// </summary>
		public List<Meeting> Meetings { get; init; } = [];

		/// <summary>
		/// Gets the contacts associated with the group.
		/// </summary>
		public List<Contact> Contacts { get; init; } = [];

		/// <summary>
		/// Gets the group's digital contribution options, when present.
		/// </summary>
		public ContributionOptions? ContributionOptions { get; init; }

		/// <summary>
		/// Gets the timestamp at which the group was last updated.
		/// </summary>
		[JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
		public DateTime? Updated { get; init; }

		/// <summary>
		/// Gets whether this group has expanded meeting data.
		/// </summary>
		[JsonIgnore]
		public bool HasExpandedMeetings => Meetings.Count > 0;
	}
}
