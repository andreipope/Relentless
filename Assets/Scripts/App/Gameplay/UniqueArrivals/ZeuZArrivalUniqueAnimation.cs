using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine.Rendering;

namespace Loom.ZombieBattleground
{
    public class ZeuZArrivalUniqueAnimation : UniqueAnimation
    {
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();

            IsPlaying = true;

            Vector3 offset = new Vector3(0, 0.718f, 0f);

            const float delayBeforeSpawn = 2.75f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            unitView.GameObject.SetActive(false);

            GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/ZeuZ_Arrival"));            

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

                BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);

                IsPlaying = false;

            }, delayBeforeSpawn);            
        }
    }
}
