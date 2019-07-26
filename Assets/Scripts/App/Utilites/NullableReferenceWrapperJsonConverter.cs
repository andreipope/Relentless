using System;
using Loom.Client;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class NullableReferenceWrapperJsonConverter<T> : JsonConverter where T : class
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Wrapper? wrapper = serializer.Deserialize<Wrapper?>(reader);
            return wrapper?.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            T valueReference = (T) value;
            Wrapper? wrapper = valueReference == null ? (Wrapper?) null : new Wrapper { Value = valueReference };
            serializer.Serialize(writer, wrapper);
        }

        private struct Wrapper
        {
            [JsonProperty("value")]
            public T Value;
        }
    }
}
