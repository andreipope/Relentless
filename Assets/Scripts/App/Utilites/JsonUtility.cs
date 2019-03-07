using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    }
}
