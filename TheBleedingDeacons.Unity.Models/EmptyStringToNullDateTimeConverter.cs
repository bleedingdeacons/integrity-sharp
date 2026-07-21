// Copyright (c) The Bleeding Deacons. Licensed under the MIT license.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheBleedingDeacons.Unity.Models
{
	/// <summary>
	/// Converts JSON string values to <see cref="DateTime?"/>, treating empty
	/// strings as <c>null</c> instead of throwing a deserialization error.
	///
	/// The Unity API returns <c>""</c> for the <c>updated</c> field when a
	/// post has no modification timestamp. System.Text.Json cannot convert an
	/// empty string to <see cref="DateTime?"/> by default, so this converter
	/// bridges the gap.
	/// </summary>
	public sealed class EmptyStringToNullDateTimeConverter : JsonConverter<DateTime?>
	{
		/// <summary>
		/// Reads a JSON value and converts it to a nullable <see cref="DateTime"/>,
		/// treating null and empty/whitespace strings as <see langword="null"/>.
		/// </summary>
		public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				return null;
			}

			if (reader.TokenType == JsonTokenType.String)
			{
				var value = reader.GetString();

				if (string.IsNullOrWhiteSpace(value))
				{
					return null;
				}

				if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
						System.Globalization.DateTimeStyles.RoundtripKind, out var result))
				{
					return result;
				}

				return null;
			}

			throw new JsonException($"Unexpected token type {reader.TokenType} for DateTime?.");
		}

		/// <summary>
		/// Writes a nullable <see cref="DateTime"/> as a round-trippable UTC string, or a JSON null.
		/// </summary>
		public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
			{
				writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture));
			}
			else
			{
				writer.WriteNullValue();
			}
		}
	}
}
