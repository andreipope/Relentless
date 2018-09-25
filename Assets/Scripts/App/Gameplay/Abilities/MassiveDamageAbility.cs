using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbility : AbilityBase
    {
        public int Value;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();
            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            object caller = AbilityUnitViewOwner ?? (object) BoardSpell;

            Player opponent = PlayerCallerOfAbility == GameplayManager.CurrentPlayer ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;
            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        foreach (BoardUnitView cardOpponent in opponent.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(caller, AbilityData, cardOpponent.Model);
                        }

                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        foreach (BoardUnitView cardPlayer in PlayerCallerOfAbility.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(caller, AbilityData, cardPlayer.Model);
                        }

                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        BattleController.AttackPlayerByAbility(caller, AbilityData, opponent);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        BattleController.AttackPlayerByAbility(caller, AbilityData, PlayerCallerOfAbility);
                        break;
                }
            }

            InvokeActionTriggered();
        }
    }
}
