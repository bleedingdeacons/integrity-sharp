// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// The response to one request inside a batch.
/// </summary>
public sealed class BatchItemResponse
{
	/// <summary>
	/// Gets a value indicating whether this request succeeded.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	/// Gets the response payload, typed for the call that was queued: a
	/// <see cref="Models.Member"/> for the member calls, an
	/// <see cref="Models.IntergroupMeetingGroupRegistration"/> or
	/// <see cref="Models.IntergroupMeetingOfficerRegistration"/> for the
	/// intergroup calls. <see langword="null"/> when the request failed.
	/// </summary>
	public object? Data { get; init; }

	/// <summary>
	/// Gets the error details, or <see langword="null"/> when the request succeeded.
	/// </summary>
	public ApiError? Error { get; init; }

	/// <summary>
	/// Gets the HTTP status code the server gave this request.
	/// </summary>
	public int StatusCode { get; init; }

	/// <summary>
	/// Gets the rate-limit information for this request, when present.
	/// </summary>
	public RateLimitInfo? RateLimit { get; init; }

	/// <summary>
	/// Gets <see cref="Data"/> as <typeparamref name="T"/>, or <see langword="null"/>
	/// when it is absent or of another type.
	/// </summary>
	/// <typeparam name="T">The payload type expected for this request.</typeparam>
	/// <returns>The typed payload, or <see langword="null"/>.</returns>
	public T? DataAs<T>()
		where T : class
	{
		return Data as T;
	}
}
