// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBleedingDeacons.Unity.Models
{
    /// <summary>
    /// Represents an attendee of an intergroup meeting.
    /// </summary>
    public class IntergroupMeetingAttendee
    {
        /// <summary>
        /// Gets the attendee's identifier.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the attendee's name.
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}
