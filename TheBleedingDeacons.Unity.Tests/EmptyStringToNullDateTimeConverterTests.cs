using System.Text.Json;
using TheBleedingDeacons.Unity.Models;
using Xunit;

namespace TheBleedingDeacons.Unity.Tests;

/// <summary>
/// Covers <see cref="EmptyStringToNullDateTimeConverter"/>'s read/write paths:
/// null, empty and whitespace strings map to null; valid dates parse; invalid
/// strings fall back to null; and a non-string token throws.
/// </summary>
public class EmptyStringToNullDateTimeConverterTests
{
	private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

	private sealed class Holder
	{
		[System.Text.Json.Serialization.JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
		public DateTime? Value { get; init; }
	}

	private static DateTime? Read(string jsonValue)
		=> JsonSerializer.Deserialize<Holder>($$"""{"value":{{jsonValue}}}""", Options)?.Value;

	[Fact]
	public void Read_Null_ReturnsNull() => Assert.Null(Read("null"));

	[Fact]
	public void Read_EmptyString_ReturnsNull() => Assert.Null(Read("\"\""));

	[Fact]
	public void Read_WhitespaceString_ReturnsNull() => Assert.Null(Read("\"   \""));

	[Fact]
	public void Read_InvalidDateString_ReturnsNull() => Assert.Null(Read("\"not-a-date\""));

	[Fact]
	public void Read_ValidDateString_ReturnsDate()
	{
		var value = Read("\"2026-01-02T03:04:05Z\"");

		Assert.NotNull(value);
		Assert.Equal(2026, value!.Value.Year);
		Assert.Equal(1, value.Value.Month);
	}

	[Fact]
	public void Read_NonStringToken_Throws()
	{
		Assert.Throws<JsonException>(() => Read("12345"));
	}

	[Fact]
	public void Write_Value_EmitsRoundTrippableString()
	{
		var json = JsonSerializer.Serialize(new Holder { Value = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc) });

		Assert.Contains("2026-01-02T03:04:05", json, StringComparison.Ordinal);
	}

	[Fact]
	public void Write_Null_EmitsJsonNull()
	{
		var json = JsonSerializer.Serialize(new Holder { Value = null });

		Assert.Equal("""{"Value":null}""", json);
	}
}
