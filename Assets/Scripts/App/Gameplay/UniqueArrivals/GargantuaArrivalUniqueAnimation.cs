using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class GargantuaArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(0f, 1.92f, 0f);

            const float delayBeforeSpawn = 3f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            unitView.GameObject.SetActive(false);

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/Gargantua_Arrival"));

            Transform cameraVFXObj = animationVFX.transform.Find("!! Camera shake");

            Transform cameraGroupTransform = CameraManager.GetGameplayCameras();
            cameraGroupTransform.SetParent(cameraVFXObj);

            animationVFX.transform.position = unitView.PositionOfBoard + offset;

            Vector3 cameraLocalPosition = animationVFX.transform.position * -1;
            cameraGroupTransform.localPosition = cameraLocalPosition;

            InternalTools.DoActionDelayed(() =>
            {
                if (cameraGroupTransform.parent == cameraVFXObj)
                {
                    cameraGroupTransform.SetParent(null);
                    cameraGroupTransform.position = Vector3.zero;
                }
                
                Object.Destroy(animationVFX);
                
                if (unitView != null)
                {
                    unitView.GameObject.SetActive(true);
                    unitView.battleframeAnimator.Play(0, -1, 1);
                    BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);
                }             

                endArrivalCallback?.Invoke();                

                IsPlaying = false;

            }, delayBeforeSpawn);
        }
    }
}