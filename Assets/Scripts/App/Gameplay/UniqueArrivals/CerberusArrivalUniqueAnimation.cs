using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;
using DG.Tweening;
using UnityEngine.Rendering;

namespace Loom.ZombieBattleground
{
    public class CerberusArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(0f, 0.3f, 0f);

            const float delayBeforeSpawn = 0.7f;
            const float delayBeforeDestroyVFX = 3f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);
            SortingGroup unitSortingGroup = unitView.GameObject.GetComponent<SortingGroup>();
            unitSortingGroup.enabled = false;
            unitView.GameObject.SetActive(false);

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/Cerberus_Arrival"), unitView.Transform, false);

                PlaySound("ZB_AUD_CerberusArrival_F1_EXP");

                animationVFX.transform.position = unitView.PositionOfBoard + offset;

                unitView.GameObject.SetActive(true);

                InternalTools.DoActionDelayed(() =>
                {
                    unitSortingGroup.enabled = true;
                    Object.Destroy(animationVFX);

                    endArrivalCallback?.Invoke();

                    BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);

                    IsPlaying = false;
                }, delayBeforeDestroyVFX);
            }, delayBeforeSpawn);
        }
    }
}
