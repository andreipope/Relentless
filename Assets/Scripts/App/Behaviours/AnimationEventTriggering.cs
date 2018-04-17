using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventTriggering : MonoBehaviour
{
    public Action<string> OnAnimationEvent;

    public void AnimationEvent(string animationName)
    {
        if (OnAnimationEvent != null)
        {
            OnAnimationEvent(animationName);
        }
    }
}
