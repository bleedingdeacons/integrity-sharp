// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// API error details.
/// </summary>
public sealed class ApiError
{
	/// <summary>
	/// Gets the machine-readable error code (e.g. "unauthorized", "not_found").
	/// </summary>
	public required string Code { get; init; }

	/// <summary>
	/// Gets the human-readable error message.
	/// </summary>
	public required string Message { get; init; }
}
