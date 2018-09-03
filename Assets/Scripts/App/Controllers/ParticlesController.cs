using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ParticlesController : IController
    {
        private ITimerManager _timerManager;

        private ulong _freeId;

        private List<ParticleSystemElement> _particleSystemElements;

        public void Init()
        {
            _timerManager = GameClient.Get<ITimerManager>();

            _particleSystemElements = new List<ParticleSystemElement>();
        }

        public void Dispose()
        {
            ForceDestroyParticles();
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
            foreach (ParticleSystemElement item in _particleSystemElements)
            {
                item.Dispose();
            }

            _particleSystemElements.Clear();
        }

        public ulong RegisterParticleSystem(GameObject particle, bool autoDestroy = false, float duration = 3f)
        {
            ulong id = _freeId++;

            _particleSystemElements.Add(new ParticleSystemElement(id, particle));

            if (autoDestroy)
            {
                _timerManager.AddTimer(
                    x =>
                    {
                        DestoryParticle(id);
                    },
                    null,
                    duration);
            }

            return id;
        }

        public void DestoryParticle(ulong id)
        {
            ParticleSystemElement element = _particleSystemElements.Find(x => x.Id == id);

            if (element != null)
            {
                element.Dispose();
                _particleSystemElements.Remove(element);
            }
        }

        public void ForceDestroyParticles()
        {
            foreach (ParticleSystemElement item in _particleSystemElements)
            {
                item.Dispose();
            }

            _particleSystemElements.Clear();
            _freeId = 0;
        }
    }

    public class ParticleSystemElement
    {
        public ulong Id;

        public GameObject ParticleObject;

        public ParticleSystemElement(ulong id, GameObject particleObject)
        {
            Id = id;
            ParticleObject = particleObject;
        }

        public void Dispose()
        {
            if (ParticleObject != null && ParticleObject)
            {
                Object.Destroy(ParticleObject);
            }
        }
    }
}
