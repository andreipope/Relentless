using System;
using Loom.Client;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class AddressToLocalAddressStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Address);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string hex = ((Address) value).LocalAddress.ToLowerInvariant();
            serializer.Serialize(writer, hex);
        }
    }
}
