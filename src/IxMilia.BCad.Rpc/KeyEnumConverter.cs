using System;
using IxMilia.BCad.Commands;
using Newtonsoft.Json;

namespace IxMilia.BCad.Rpc
{
    public class KeyEnumConverter : JsonConverter<Key>
    {
        public override Key ReadJson(JsonReader reader, Type objectType, Key existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, Key value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
