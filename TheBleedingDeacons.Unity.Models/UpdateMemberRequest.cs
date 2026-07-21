// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace TheBleedingDeacons.Unity.Models
{
	/// <summary>
	/// Request model for updating a member via PUT /members/{id}.
	/// All properties are optional — only supplied fields are updated (partial update).
	/// </summary>
	public class UpdateMemberRequest
	{
		/// <summary>
		/// The member's anonymous/display name.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? AnonymousName { get; init; }

		/// <summary>
		/// The member's personal email address.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? PersonalEmail { get; init; }

		/// <summary>
		/// The member's mobile phone number.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? MobileNumber { get; init; }

		/// <summary>
		/// Whether the anonymous name may be shown publicly.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public bool? ShowAnonymousName { get; init; }

		/// <summary>
		/// Whether the member profile may be shown publicly.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public bool? ShowMemberProfile { get; init; }

		/// <summary>
		/// The member's anonymous profile text.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? AnonymousProfile { get; init; }

		/// <summary>
		/// The identifier of the member's home group.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? HomeGroupId { get; init; }

		/// <summary>
		/// Whether the member is a Group Service Representative.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public bool? IsGsr { get; init; }

		/// <summary>
		/// The identifier of the member's intergroup position.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? IntergroupPositionId { get; init; }

		/// <summary>
		/// The rotation (term) of the member's intergroup position.
		/// </summary>
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? IntergroupPositionRotation { get; init; }
	}
}
