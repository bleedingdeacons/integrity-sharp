// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// The result of a batch sent through WordPress core's <c>/wp-json/batch/v1</c>.
/// </summary>
/// <remarks>
/// <see cref="Responses"/> holds one entry per queued request, in queue order.
/// When the batch was sent with <c>requireAllValid</c> and any request failed
/// validation, nothing ran: <see cref="ValidationFailed"/> is true, the failing
/// requests carry their error and the rest are <see langword="null"/>.
/// </remarks>
public sealed class BatchResponse
{
	/// <summary>
	/// Gets a value indicating whether every request in the batch succeeded.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	/// Gets a value indicating whether a <c>requireAllValid</c> batch was rejected
	/// at validation, so that none of its requests ran.
	/// </summary>
	public bool ValidationFailed { get; init; }

	/// <summary>
	/// Gets the per-request responses, in queue order. An entry is
	/// <see langword="null"/> when its request did not run.
	/// </summary>
	public IReadOnlyList<BatchItemResponse?> Responses { get; init; } = [];

	/// <summary>
	/// Gets the error that refused the batch as a whole (for example, more
	/// requests than the server's limit, or a network failure), or
	/// <see langword="null"/> when the server processed the batch.
	/// </summary>
	public ApiError? Error { get; init; }

	/// <summary>
	/// Gets the HTTP status code of the batch call (207 when the server
	/// processed it; 0 when no response was received).
	/// </summary>
	public int StatusCode { get; init; }

	/// <summary>
	/// Gets the responses that did not succeed, with their position in the batch.
	/// </summary>
	/// <returns>Each failed response paired with its index.</returns>
	public IEnumerable<(int Index, BatchItemResponse Response)> Failures()
	{
		for (var i = 0; i < Responses.Count; i++)
		{
			var response = Responses[i];
			if (response is { Success: false })
				yield return (i, response);
		}
	}
}
