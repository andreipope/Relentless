using System;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class PackOpeningAnimationEventsReceiver : MonoBehaviour
    {
        public event Action CardsFlyingStarted;
        public event Action CardsFlyingEnded;

        public void OnCardsFlyingStart()
        {
            CardsFlyingStarted?.Invoke();
        }

        public void OnCardsFlyingEnd()
        {
            CardsFlyingEnded?.Invoke();
        }
    }
}
