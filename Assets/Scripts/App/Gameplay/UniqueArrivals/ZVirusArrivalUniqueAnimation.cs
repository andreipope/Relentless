using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;
using DG.Tweening;

namespace Loom.ZombieBattleground
{
    public class ZVirusArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(BoardObject boardObject)
        {
            IsPlaying = true;

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/ZVirus"));

            BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel(boardObject as BoardUnitModel);

            const float xOffsetOfCard = 5.2f;

            const float delayBeforeUnitShow = 1.5f;
            const float durationUnitScaling = 1f;
            const float delayBeforeDestroyVFX = 8f;

            animationVFX.transform.position = unitView.Transform.position + Vector3.right * xOffsetOfCard;

            unitView.Transform.localScale = Vector3.zero;

            InternalTools.DoActionDelayed(() =>
            {
                unitView.Transform.DOScale(Vector3.one, durationUnitScaling);
            }, delayBeforeUnitShow);

            InternalTools.DoActionDelayed(() =>
            {
                unitView.Transform.parent = null;

                Object.Destroy(animationVFX);

                if (unitView.Model.OwnerPlayer.IsLocalPlayer)
                {
                    BattlegroundController.UpdatePositionOfBoardUnitsOfPlayer(unitView.Model.OwnerPlayer.BoardCards);
                }
                else
                {
                    BattlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
                }

                IsPlaying = false;
            }, delayBeforeDestroyVFX);
        }
    }
}
