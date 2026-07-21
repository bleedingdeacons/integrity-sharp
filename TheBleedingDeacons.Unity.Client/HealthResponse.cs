// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Health check response.
/// </summary>
public sealed class HealthResponse
{
	/// <summary>
	/// Gets the overall health status reported by the API (e.g. "ok").
	/// </summary>
	public required string Status { get; init; }

	/// <summary>
	/// Gets the timestamp at which the health check was produced.
	/// </summary>
	public required string Timestamp { get; init; }

	/// <summary>
	/// Gets the Integrity API version.
	/// </summary>
	public required string Version { get; init; }

	/// <summary>
	/// Gets a value indicating whether the Unity plugin is available.
	/// </summary>
	public bool UnityAvailable { get; init; }
}
