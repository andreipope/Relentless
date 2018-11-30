using System.Collections.Generic;
using UnityEngine;
using System;

namespace Loom.ZombieBattleground
{
    public class UniqueAnimation
    {
        public bool IsPlaying { get; protected set; }

        protected ILoadObjectsManager LoadObjectsManager;
        protected IGameplayManager GameplayManager;

        protected BattlegroundController BattlegroundController;
        protected CardsController CardsController;

        public UniqueAnimation()
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();

            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            CardsController = GameplayManager.GetController<CardsController>();
        }

        public virtual void Play() { }

        public virtual void Play(BoardObject boardObject) { }
        public virtual void Play(BoardObject boardObject, Action startGeneralArrivalCallback) { }
    }
}
