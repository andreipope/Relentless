using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ShammannArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(BoardObject boardObject, Action startGeneralArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/ZB_ANM_Shammann"));

            BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel(boardObject as BoardUnitModel);

            const float yOffsetOfCard = -0.75f;

            animationVFX.transform.position = unitView.PositionOfBoard;
            unitView.Transform.SetParent(animationVFX.transform.Find("Shaman/Main_Model/Root"));
            unitView.Transform.localPosition = new Vector3(0, yOffsetOfCard, 0);

            InternalTools.DoActionDelayed(() =>
            {
                unitView.Transform.parent = null;

                Object.Destroy(animationVFX);

                if(unitView.Model.OwnerPlayer.IsLocalPlayer)
                {
                    BattlegroundController.UpdatePositionOfBoardUnitsOfPlayer(unitView.Model.OwnerPlayer.BoardCards);
                }
                else
                {
                    BattlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
                }

                IsPlaying = false;
            }, 3f);
        }
    }
}
