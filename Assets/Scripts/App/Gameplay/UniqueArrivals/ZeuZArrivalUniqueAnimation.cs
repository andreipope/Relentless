using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;
using DG.Tweening;

namespace Loom.ZombieBattleground
{
    public class ZeuZArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(BoardObject boardObject, Action startGeneralArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(0, 0.89f, 0f);

            const float delayBeforeSpawn = 0.7f;
            const float delayBeforeDestroyVFX = 5f;

            BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel(boardObject as BoardUnitModel);


           

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/ZeuZ"));

                animationVFX.transform.position = unitView.PositionOfBoard + offset;

                InternalTools.DoActionDelayed(() =>
                {
                    unitView.Transform.SetParent(animationVFX.transform.Find("Zeus_Card_PH"), true);
                    unitView.Transform.localScale = Vector3.one * 5.65f;
                }, Time.deltaTime);
                //unitView.Transform.localScale = Vector3.one * 5.65f;
                //unitView.Transform.localPosition = new Vector3(0, -2.54f, 0);

                InternalTools.DoActionDelayed(() =>
                {
                    
                    unitView.Transform.SetParent(null, true);
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
