// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Internal response structure for deserialization.
/// </summary>
/// <typeparam name="T">The type of the deserialized payload.</typeparam>
internal sealed class ApiDataResponse<T> where T : class
{
    /// <summary>
    /// Gets a value indicating whether the request succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the deserialized response payload.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets the pagination metadata for list responses, when present.
    /// </summary>
    public ResponseMeta? Meta { get; init; }
}
