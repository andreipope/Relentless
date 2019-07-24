using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class TzunamyArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(-1.5f, -2.5f);

            const float delayBeforeSpawn = 0f;
            const float delayBeforeDestroyVFX = 8.5f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);
            unitView.GameObject.SetActive(false);

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/Tzunamy_Arrival"));
                animationVFX.transform.position = unitView.PositionOfBoard + offset;

                Transform cameraVFXObj = animationVFX.transform.Find("!!NULL camera shake");
                Transform cameraGroupTransform = CameraManager.GetGameplayCameras();
                cameraGroupTransform.SetParent(cameraVFXObj);

                PlaySound("ZB_AUD_Tzunamy_F1_EXP");

                Vector3 cameraLocalPosition = animationVFX.transform.position * -1;
                cameraGroupTransform.localPosition = cameraLocalPosition;

                InternalTools.DoActionDelayed(() =>
                {
                    cameraGroupTransform.SetParent(null);
                    cameraGroupTransform.position = Vector3.zero;
                    
                    Object.Destroy(animationVFX);
                    
                    if (unitView != null)
                    {
                        unitView.GameObject.SetActive(true);
                        unitView.battleframeAnimator.Play(0, -1, 1);
                        foreach (Transform child in unitView.battleframeAnimator.transform)
                        {
                            if (child.name == "ScrapFlies")
                            {
                                child.gameObject.SetActive(false);
                                break;
                            }
                        }
                        BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);
                    }

                    endArrivalCallback?.Invoke();

                    IsPlaying = false;
                }, delayBeforeDestroyVFX);
            }, delayBeforeSpawn);
        }
    }
}
