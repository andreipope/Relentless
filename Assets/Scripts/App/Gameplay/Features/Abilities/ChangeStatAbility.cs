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
            Debug.Log("BAM");
            Debug.Log("!");
            //cardCaller.FightCreatureBySkill(1, boardCreature.card);
            return;
            if (statType == Enumerators.StatType.DAMAGE)
            {
                boardCreature.card.namedStats["HP"].baseValue += value;
                //boardCreature.attackStat.baseValue += value;
                if (boardCreature.attackStat.baseValue < 0)
                    boardCreature.attackStat.baseValue = 0;
            }
            else if (statType == Enumerators.StatType.HEALTH)
            {
                boardCreature.healthStat.baseValue += value;
                if (boardCreature.healthStat.baseValue < 0)
                    boardCreature.healthStat.baseValue = 0;
            }
            CreateVFX(boardCreature.transform.position);
        }
    }
}