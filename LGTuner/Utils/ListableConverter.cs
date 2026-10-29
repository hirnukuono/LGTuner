using System.Text.Json;
using System.Text.Json.Serialization;

namespace LGTuner.Utils
{
    public class ListableConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = new List<T>();
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    var item = JsonSerializer.Deserialize<T>(ref reader, options);
                    list.Add(item!);
                }
            }
            else if (reader.TokenType != JsonTokenType.Null)
            {
                var item = JsonSerializer.Deserialize<T>(ref reader, options);
                list.Add(item!);
            }
            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }
            writer.WriteEndArray();
        }
    }
}
