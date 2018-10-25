using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FreezeNumberOfRandomAllyAbility : AbilityBase
    {
        public int Value { get; }

        public int Turns { get; }

        public FreezeNumberOfRandomAllyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Turns = ability.Turns;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardObject> allies = new List<BoardObject>();

            if (PredefinedTargets != null)
            {
                allies = PredefinedTargets.Select(x => x.BoardObject).ToList();
            }
            else
            {
                allies.AddRange(PlayerCallerOfAbility.BoardCards.Select(x => x.Model));
                allies.Remove(AbilityUnitOwner);
                allies.Add(PlayerCallerOfAbility);

                allies = InternalTools.GetRandomElementsFromList(allies, Value);
            }

            for (int i = 0; i < allies.Count; i++)
            {
                object ally = allies[i];
                switch (ally)
                {
                    case Player player:
                        player.Stun(Enumerators.StunType.FREEZE, Turns);
                        CreateVfx(player.AvatarObject.transform.position, true, 5f);
                        break;
                    case BoardUnitModel unit:
                        unit.Stun(Enumerators.StunType.FREEZE, Turns);
                        CreateVfx(BattlegroundController.GetBoardUnitViewByModel(unit).Transform.position, true, 5f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ally), ally, null);
                }
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, allies, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }
    }
}
