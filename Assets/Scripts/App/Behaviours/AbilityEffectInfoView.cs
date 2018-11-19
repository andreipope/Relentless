using Loom.ZombieBattleground.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AbilityEffectInfoView : MonoBehaviour
    {
        public string cardName;

        [Range(0, 20)]
        public float delayBeforeEffect;
        [Range(0, 20)]
        public float delayAfterEffect;

        public string soundName;

        public Vector3 offset;
    }
}
