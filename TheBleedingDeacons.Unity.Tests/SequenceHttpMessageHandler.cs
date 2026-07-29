using System.Net;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// A mock handler that returns a queued sequence of responses, one per call, so
/// the retry path can be exercised (e.g. a transient failure followed by a
/// success). Once the queue is exhausted the last response is repeated.
/// </summary>
internal sealed class SequenceHttpMessageHandler : HttpMessageHandler
{
	private readonly Queue<(HttpStatusCode Status, string Body, IReadOnlyDictionary<string, string>? Headers)> _responses = new();

	public int CallCount { get; private set; }

	public void Enqueue(HttpStatusCode status, string body, IReadOnlyDictionary<string, string>? headers = null)
		=> _responses.Enqueue((status, body, headers));

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		CallCount++;

		var (status, body, headers) = _responses.Count > 1 ? _responses.Dequeue() : _responses.Peek();

		var response = new HttpResponseMessage(status)
		{
			Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
			RequestMessage = request,
		};

		if (headers != null)
		{
			foreach (var header in headers)
				response.Headers.TryAddWithoutValidation(header.Key, header.Value);
		}

		return Task.FromResult(response);
	}
}
