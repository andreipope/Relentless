using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class VortexArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(0f, -1.64f, 0f);

            const float delayBeforeSpawn = 3.5f;
            const float delaySubTriggerAbility = 0.75f;

            SummonsAbility.LessDefThanInOpponentSubTriggerDelay = () =>
            {
                return delayBeforeSpawn - delaySubTriggerAbility;
            };

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            unitView.GameObject.SetActive(false);

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/Vortex_Arrival"));

            animationVFX.transform.position = unitView.PositionOfBoard + offset;

            InternalTools.DoActionDelayed(() =>
            {
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