using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

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

            animationVFX.transform.position = unitView.Transform.position;
            unitView.Transform.SetParent(animationVFX.transform.Find("Shaman/Main_Model/Root"));
            unitView.Transform.localPosition = new Vector3(xOffsetOfCard, 0, 0);

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
            }, 3f);
        }
    }
}
