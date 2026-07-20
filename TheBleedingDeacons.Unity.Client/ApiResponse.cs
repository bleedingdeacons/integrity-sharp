namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// API response wrapper with success status, data, and metadata.
/// </summary>
public sealed class ApiResponse<T> where T : class
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public ApiError? Error { get; init; }

    public ResponseMeta? Meta { get; init; }

    public int StatusCode { get; init; }

    public RateLimitInfo? RateLimit { get; init; }
}
