using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ChernoBillArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(-0.25f, 3.21f, 0f);

            const float delayBeforeSpawn = 5f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            unitView.GameObject.SetActive(false);

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/ChernoBillArrival"));

            PlaySound("CZB_AUD_Cherno_Bill_Arrival_F1_EXP");

            animationVFX.transform.position = unitView.PositionOfBoard + offset;

            InternalTools.DoActionDelayed(() =>
            {
                unitView.GameObject.SetActive(true);
                unitView.battleframeAnimator.Play(0, -1, 1);

                Object.Destroy(animationVFX);

                endArrivalCallback?.Invoke();

                BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);

                IsPlaying = false;

            }, delayBeforeSpawn);
        }
    }
}
