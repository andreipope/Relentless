using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.SetType setType;
        public Enumerators.StatType statType;
        public int value = 1;


        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
            }
        }

        public override void Action()
        {
            base.Action();
            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.CHARACTER:
                    var targetCardInfo = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(targetCreature.card.cardId);
                    Debug.Log("!");

                    if (statType == Enumerators.StatType.DAMAGE)
                    {
                        Debug.Log("!!");

                        targetCreature.attackStat.baseValue += value;
                        if (targetCreature.attackStat.baseValue < 0)
                            targetCreature.attackStat.baseValue = 0;
                    }
                    else if (statType == Enumerators.StatType.HEALTH)
                    {
                        targetCreature.healthStat.baseValue += value;
                        if (targetCreature.healthStat.baseValue < 0)
                            targetCreature.healthStat.baseValue = 0;
                    }
                    CreateVFX(targetCreature.transform.position);
                    break;
                default: break;
            }
        }
    }
}