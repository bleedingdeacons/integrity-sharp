// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBleedingDeacons.Unity.Models
{
    /// <summary>
    /// Digital contribution options for a group.
    /// </summary>
    public class ContributionOptions
    {
        /// <summary>
        /// Gets the group's Venmo handle.
        /// </summary>
        public string Venmo { get; init; } = string.Empty;

        /// <summary>
        /// Gets the group's PayPal handle.
        /// </summary>
        public string Paypal { get; init; } = string.Empty;

        /// <summary>
        /// Gets the group's Square handle.
        /// </summary>
        public string Square { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the group has any digital contribution options.
        /// </summary>
        public bool HasOptions { get; init; }
    }
}
