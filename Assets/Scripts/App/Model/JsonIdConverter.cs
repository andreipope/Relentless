using System;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data {
    internal class JsonIdConverter<TId, TIdValue> : JsonConverter where TIdValue : struct where TId : struct, IId<TIdValue>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TId id = (TId) value;
            serializer.Serialize(writer, id.Id);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TIdValue idValue = serializer.Deserialize<TIdValue>(reader);
            return (TId) Activator.CreateInstance(typeof(TId), idValue);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TId);
        }
    }
}
