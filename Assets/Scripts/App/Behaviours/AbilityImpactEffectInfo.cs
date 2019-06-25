using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AbilityImpactEffectInfo : MonoBehaviour
    {
        [Range(0, 20)]
        public float delayAfterImpactVFX;
        [Range(0, 20)]
        public float delayBeforeDestroyImpactVFX;
        public string soundName;

        public AbilityImpactEffectInfo()
        {
            delayAfterImpactVFX = 0;
            delayBeforeDestroyImpactVFX = 0;
        }
    }
}
