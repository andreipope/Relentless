﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class ModificateStatAbility : AbilityBase
    {
        public Enumerators.SetType setType;
        public Enumerators.StatType statType;
        public int value = 1;


        public ModificateStatAbility(Enumerators.CardKind cardKind, AbilityData ability, Enumerators.SetType setType) : base(cardKind, ability)
        {
            //this.setType = ability.ty;
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
                switch (affectObjectType)
                {
                    case Enumerators.AffectObjectType.CHARACTER:
                        var targetCardInfo = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(targetCreature.card.cardId);

                        if (targetCardInfo.cardSetType == setType || setType == Enumerators.SetType.NONE)
                        {
                            if (statType == Enumerators.StatType.DAMAGE)
                                targetCreature.attackStat.AddModifier(new Modifier(value));
                            else if (statType == Enumerators.StatType.HEALTH)
                                targetCreature.healthStat.AddModifier(new Modifier(value));

                            CreateVFX(targetCreature.transform.position);
                        }
                        break;
                    default: break;
                }
            }
        }
    }
}