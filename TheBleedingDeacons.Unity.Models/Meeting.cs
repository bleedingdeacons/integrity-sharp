// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TheBleedingDeacons.Unity.Models
{
	/// <summary>
	/// Represents a meeting in the Unity system.
	/// </summary>
	public class Meeting
	{
		/// <summary>
		/// Gets the meeting's unique identifier.
		/// </summary>
		public int Id { get; init; }

		/// <summary>
		/// Gets the meeting name.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the URL-friendly slug for the meeting.
		/// </summary>
		public string Slug { get; init; } = string.Empty;

		/// <summary>
		/// Gets the meeting's location, when present.
		/// </summary>
		public Location? Location { get; init; }

		/// <summary>
		/// Gets the canonical URL of the meeting.
		/// </summary>
		public string Url { get; init; } = string.Empty;

		/// <summary>
		/// Gets the day of the week the meeting occurs on (Sunday = 0).
		/// </summary>
		public int Day { get; init; }

		/// <summary>
		/// Gets the day of the week as a display string.
		/// </summary>
		public string DayOfWeek { get; init; } = string.Empty;

		/// <summary>
		/// Gets the meeting start time.
		/// </summary>
		public string Time { get; init; } = string.Empty;

		/// <summary>
		/// Gets the meeting end time.
		/// </summary>
		public string EndTime { get; init; } = string.Empty;

		/// <summary>
		/// Gets the meeting type codes (e.g. open, closed, step study).
		/// </summary>
		public List<string> Types { get; init; } = [];

		/// <summary>
		/// Gets the state or status of the meeting.
		/// </summary>
		public string State { get; init; } = string.Empty;

		/// <summary>
		/// Gets a value indicating whether the meeting is held online.
		/// </summary>
		public bool IsOnline { get; init; }

		/// <summary>
		/// Gets the link used to join the meeting online.
		/// </summary>
		public string OnlineLink { get; init; } = string.Empty;

		/// <summary>
		/// Gets additional notes about joining the meeting online.
		/// </summary>
		public string OnlineNotes { get; init; } = string.Empty;

		/// <summary>
		/// Gets the contacts associated with the meeting.
		/// </summary>
		public List<Contact> Contacts { get; init; } = [];

		/// <summary>
		/// Gets additional, free-form metadata for the meeting.
		/// </summary>
		public Dictionary<string, object>? Meta { get; init; }

		/// <summary>
		/// Gets the timestamp at which the meeting was last updated.
		/// </summary>
		[JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
		public DateTime? Updated { get; init; }
	}
}
