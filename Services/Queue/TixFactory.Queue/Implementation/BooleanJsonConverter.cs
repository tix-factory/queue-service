using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TixFactory.Queue
{
	internal class BooleanJsonConverter : JsonConverter<bool>
	{
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					var str = reader.GetString();
					switch (str)
					{
						case "true":
							return true;
						case "false":
							return false;
						default:
							throw new NotSupportedException($"{nameof(BooleanJsonConverter)}.{nameof(Read)}: \"{str}\"");
					}
				case JsonTokenType.Number:
					var num = reader.GetInt64();
					switch (num)
					{
						case 0:
							return false;
						case 1:
							return true;
						default:
							throw new NotSupportedException($"{nameof(BooleanJsonConverter)}.{nameof(Read)}: {num}");
					}
				default:
					throw new NotSupportedException($"{nameof(BooleanJsonConverter)}.{nameof(Read)}: Unsupported {nameof(JsonTokenType)} ({reader.TokenType})");
			}
		}

		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString().ToLower());
		}
	}
}
