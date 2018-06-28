using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class ParticlesController : IController
    {
        private ITimerManager _timerManager;
        private ulong _freeId = 0;
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

        public ulong RegisterParticleSystem(GameObject particle, bool autoDestroy = false, float duration = 3f)
        {
            ulong id = _freeId++;

            _particleSystemElements.Add(new ParticleSystemElement(id, particle));

            if(autoDestroy)
            {
                _timerManager.AddTimer((x) =>
                {
                    DestoryParticle(id);
                }, null, duration, false);
            }

            return id;
        }

        public void DestoryParticle(ulong id)
        {
           var element = _particleSystemElements.Find(x => x.id == id);

            if (element != null)
            {
                element.Dispose();
                _particleSystemElements.Remove(element);
            }
        }


        public void ForceDestroyParticles()
        {
            foreach (var item in _particleSystemElements)
                item.Dispose();
            _particleSystemElements.Clear();
            _freeId = 0;
        }
    }

    public class ParticleSystemElement
    {
        public ulong id;
        public GameObject particleObject;

        public ParticleSystemElement(ulong id, GameObject particleObject)
        {
            this.id = id;
            this.particleObject = particleObject;
        }

        public void Dispose()
        {
            if (particleObject != null && particleObject)
                MonoBehaviour.Destroy(particleObject);
        }
    }
}