using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ShammannArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/ZB_ANM_Shammann"));

            PlaySound("Shammann");

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            const float yOffsetOfCard = -0.75f;

            animationVFX.transform.position = unitView.PositionOfBoard;
            unitView.Transform.SetParent(animationVFX.transform.Find("Shaman/Main_Model/Root"));
            unitView.Transform.localPosition = new Vector3(0, yOffsetOfCard, 0);

            InternalTools.DoActionDelayed(() =>
            {
                unitView.Transform.parent = null;

                Object.Destroy(animationVFX);

                endArrivalCallback?.Invoke();

                BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);
                IsPlaying = false;
            }, 3f);
        }
    }
}
