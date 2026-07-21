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
	/// Represents a location in the Unity system.
	/// </summary>
	public class Location
	{
		/// <summary>
		/// Gets the location's unique identifier.
		/// </summary>
		public int Id { get; init; }

		/// <summary>
		/// Gets the location name.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the street address.
		/// </summary>
		public string Address { get; init; } = string.Empty;

		/// <summary>
		/// Gets the city or town.
		/// </summary>
		public string City { get; init; } = string.Empty;

		/// <summary>
		/// Gets the state, province, or county.
		/// </summary>
		public string State { get; init; } = string.Empty;

		/// <summary>
		/// Gets the postal or ZIP code.
		/// </summary>
		public string PostalCode { get; init; } = string.Empty;

		/// <summary>
		/// Gets the country.
		/// </summary>
		public string Country { get; init; } = string.Empty;

		/// <summary>
		/// Gets the region the location belongs to.
		/// </summary>
		public string Region { get; init; } = string.Empty;

		/// <summary>
		/// Gets free-form notes about the location.
		/// </summary>
		public string Notes { get; init; } = string.Empty;

		/// <summary>
		/// Gets the canonical link to the location.
		/// </summary>
		public string Link { get; init; } = string.Empty;

		/// <summary>
		/// Gets the latitude coordinate, when known.
		/// </summary>
		public double? Latitude { get; init; }

		/// <summary>
		/// Gets the longitude coordinate, when known.
		/// </summary>
		public double? Longitude { get; init; }

		/// <summary>
		/// Gets the IANA time zone identifier for the location.
		/// </summary>
		public string Timezone { get; init; } = string.Empty;

		/// <summary>
		/// Gets the fully formatted address string.
		/// </summary>
		public string FormattedAddress { get; init; } = string.Empty;

		/// <summary>
		/// Gets the timestamp at which the location was last updated.
		/// </summary>
		[JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
		public DateTime? Updated { get; init; }
	}
}
