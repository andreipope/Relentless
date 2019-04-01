using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface ICardAbilityView
    {
        event Action VFXBegan;

        event Action VFXEnded;

        ICardAbility CardAbility { get; }

        void Init(ICardAbility cardAbility);

        void DoVFXAction(IReadOnlyList<BoardObject> targets = null, IReadOnlyList<GenericParameter> genericParameters = null);
    }

    public class CardAbilityView : ICardAbilityView
    {
        protected readonly IGameplayManager GameplayManager;

        protected readonly VfxController VfxController;

        protected readonly BoardController BoardController;

        protected readonly BattlegroundController BattlegroundController;

        public event Action VFXBegan;

        public event Action VFXEnded;

        public ICardAbility CardAbility { get; protected set; }

        public CardAbilityView()
        {
            GameplayManager = GameClient.Get<IGameplayManager>();
            VfxController = GameplayManager.GetController<VfxController>();
            BoardController = GameplayManager.GetController<BoardController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        public void Init(ICardAbility cardAbility)
        {
            CardAbility = cardAbility;
        }

        public virtual void DoVFXAction(IReadOnlyList<BoardObject> targets = null, IReadOnlyList<GenericParameter> genericParameters = null)
        {
            VFXBegan?.Invoke();
        }

        public virtual void EndVFXAction()
        {
            VFXEnded?.Invoke();
        }
    }
}
