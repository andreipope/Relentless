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

        [Range(0, 20)]
        public float delayForSound;

        public Vector3 offset;

        public Vector3 localOffset;

        public bool isRotate = false;

        public Vector3 localPlayerAbilityEffectRotation = Vector3.zero;
        public Vector3 opponentPlayerAbilityEffectRotation = Vector3.zero;

        public AbilityInfoPositionBlock positionInfo;
    }

    [Serializable]
    public class AbilityInfoPositionBlock
    {
        public Enumerators.AbilityEffectInfoPositionType type;

    }
}
