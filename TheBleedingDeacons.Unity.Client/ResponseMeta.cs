namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Response pagination metadata.
/// </summary>
public sealed class ResponseMeta
{
    public int Total { get; init; }

    public int Page { get; init; }

    public int PerPage { get; init; }

    public int TotalPages { get; init; }
}
