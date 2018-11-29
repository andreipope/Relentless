using Loom.ZombieBattleground.Common;
using System;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AbilityEffectInfoView : MonoBehaviour
    {
        public string cardName;

        public Enumerators.CardNameOfAbility cardNameOfAbility;

        [Range(0, 20)]
        public float delayBeforeEffect;
        [Range(0, 20)]
        public float delayAfterEffect;
        [Range(0, 20)]
        public float delayForChangeState;

        public string soundName;

        public Vector3 offset;


        public AbilityInfoPositionBlock positionInfo;
    }

    [Serializable]
    public class AbilityInfoPositionBlock
    {
        public Enumerators.AbilityEffectInfoPositionType type;

    }
}
