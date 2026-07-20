namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Internal response structure for deserialization.
/// </summary>
internal sealed class ApiDataResponse<T> where T : class
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public ResponseMeta? Meta { get; init; }
}
