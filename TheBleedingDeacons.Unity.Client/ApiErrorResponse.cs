namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Internal error response structure for deserialization.
/// </summary>
internal sealed class ApiErrorResponse
{
    public bool Success { get; init; }

    public ApiError? Error { get; init; }
}
