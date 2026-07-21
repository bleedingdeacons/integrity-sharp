// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBleedingDeacons.Unity.Models
{
	/// <summary>
	/// Represents the result of registering or unregistering a member from an intergroup meeting.
	/// </summary>
	public class IntergroupMeetingGroupRegistration
	{
		/// <summary>
		/// Gets the identifier of the intergroup meeting.
		/// </summary>
		public int IntergroupMeetingId { get; init; }

		/// <summary>
		/// Gets the display label of the intergroup meeting.
		/// </summary>
		public string MeetingLabel { get; init; } = string.Empty;

		/// <summary>
		/// Gets the identifier of the member (GSR) involved in the registration.
		/// </summary>
		public int MemberId { get; init; }

		/// <summary>
		/// Gets the name of the member (GSR) involved in the registration.
		/// </summary>
		public string MemberName { get; init; } = string.Empty;

		/// <summary>
		/// Gets the identifier of the group being registered.
		/// </summary>
		public int GroupId { get; init; }

		/// <summary>
		/// Gets the display name of the group being registered.
		/// </summary>
		public string MeetingGroup { get; init; } = string.Empty;

		/// <summary>
		/// Gets the name of the General Service Representative (GSR).
		/// </summary>
		public string GsrName { get; init; } = string.Empty;

		/// <summary>
		/// Gets a value indicating whether a proxy attended in place of the GSR.
		/// </summary>
		public bool GsrProxy { get; init; }

		/// <summary>
		/// Gets the name of the proxy, when a proxy attended.
		/// </summary>
		public string GsrProxyName { get; init; } = string.Empty;

		/// <summary>
		/// Gets a value indicating whether the group is registered after this operation.
		/// </summary>
		public bool Registered { get; init; }
	}
}
