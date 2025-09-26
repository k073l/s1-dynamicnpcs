using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicNPCs.Helpers;

public class TupleConverter<T1, T2> : JsonConverter<(T1, T2)>
{
    public override (T1, T2) ReadJson(JsonReader reader, Type objectType, (T1, T2) existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);
        if (array.Count != 2)
            throw new JsonSerializationException($"Expected array of length 2 to deserialize tuple, got {array.Count}");

        var item1 = array[0].ToObject<T1>(serializer);
        var item2 = array[1].ToObject<T2>(serializer);

        return (item1, item2);
    }

    public override void WriteJson(JsonWriter writer, (T1, T2) value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        serializer.Serialize(writer, value.Item1);
        serializer.Serialize(writer, value.Item2);
        writer.WriteEndArray();
    }
}