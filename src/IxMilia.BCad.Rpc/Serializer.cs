using Newtonsoft.Json;

namespace IxMilia.BCad.Rpc
{
    public static class Serializer
    {
        public static void PrepareSerializer(JsonSerializer serializer)
        {
            serializer.Converters.Add(new CadEnumStringConverter());
        }
    }
}
