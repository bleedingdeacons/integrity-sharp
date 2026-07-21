// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Models
{
    /// <summary>
    /// Represents the result of registering or unregistering an officer from an intergroup meeting.
    /// </summary>
    public class IntergroupMeetingOfficerRegistration
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
        /// Gets the identifier of the officer (member).
        /// </summary>
        public int OfficerId { get; init; }

        /// <summary>
        /// Gets the name of the officer.
        /// </summary>
        public string OfficerName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the name of the position the officer holds.
        /// </summary>
        public string PositionName { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the officer is registered after this operation.
        /// </summary>
        public bool Registered { get; init; }
    }
}
