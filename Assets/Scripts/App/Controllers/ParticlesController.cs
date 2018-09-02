// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
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
                    duration,
                    false);
            }

            return id;
        }

        public void DestoryParticle(ulong id)
        {
            ParticleSystemElement element = _particleSystemElements.Find(x => x.id == id);

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
        public ulong id;

        public GameObject particleObject;

        public ParticleSystemElement(ulong id, GameObject particleObject)
        {
            this.id = id;
            this.particleObject = particleObject;
        }

        public void Dispose()
        {
            if ((particleObject != null) && particleObject)
            {
                Object.Destroy(particleObject);
            }
        }
    }
}
