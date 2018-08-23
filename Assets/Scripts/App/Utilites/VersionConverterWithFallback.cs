using System;
using Loom.Newtonsoft.Json;
using Loom.Newtonsoft.Json.Converters;

namespace LoomNetwork.CZB
{
    public class VersionConverterWithFallback : VersionConverter
    {
        public Version FallbackVersion { get; }

        public VersionConverterWithFallback(Version fallbackVersion)
        {
            FallbackVersion = fallbackVersion;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            } catch (JsonSerializationException e)
            {
                return FallbackVersion;
            }
        }
    }
}
