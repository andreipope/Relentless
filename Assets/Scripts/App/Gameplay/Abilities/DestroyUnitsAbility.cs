using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitsAbility : AbilityBase
    {
        public DestroyUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnitModel> units = new List<BoardUnitModel>();

            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        units.AddRange(GetOpponentOverlord().BoardCards.Select(x => x.Model));
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        units.AddRange(PlayerCallerOfAbility.BoardCards.Select(x => x.Model));
                        break;
                }
            }

            units.ForEach(BattlegroundController.DestroyBoardUnit);
        }
    }
}
