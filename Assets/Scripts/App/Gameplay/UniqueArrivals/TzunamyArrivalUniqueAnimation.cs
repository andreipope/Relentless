using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;
using DG.Tweening;

namespace Loom.ZombieBattleground
{
    public class TzunamyArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(BoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = Vector3.zero;

            const float delayBeforeSpawn = 0f;
            const float delayBeforeDestroyVFX = 5f;

            BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel(boardObject as BoardUnitModel);

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/Tzunamy_Arrival"));

                PlaySound("ZB_AUD_Tzunamy_F1_EXP");

                InternalTools.DoActionDelayed(() =>
                {
                    Object.Destroy(animationVFX);

                    endArrivalCallback?.Invoke();

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
            }, delayBeforeSpawn);
        }
    }
}
