using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Loom.ZombieBattleground
{
    public class VersionConverterWithFallback : VersionConverter
    {
        public VersionConverterWithFallback(Version fallbackVersion)
        {
            FallbackVersion = fallbackVersion;
        }

        public Version FallbackVersion { get; }

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch (JsonSerializationException e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                return FallbackVersion;
            }
        }
    }
}
