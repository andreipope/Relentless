
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class VfxParameter
    {
        [JsonProperty]
        public Enumerators.AbilityEffectType EffectType { get; protected set; }

        [JsonProperty]
        public List<VfxParameterInfo> Parameters { get; protected set; }

        [JsonConstructor]
        public VfxParameter(Enumerators.AbilityEffectType effectType, List<VfxParameterInfo> parameters)
        {
            EffectType = effectType;
            Parameters = parameters;
        }

        public VfxParameter(VfxParameter source)
        {
            EffectType = source.EffectType;
            Parameters = source.Parameters;
        }

        public class VfxParameterInfo
        {
            [JsonProperty]
            public Enumerators.AbilityEffectParameter EffectParameter { get; protected set; }

            [JsonProperty]
            public object Value { get; protected set; }

            [JsonConstructor]
            public VfxParameterInfo(Enumerators.AbilityEffectParameter effectParameter, object value)
            {
                EffectParameter = effectParameter;
                Value = value;
            }

            public VfxParameterInfo(VfxParameterInfo source)
            {
                EffectParameter = source.EffectParameter;
                Value = source.Value;
            }
        }
    }
}
