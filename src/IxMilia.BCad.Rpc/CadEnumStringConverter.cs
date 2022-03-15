using System;
using Newtonsoft.Json;

namespace IxMilia.BCad.Rpc
{
    public class CadEnumStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType.IsEnum && !ContractGenerator.EnumsAsNumbers.Contains(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return Enum.Parse(objectType, (string)reader.Value);
                default:
                    throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
