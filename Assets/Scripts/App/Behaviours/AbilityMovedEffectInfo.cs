using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AbilityMovedEffectInfo : MonoBehaviour
    {
        [Range(0, 20)]
        public float delayBeforeMovedVFX;
        [Range(0, 20)]
        public float delayBeforeDestroyMovedVFX;
        public string soundName;
    }
}
