namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// API error details.
/// </summary>
public sealed class ApiError
{
    public required string Code { get; init; }

    public required string Message { get; init; }
}
