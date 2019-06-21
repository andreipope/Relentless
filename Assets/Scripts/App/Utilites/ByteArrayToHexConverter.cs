using System;
using Loom.Client;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class ByteArrayToHexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string hex = serializer.Deserialize<string>(reader);
                if (!String.IsNullOrEmpty(hex))
                {
                    return CryptoUtils.HexStringToBytes(hex);
                }
            }

            throw new InvalidOperationException("Unsupported token type " + reader.TokenType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string hex = CryptoUtils.BytesToHexString((byte[]) value);
            serializer.Serialize(writer, hex);
        }
    }
}
