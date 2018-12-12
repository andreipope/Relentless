using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public class UniqueAnimation
    {
        public bool IsPlaying { get; protected set; }

        protected ILoadObjectsManager LoadObjectsManager;
        protected IGameplayManager GameplayManager;
        protected ISoundManager SoundManager;

        protected BattlegroundController BattlegroundController;
        protected CardsController CardsController;

        public UniqueAnimation()
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();
            SoundManager = GameClient.Get<ISoundManager>();

            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            CardsController = GameplayManager.GetController<CardsController>();
        }

        public virtual void Play() { }

        public virtual void Play(BoardObject boardObject) { }
        public virtual void Play(BoardObject boardObject, Action startGeneralArrivalCallback) { }

        public virtual void PlaySound(string clipTitle)
        {
            SoundManager.PlaySound(Common.Enumerators.SoundType.UNIQUE_ARRIVALS, clipTitle, Constants.ArrivalSoundVolume, isLoop: false);
        }
    }
}
