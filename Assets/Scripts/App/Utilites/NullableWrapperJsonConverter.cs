using System;
using Loom.Client;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class NullableWrapperJsonConverter<T> : JsonConverter where T : struct
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Wrapper? wrapper = serializer.Deserialize<Wrapper?>(reader);
            return wrapper?.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            T? valueNullable = (T?) value;
            Wrapper? wrapper = valueNullable == null ? (Wrapper?) null : new Wrapper { Value = valueNullable.Value };
            serializer.Serialize(writer, wrapper);
        }

        private struct Wrapper
        {
            [JsonProperty("value")]
            public T Value;
        }
    }
}
