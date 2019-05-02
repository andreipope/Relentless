using System;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data {
    internal class MouldIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            MouldId mouldId = (MouldId) value;
            serializer.Serialize(writer, mouldId.Id);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long mouldId = serializer.Deserialize<long>(reader);
            return new MouldId(mouldId);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MouldId);
        }
    }
}