// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

namespace TheBleedingDeacons.Unity.Client;

/// <summary>
/// Thrown when a REST API request fails after exhausting all retry attempts for transient failures
/// (network errors, timeouts, 5xx, 408, 429, or WAF-style HTML 403 responses).
/// </summary>
public sealed class RestApiRequestFailed : Exception
{
    /// <summary>HTTP method of the failed request (e.g. "GET", "POST").</summary>
    public string Method { get; }

    /// <summary>The full request URL.</summary>
    public string Url { get; }

    /// <summary>Number of attempts made before giving up.</summary>
    public int Attempts { get; }

    /// <summary>Last HTTP status code observed, if any (null if all attempts failed before a response was received).</summary>
    public int? LastStatusCode { get; }

    /// <summary>Human-readable reason for the final failure (e.g. "HTTP 503", "network error: ...", "request timeout").</summary>
    public string Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RestApiRequestFailed"/> class.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="attempts"></param>
    /// <param name="lastStatusCode"></param>
    /// <param name="reason"></param>
    /// <param name="innerException"></param>
    public RestApiRequestFailed(string method, string url, int attempts, int? lastStatusCode, string reason, Exception? innerException = null)
        : base(BuildMessage(method, url, attempts, lastStatusCode, reason), innerException)
    {
        Method = method;
        Url = url;
        Attempts = attempts;
        LastStatusCode = lastStatusCode;
        Reason = reason;
    }

    private static string BuildMessage(string method, string url, int attempts, int? lastStatusCode, string reason)
    {
        var status = lastStatusCode.HasValue ? $" (last status: HTTP {lastStatusCode.Value})" : string.Empty;
        return $"{method} {url} failed after {attempts} attempt(s): {reason}{status}.";
    }
}
