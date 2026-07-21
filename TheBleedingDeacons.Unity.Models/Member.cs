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
    /// Represents a member in the Unity system.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Gets the member's unique identifier.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the member's private (full) name.
        /// </summary>
        public string PrivateName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the member's anonymous (public-facing) name.
        /// </summary>
        public string AnonymousName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the member's primary email address.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the member's personal email address.
        /// </summary>
        public string PersonalEmail { get; init; } = string.Empty;

        /// <summary>
        /// Gets the member's mobile phone number.
        /// </summary>
        public string MobileNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the anonymous name may be shown publicly.
        /// </summary>
        public bool ShowAnonymousName { get; init; }

        /// <summary>
        /// Gets a value indicating whether the member profile may be shown publicly.
        /// </summary>
        public bool ShowMemberProfile { get; init; }

        /// <summary>
        /// Gets the member's anonymous profile text.
        /// </summary>
        public string AnonymousProfile { get; init; } = string.Empty;

        /// <summary>
        /// Gets the identifier of the member's home group, if any.
        /// </summary>
        public int? HomeGroupId { get; init; }

        /// <summary>
        /// Gets the name of the member's home group.
        /// </summary>
        public string HomeGroupName { get; init; } = string.Empty;

        /// <summary>
        /// Full home group object (when expand=home_group is used).
        /// This will be populated when the API returns home_group.
        /// </summary>
        public Group? HomeGroup { get; init; }

        /// <summary>
        /// Gets a value indicating whether the member is a General Service Representative (GSR).
        /// </summary>
        public bool IsGsr { get; init; }

        /// <summary>
        /// Gets the member's meeting post-office / contact reference.
        /// </summary>
        public string MeetingPo { get; init; } = string.Empty;

        /// <summary>
        /// Gets the identifier of the intergroup position the member holds, if any.
        /// </summary>
        public int? IntergroupPositionId { get; init; }

        /// <summary>
        /// Gets the name of the intergroup position the member holds.
        /// </summary>
        public string IntergroupPositionName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the rotation (term) of the member's intergroup position.
        /// </summary>
        public string IntergroupPositionRotation { get; init; } = string.Empty;

        /// <summary>
        /// Gets the canonical link to the member.
        /// </summary>
        public string Link { get; init; } = string.Empty;

        /// <summary>
        /// Gets the timestamp at which the member was last updated.
        /// </summary>
        public DateTime? Updated { get; init; }

        /// <summary>
        /// GDPR compliance state for the member.
        ///
        /// <para>
        /// Null when talking to a server that pre-dates the compliance
        /// endpoint and therefore omits <c>gdpr_compliance</c> from its
        /// response. New servers always populate it.
        /// </para>
        /// </summary>
        public GdprCompliance? GdprCompliance { get; init; }

        /// <summary>
        /// Gets whether this member has expanded home group data.
        /// </summary>
        [JsonIgnore]
        public bool HasExpandedHomeGroup => HomeGroup != null;
    }
}
