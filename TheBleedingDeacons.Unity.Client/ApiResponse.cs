// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// API response wrapper with success status, data, and metadata.
/// </summary>
/// <typeparam name="T">The type of the payload carried in <see cref="Data"/>.</typeparam>
public sealed class ApiResponse<T> where T : class
{
	/// <summary>
	/// Gets a value indicating whether the request succeeded.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	/// Gets the response payload, or <see langword="null"/> when the request failed.
	/// </summary>
	public T? Data { get; init; }

	/// <summary>
	/// Gets the error details, or <see langword="null"/> when the request succeeded.
	/// </summary>
	public ApiError? Error { get; init; }

	/// <summary>
	/// Gets the pagination metadata for list responses, when present.
	/// </summary>
	public ResponseMeta? Meta { get; init; }

	/// <summary>
	/// Gets the HTTP status code of the response (0 when no response was received).
	/// </summary>
	public int StatusCode { get; init; }

	/// <summary>
	/// Gets the rate-limit information parsed from the response headers, when present.
	/// </summary>
	public RateLimitInfo? RateLimit { get; init; }
}
