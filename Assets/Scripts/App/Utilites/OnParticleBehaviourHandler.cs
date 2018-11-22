using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Loom.ZombieBattleground
{
    public class OnParticleBehaviourHandler : MonoBehaviour
    {
        public event Action OnParticleTriggerEvent;

        public void OnParticleCollision(GameObject other)
        {
            Debug.LogError(3333);
            OnParticleTriggerEvent?.Invoke();
        }

        public void OnParticleTrigger()
        {
            if (transform.GetComponent<ParticleSystem>() == null)
                Debug.LogError(4444);
        }
    }
}
