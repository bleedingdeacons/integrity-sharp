// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Internal error response structure for deserialization.
/// </summary>
internal sealed class ApiErrorResponse
{
	/// <summary>
	/// Gets a value indicating whether the request succeeded.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	/// Gets the error details returned by the API.
	/// </summary>
	public ApiError? Error { get; init; }
}
