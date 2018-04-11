using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class DamageTargetAdjustmentsAbility : AbilityBase
    {
        public int value = 1;

        public DamageTargetAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Debug.Log(_vfxObject);
                switch (affectObjectType)
                {
                    case Enumerators.AffectObjectType.PLAYER:
                        if (targetPlayer.playerInfo.netId == cardCaller.netId)
                            cardCaller.FightPlayerBySkill(value, false);
                        else
                            cardCaller.FightPlayerBySkill(value);
                        CreateVFX(targetPlayer.transform.position);
                        break;
                    case Enumerators.AffectObjectType.CHARACTER:
                        Action(targetCreature);

                        cardCaller.FightCreatureBySkill(value, targetCreature.card);
                        CreateVFX(targetCreature.transform.position);
                        break;
                    default: break;
                }

                
            }
        }
        public override void Action(object info = null)
        {
            base.Action(info);

            var creature = info as BoardCreature;

            BoardCreature leftAdjustment = null,
                    rightAdjastment = null;

            int targetIndex = -1;
            List<BoardCreature> list = null;
            for (int i = 0; i < cardCaller.opponentBoardCardsList.Count; i++)
            {
                if (cardCaller.opponentBoardCardsList[i] == creature)
                {
                    targetIndex = i;
                    list = cardCaller.opponentBoardCardsList;
                    break;
                }
            }
            if(targetIndex == -1)
            for (int i = 0; i < cardCaller.playerBoardCardsList.Count; i++)
            {
                if (cardCaller.playerBoardCardsList[i] == creature)
                {
                    targetIndex = i;
                    list = cardCaller.playerBoardCardsList;
                    break;
                }
            }
            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                    leftAdjustment = list[targetIndex - 1];
                if (targetIndex + 1 < list.Count)
                    rightAdjastment = list[targetIndex + 1];
            }

            if (leftAdjustment != null)
            {
                cardCaller.FightCreatureBySkill(value, leftAdjustment.card);
                CreateVFX(leftAdjustment.transform.position);
            }

            if (rightAdjastment != null)
            {
                cardCaller.FightCreatureBySkill(value, rightAdjastment.card);
                CreateVFX(rightAdjastment.transform.position);
            }
        }
    }
}