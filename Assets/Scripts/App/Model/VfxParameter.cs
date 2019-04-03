
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class VfxParameter
    {
        [JsonProperty]
        public Enumerators.AbilityEffectType EffectType { get; private set; }

        [JsonProperty]
        public List<VfxParameterInfo> Parameters { get; private set; }

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
            public Enumerators.AbilityEffectParameter EffectParameter { get; private set; }

            [JsonProperty]
            public object Value { get; private set; }

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
