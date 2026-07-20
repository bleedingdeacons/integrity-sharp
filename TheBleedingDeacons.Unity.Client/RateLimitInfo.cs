namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Rate limit information from response headers.
/// </summary>
public sealed class RateLimitInfo
{
    public int Limit { get; init; }

    public int Remaining { get; init; }

    public long Reset { get; init; }

    public DateTime ResetDateTime => DateTimeOffset.FromUnixTimeSeconds(Reset).LocalDateTime;
}
