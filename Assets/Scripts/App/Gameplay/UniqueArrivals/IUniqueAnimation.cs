using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;

namespace Loom.ZombieBattleground
{
    public class UniqueAnimation
    {
        public bool IsPlaying { get; protected set; }

        protected ILoadObjectsManager LoadObjectsManager;
        protected IGameplayManager GameplayManager;
        protected ISoundManager SoundManager;
        protected ICameraManager CameraManager;

        protected BattlegroundController BattlegroundController;
        protected BoardController BoardController;
        protected CardsController CardsController;

        public UniqueAnimation()
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();
            SoundManager = GameClient.Get<ISoundManager>();
            CameraManager = GameClient.Get<ICameraManager>();

            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            CardsController = GameplayManager.GetController<CardsController>();
            BoardController = GameplayManager.GetController<BoardController>();
        }

        public virtual void Play() { }

        public virtual void Play(IBoardObject boardObject) { }
        public virtual void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback) { }

        public virtual void PlaySound(string clipTitle)
        {
            SoundManager.PlaySound(Common.Enumerators.SoundType.UNIQUE_ARRIVALS, clipTitle, Constants.ArrivalSoundVolume, isLoop: false);
        }
    }
}
