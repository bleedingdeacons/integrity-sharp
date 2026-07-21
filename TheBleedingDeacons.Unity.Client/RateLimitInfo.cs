// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Rate limit information from response headers.
/// </summary>
public sealed class RateLimitInfo
{
	/// <summary>
	/// Gets the maximum number of requests allowed in the current window.
	/// </summary>
	public int Limit { get; init; }

	/// <summary>
	/// Gets the number of requests remaining in the current window.
	/// </summary>
	public int Remaining { get; init; }

	/// <summary>
	/// Gets the Unix timestamp (seconds) at which the rate-limit window resets.
	/// </summary>
	public long Reset { get; init; }

	/// <summary>
	/// Gets the <see cref="Reset"/> timestamp as a local <see cref="DateTime"/>.
	/// </summary>
	public DateTime ResetDateTime => DateTimeOffset.FromUnixTimeSeconds(Reset).LocalDateTime;
}
