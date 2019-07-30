using UnityEngine;
using System;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MountainArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(-0.78f, 2.0f, 0f);

            const float delayBeforeSpawn = 4f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            unitView.GameObject.SetActive(false);

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/Mountain_Arrival"));

            Transform creatureImage = animationVFX.transform.Find("Battleframe_/SpriteContainer/CreaturePicture").transform;
            Card card = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCardByName("Mountain");
            FloatVector2 position = card.PictureTransforms.Battleground.Position;
            float scale = card.PictureTransforms.Battleground.Scale;
            creatureImage.transform.localPosition = new Vector3(position.X, position.Y, 0f);
            creatureImage.transform.localScale = Vector3.one * scale;

            Transform cameraVFXObj = animationVFX.transform.Find("!! Camera shake");

            Transform cameraGroupTransform = CameraManager.GetGameplayCameras();
            cameraGroupTransform.SetParent(cameraVFXObj);

            //PlaySound("CZB_AUD_Cherno_Bill_Arrival_F1_EXP");

            animationVFX.transform.position = unitView.PositionOfBoard + offset;

            Vector3 cameraLocalPosition = animationVFX.transform.position * -1;
            cameraGroupTransform.localPosition = cameraLocalPosition;

            GameObject battleFrameParticles = unitView.Transform.Find("Walker_Arrival_VFX(Clone)/ScrapFlies").gameObject;

            InternalTools.DoActionDelayed(() =>
            {
                unitView.GameObject.SetActive(true);

                battleFrameParticles.SetActive(false);
                unitView.battleframeAnimator.Play(0, -1, 1);

                cameraGroupTransform.SetParent(null);
                cameraGroupTransform.position = Vector3.zero;
                Object.Destroy(animationVFX);

                endArrivalCallback?.Invoke();

                BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);

                IsPlaying = false;

            }, delayBeforeSpawn);
        }
    }
}
