using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class PutUnitsFromLibraryIntoPlayAbility : AbilityBase
    {
        public int Count { get; }

        public int Cost { get; }

        public PutUnitsFromLibraryIntoPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
            Cost = ability.Cost;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<TargetCardInfo> targets = new List<TargetCardInfo>();
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            Player playerOwner = null;

            foreach (Enumerators.Target targetType in AbilityData.Targets)
            {
                switch (targetType)
                {
                    case Enumerators.Target.PLAYER:
                        playerOwner = PlayerCallerOfAbility;
                        break;
                    case Enumerators.Target.OPPONENT:
                        playerOwner = GetOpponentOverlord();
                        break;
                }

                List<Card> elements = DataManager.CachedCardsLibraryData.Cards.ToList();

                elements = elements.FindAll(item => item.Faction != Enumerators.Faction.ITEM);

                if(Cost > 0)
                {
                    elements = elements.FindAll(item => item.Cost == Cost);
                }

                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
                {
                    elements = GetRandomElements(elements, Count);
                }

                if (HasEmptySpaceOnBoard(playerOwner, out int emptyFields) && elements.Count > 0)
                {
                    for (int i = 0; i < emptyFields; i++)
                    {
                        if (i >= elements.Count)
                            break;

                        targets.Add(new TargetCardInfo()
                        {
                            Name = elements[i].Name,
                            Owner = playerOwner
                        });
                    }
                }
            }

            if (targets.Count > 0)
            {
                foreach (TargetCardInfo target in targets)
                {
                    PutCardOnBoard(target.Owner, target.Name, ref targetEffects);
                }

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = GetCaller(),
                    TargetEffects = targetEffects
                });
            }
        }

        private void PutCardOnBoard(Player owner, string name, ref List<PastActionsPopup.TargetEffectParam> targetEffects)
        {
            BoardUnitView boardUnitView = owner.PlayerCardsController.SpawnUnitOnBoard(name, ItemPosition.End, IsPVPAbility);

            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                Target = boardUnitView.Model
            });
        }

        class TargetCardInfo
        {
            public string Name;
            public Player Owner;
        }
    }
}
