using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Loom.ZombieBattleground
{
    public static class JsonUtility
    {
        public static JsonSerializerSettings CreateStrictSerializerSettings(EventHandler<ErrorEventArgs> errorHandler)
        {
            return new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                Converters =
                {
                    new StringEnumConverter()
                },
                CheckAdditionalContent = true,
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = errorHandler
            };
        }

        public static JsonSerializerSettings CreateProtobufSerializerSettings(EventHandler<ErrorEventArgs> errorHandler)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            };

            return new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                Converters =
                {
                    new StringEnumConverter(),
                    new WholeValueDecimalJsonConverter()
                },
                ContractResolver = contractResolver,
                CheckAdditionalContent = true,
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = errorHandler,
            };
        }

        public static string PrettyPrint(string json)
        {
            return JToken.Parse(json).ToString(Formatting.Indented);
        }

        private class WholeValueDecimalJsonConverter : JsonConverter
        {
            public override bool CanRead => false;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(decimal) || objectType == typeof(float) || objectType == typeof(double);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (IsWholeValue(value))
                {
                    writer.WriteRawValue(JsonConvert.ToString(Convert.ToInt64(value)));
                }
                else
                {
                    writer.WriteRawValue(JsonConvert.ToString(value));
                }
            }

            private static bool IsWholeValue(object value)
            {
                switch (value)
                {
                    case decimal decimalValue:
                    {
                        int precision = (Decimal.GetBits(decimalValue)[3] >> 16) & 0x000000FF;
                        return precision == 0;
                    }
                    case float floatValue:
                    {
                        double doubleValue = floatValue;
                        return doubleValue == Math.Truncate(doubleValue);
                    }
                    case double doubleValue:
                    {
                        return doubleValue == Math.Truncate(doubleValue);
                    }
                    default:
                        return false;
                }
            }
        }
    }
}
