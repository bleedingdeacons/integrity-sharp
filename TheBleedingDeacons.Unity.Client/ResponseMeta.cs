// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Response pagination metadata.
/// </summary>
public sealed class ResponseMeta
{
	/// <summary>
	/// Gets the total number of items across all pages.
	/// </summary>
	public int Total { get; init; }

	/// <summary>
	/// Gets the current page number (1-based).
	/// </summary>
	public int Page { get; init; }

	/// <summary>
	/// Gets the number of items per page.
	/// </summary>
	public int PerPage { get; init; }

	/// <summary>
	/// Gets the total number of pages.
	/// </summary>
	public int TotalPages { get; init; }
}
