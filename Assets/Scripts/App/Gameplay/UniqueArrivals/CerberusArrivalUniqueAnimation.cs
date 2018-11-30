using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;
using DG.Tweening;

namespace Loom.ZombieBattleground
{
    public class CerberusArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(BoardObject boardObject, Action startGeneralArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(0f, 0.3f, 0f);

            const float delayBeforeSpawn = 0.7f;
            const float delayBeforeDestroyVFX = 3f;

            BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel(boardObject as BoardUnitModel);

            unitView.GameObject.SetActive(false);

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/Cerberus_Arrival"), unitView.Transform, false);

                animationVFX.transform.position = unitView.PositionOfBoard + offset;

                unitView.GameObject.SetActive(true);

                InternalTools.DoActionDelayed(() =>
                {
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
            }, delayBeforeSpawn);
        }
    }
}
