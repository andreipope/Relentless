using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Loom.ZombieBattleground
{

    public abstract class AbilityViewBase
    {
        protected ILoadObjectsManager LoadObjectsManager;

        protected IGameplayManager GameplayManager;

        protected ParticlesController ParticlesController;

        protected AbilityBase Ability;


        protected GameObject VfxObject;

        protected List<ulong> ParticleIds;

        public AbilityViewBase(AbilityBase ability)
        {
            Ability = ability;
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();
            ParticlesController = GameplayManager.GetController<ParticlesController>();
            ParticleIds = new List<ulong>();

            Ability.Disposed += Dispose;
            Ability.ActionTriggered += OnAbilityAction;
        }

        protected virtual void CreateVfx(
            Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            // todo make it async
            if (VfxObject != null)
            {
                VfxObject = Object.Instantiate(VfxObject);

                if (!justPosition)
                {
                    VfxObject.transform.position = pos - Constants.VfxOffset + Vector3.forward;
                }
                else
                {
                    VfxObject.transform.position = pos;
                }

                ulong id = ParticlesController.RegisterParticleSystem(VfxObject, autoDestroy, duration);

                if (!autoDestroy)
                {
                    ParticleIds.Add(id);
                }
            }
        }

        protected virtual void OnAbilityAction(object info = null)
        {

        }

        protected virtual void Dispose()
        {
            Ability.Disposed -= Dispose;
            Ability.ActionTriggered -= OnAbilityAction;
        }

        protected void ClearParticles()
        {
            foreach (ulong id in ParticleIds)
            {
                ParticlesController.DestroyParticle(id);
            }
        }
    }

    public abstract class AbilityViewBase<TAbility> : AbilityViewBase where TAbility : AbilityBase
    {
        protected new TAbility Ability;

        protected AbilityViewBase(TAbility ability) : base(ability)
        {
            Ability = ability;
        }
    }
}
