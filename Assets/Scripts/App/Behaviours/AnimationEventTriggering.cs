using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Loom.ZombieBattleground
{
    public class AnimationEventTriggering : MonoBehaviour
    {
        [FormerlySerializedAs("OnAnimationEvent")]
        public Action<string> AnimationEventTriggered;

        public void AnimationEvent(string animationName)
        {
            AnimationEventTriggered?.Invoke(animationName);
        }
    }
}
