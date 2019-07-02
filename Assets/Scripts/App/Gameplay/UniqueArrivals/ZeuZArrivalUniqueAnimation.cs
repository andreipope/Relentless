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

            Vector3 offset = new Vector3(0, 0.48f, 0f);

            const float delayBeforeSpawn = 0.7f;
            const float delayBeforeDestroyVFX = 3f;

            BoardUnitView unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            Transform boardParent = unitView.GameObject.transform.parent;

            unitView.GameObject.SetActive(false);

            unitView.GameObject.transform.Find("Other").gameObject?.SetActive(true);

            Dictionary<GameObject, int> unitLayerInfo = new Dictionary<GameObject, int>();

            SortingGroup sortingGroup = unitView.Transform.GetComponent<SortingGroup>();

            int sortingIndex = sortingGroup.sortingLayerID;

            sortingGroup.sortingLayerID = SRSortingLayers.GameplayInfo;

            List<GameObject>  allUnitObj = unitView.GameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject).ToList();
            foreach (GameObject child in allUnitObj)
            {
                unitLayerInfo.Add(child, child.layer);
                child.layer = 0;
            }

            InternalTools.DoActionDelayed(() =>
            {
                GameObject animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                            "Prefabs/VFX/UniqueArrivalAnimations/ZeuZ"));

                animationVFX.transform.position = unitView.PositionOfBoard + offset;
                unitView.GameObject.SetActive(true);
                unitView.Transform.SetParent(animationVFX.transform.Find("Zeus_Card_PH"), false);
                unitView.Transform.localScale = Vector3.one * 5.95f;


                InternalTools.DoActionDelayed(() =>
                {

                    unitView.Transform.SetParent(boardParent, true);
                    Object.Destroy(animationVFX);

                    foreach (GameObject child in allUnitObj)
                    {
                        if (unitLayerInfo.ContainsKey(child))
                        {
                            child.layer = unitLayerInfo[child];
                        }
                    }
                    sortingGroup.sortingLayerID = sortingIndex;

                    endArrivalCallback?.Invoke();

                    BoardController.UpdateCurrentBoardOfPlayer(unitView.Model.OwnerPlayer, null);

                    IsPlaying = false;
                }, delayBeforeDestroyVFX);
            }, delayBeforeSpawn);
        }
    }
}
