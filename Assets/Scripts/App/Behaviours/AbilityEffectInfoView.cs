using Loom.ZombieBattleground.Common;
using System;
using System.Collections;
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
        [Range(0, 20)]
        public float delayForChangeState;

        public string soundName;

        [Range(0, 20)]
        public float delayForSound;

        public Vector3 offset;

        public RotationEffectParam rotationParameters;
    }

    [Serializable]
    public class RotationEffectParam
    {
        public bool isOnlyForLocalPlayer;

        public Vector3 rotation;
    }
}
