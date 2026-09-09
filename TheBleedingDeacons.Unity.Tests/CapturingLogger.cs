using Microsoft.Extensions.Logging;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// Keeps every formatted log message so a test can assert on what was
/// written. Only the client's own warnings are interesting here — chiefly
/// that a long error body is truncated before it reaches a log sink.
/// </summary>
internal sealed class CapturingLogger<T> : ILogger<T>
{
	private readonly List<string> _messages = [];

	public IReadOnlyList<string> Messages => _messages.AsReadOnly();

	public IDisposable? BeginScope<TState>(TState state)
		where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		ArgumentNullException.ThrowIfNull(formatter);

		_messages.Add(formatter(state, exception));
	}
}
