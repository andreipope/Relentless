using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ShammannArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(BoardObject boardObject)
        {
            IsPlaying = true;

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/ZB_ANM_Shammann"));

            animationVFX.transform.position =
                        BattlegroundController.GetBoardUnitViewByModel(boardObject as BoardUnitModel).Transform.position;

            // -90, 0, -180 ???

            InternalTools.DoActionDelayed(() =>
            {
                Object.Destroy(animationVFX);

                IsPlaying = false;
            }, 1f);
        }
    }
}
