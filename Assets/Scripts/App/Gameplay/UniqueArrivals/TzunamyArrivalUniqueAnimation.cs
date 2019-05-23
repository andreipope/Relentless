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
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = Vector3.zero;

            const float delayBeforeSpawn = 0f;
            const float delayBeforeDestroyVFX = 5f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/Tzunamy_Arrival"));

                PlaySound("ZB_AUD_Tzunamy_F1_EXP");

                InternalTools.DoActionDelayed(() =>
                {
                    Object.Destroy(animationVFX);

                    endArrivalCallback?.Invoke();
                    BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);

                    IsPlaying = false;
                }, delayBeforeDestroyVFX);
            }, delayBeforeSpawn);
        }
    }
}
