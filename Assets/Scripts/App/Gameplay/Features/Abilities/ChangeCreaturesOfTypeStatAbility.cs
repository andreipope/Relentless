using CCGKit;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using GrandDevs.Internal;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class ChangeCreaturesOfTypeStatAbility : AbilityBase
    {
        public Enumerators.SetType setType;
        public Enumerators.StatType statType;
        public int value = 1;


        public ChangeCreaturesOfTypeStatAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.setType = Utilites.CastStringTuEnum<Enumerators.SetType>(ability.setType);
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (statType)
            {
                case Enumerators.StatType.HEALTH:
                case Enumerators.StatType.DAMAGE:
                default:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/HealVFX");
                    break;
            }

            if (abilityCallType != Enumerators.AbilityCallType.PERMANENT)
                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private void Action()
        {
            var creaturesOnBoard = playerCallerOfAbility.playerBoardCardsList.
                 FindAll(x => _dataManager.CachedCardsLibraryData.GetCard(x.card.cardId).cardSetType.Equals(setType));

            foreach (var creature in creaturesOnBoard)
            {
                if (creature.Equals(boardCreature))
                    continue;

                switch (statType)
                {
                    case Enumerators.StatType.DAMAGE:
                        creature.attackStat.AddModifier(new Modifier(value));
                        break;
                    case Enumerators.StatType.HEALTH:
                        creature.healthStat.AddModifier(new Modifier(value));
                        break;
                    default: break;
                }

                CreateVFX(creature.transform.position, true);
            }
        }
    }
}