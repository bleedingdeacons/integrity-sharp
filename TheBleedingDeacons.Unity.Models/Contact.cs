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
	/// Contact information.
	/// </summary>
	public class Contact
	{
		/// <summary>
		/// Gets the contact's name.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the contact's email address.
		/// </summary>
		public string Email { get; init; } = string.Empty;

		/// <summary>
		/// Gets the contact's phone number.
		/// </summary>
		public string Phone { get; init; } = string.Empty;

		/// <summary>
		/// Gets the timestamp at which the contact was last updated.
		/// </summary>
		[JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
		public DateTime? Updated { get; init; }
	}
}
