using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;

namespace Loom.ZombieBattleground
{
    public class CerberusArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(-0.32f, 0.7f, 0f);

            const float delayBeforeSpawn = 0.7f;
            const float delayBeforeDestroyVFX = 3.6f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);
            SortingGroup unitSortingGroup = unitView.GameObject.GetComponent<SortingGroup>();
            unitSortingGroup.enabled = false;
            unitView.GameObject.SetActive(false);

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/Cerberuz_Arrival"));

                Transform cameraVFXObj = animationVFX.transform.Find("!!Null cam shake");

                Transform cameraGroupTransform = CameraManager.GetGameplayCameras();
                cameraGroupTransform.SetParent(cameraVFXObj);

                PlaySound("ZB_AUD_CerberusArrival_F1_EXP");

                animationVFX.transform.position = unitView.PositionOfBoard + offset;

                Vector3 cameraLocalPosition = animationVFX.transform.position * -1;
                cameraGroupTransform.localPosition = cameraLocalPosition;

                GameObject battleFrameParticles = unitView.Transform.Find("Walker_Arrival_VFX(Clone)/ScrapFlies").gameObject;

                InternalTools.DoActionDelayed(() =>
                {
                    unitSortingGroup.enabled = true;

                    unitView.GameObject.SetActive(true);

                    battleFrameParticles.SetActive(false);
                    unitView.battleframeAnimator.Play(0, -1, 1);

                    cameraGroupTransform.SetParent(null);
                    cameraGroupTransform.position = Vector3.zero;

                    Object.Destroy(animationVFX);

                    endArrivalCallback?.Invoke();

                    BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);

                    IsPlaying = false;
                }, delayBeforeDestroyVFX);
            }, delayBeforeSpawn);
        }
    }
}
