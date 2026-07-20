namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Health check response.
/// </summary>
public sealed class HealthResponse
{
    public required string Status { get; init; }

    public required string Timestamp { get; init; }

    public required string Version { get; init; }

    public bool UnityAvailable { get; init; }
}
