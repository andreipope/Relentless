using UnityEngine;
using System;
using System.Collections;
using Loom.ZombieBattleground.Helpers;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class GargantuaArrivalUniqueAnimation : UniqueAnimation
    {
        public Coroutine CheckForCancelCoroutine;

        private GameObject _animationVFX;

        private Transform _cameraGroupTransform, _cameraVFXObj;

        private BoardUnitView _unitView;

        private Action _endArrivalCallback;
        
        public override void Play(IBoardObject boardObject, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            startGeneralArrivalCallback?.Invoke();
            _endArrivalCallback = endArrivalCallback;

            IsPlaying = true;

            Vector3 offset = new Vector3(0f, 1.92f, 0f);

            const float delayBeforeSpawn = 3f;

            _unitView = BattlegroundController.GetCardViewByModel<BoardUnitView>(boardObject as CardModel);

            _unitView.GameObject.SetActive(false);

            _animationVFX = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>(
                                                        "Prefabs/VFX/UniqueArrivalAnimations/Gargantua_Arrival"));

            _cameraVFXObj = _animationVFX.transform.Find("!! Camera shake");

            _cameraGroupTransform = CameraManager.GetGameplayCameras();
            _cameraGroupTransform.SetParent(_cameraVFXObj);

            _animationVFX.transform.position = _unitView.PositionOfBoard + offset;

            Vector3 cameraLocalPosition = _animationVFX.transform.position * -1;
            _cameraGroupTransform.localPosition = cameraLocalPosition;

            CheckForCancelCoroutine = MainApp.Instance.StartCoroutine(CheckForCancel(_unitView, _animationVFX));
            DamageTargetAbility.OnInputEnd += OnAbilityInputEnd;

            InternalTools.DoActionDelayed(() =>
            {
                VFXEnd();        

                IsPlaying = false;

            }, delayBeforeSpawn);
        }
        
        public IEnumerator CheckForCancel(BoardUnitView unitView, GameObject animationVFX)
        {
            while(true)
            {
                if (unitView == null)
                {
                    Object.Destroy(animationVFX);
                    break;
                }
                yield return null;
            }
        }
        
        private void VFXEnd(bool isAbilityResolved = true)
        {
            if (CheckForCancelCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(CheckForCancelCoroutine);
            }
            CheckForCancelCoroutine = null;

            if (_cameraGroupTransform.parent == _cameraVFXObj)
            {
                _cameraGroupTransform.SetParent(null);
                _cameraGroupTransform.position = Vector3.zero;
            }

            Object.Destroy(_animationVFX);

            if (_unitView != null)
            {
                _unitView.GameObject.SetActive(true);
                _unitView.battleframeAnimator.Play(0, -1, 1);
                foreach (Transform child in _unitView.battleframeAnimator.transform)
                {
                    if (child.name == "ScrapFlies")
                    {
                        child.gameObject.SetActive(false);
                        break;
                    }
                }
                BoardController.UpdateCurrentBoardOfPlayer(_unitView.Model.OwnerPlayer, null);
            }

            if (isAbilityResolved)
            {
                _endArrivalCallback?.Invoke();
                _endArrivalCallback = null;
            }
        }        

        private void OnAbilityInputEnd(bool isAbilityResolved)
        {
            DamageTargetAbility.OnInputEnd -= OnAbilityInputEnd;
            if (!isAbilityResolved)
            {
                VFXEnd(isAbilityResolved);
            }           
        }
    }
}