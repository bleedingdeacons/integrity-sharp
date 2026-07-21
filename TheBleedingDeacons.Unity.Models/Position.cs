// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBleedingDeacons.Unity.Models
{
    /// <summary>
    /// Represents a position in the Unity system.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Gets the position's unique identifier.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the full name of the position.
        /// </summary>
        public string LongName { get; init; } = string.Empty;

        /// <summary>
        /// Gets a short description of the position.
        /// </summary>
        public string ShortDescription { get; init; } = string.Empty;

        /// <summary>
        /// Gets a summary of the position's responsibilities.
        /// </summary>
        public string Summary { get; init; } = string.Empty;

        /// <summary>
        /// Gets the contact email for the position.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the minimum sobriety (in years) required to hold the position.
        /// </summary>
        public int MinimumSobriety { get; init; }

        /// <summary>
        /// Gets the length of a term for the position, in years.
        /// </summary>
        public int TermYears { get; init; }

        /// <summary>
        /// Gets the canonical link to the position.
        /// </summary>
        public string Link { get; init; } = string.Empty;

        /// <summary>
        /// Gets the timestamp at which the position was last updated.
        /// </summary>
        public DateTime? Updated { get; init; }
    }
}
