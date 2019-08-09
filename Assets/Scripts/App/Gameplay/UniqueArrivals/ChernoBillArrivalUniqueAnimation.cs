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

            const float delayBeforeSpawn = 6f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            unitView.GameObject.SetActive(false);

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/ChernoBillArrival"));                                                        

            Transform cameraVFXObj = animationVFX.transform.Find("!! Camera shake");

            Transform cameraGroupTransform = CameraManager.GetGameplayCameras();
            cameraGroupTransform.SetParent(cameraVFXObj);

            Vector3 vfxPosition = unitView.PositionOfBoard;
            animationVFX.transform.position = vfxPosition;
            animationVFX.transform.Find("DestroyAllCards/Bubble").position = Vector3.zero;

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

            }, delayBeforeSpawn);
        }
    }
}
