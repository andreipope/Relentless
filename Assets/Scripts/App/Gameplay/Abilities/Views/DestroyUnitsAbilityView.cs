using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitsAbilityView : AbilityViewBase<DestroyUnitsAbility>
    {
        private BattlegroundController _battlegroundController;

        private List<BoardUnitView> _unitsViews;
        
        public Coroutine CorrectActionReportPanelCoroutine;

        #region BulldozerFields

        private GameObject opponentLineObject;

        private GameObject playerLineObject;

        private GameObject _cardDissapearingPrefab;

        private event Action OnEventEnded;

        private bool _deactivateUnitImmediately;
        #endregion

        public DestroyUnitsAbilityView(DestroyUnitsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();

            _unitsViews = new List<BoardUnitView>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            List<CardModel> units = info as List<CardModel>;
            float delayBeforeDestroy = 3f;
            float delayAfter = 0;
            Vector3 offset = Vector3.zero;
            Enumerators.CardNameOfAbility cardNameOfAbility = Enumerators.CardNameOfAbility.None;

            string soundName = string.Empty;
            float delaySound = 0;
            _deactivateUnitImmediately = false;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    cardNameOfAbility = effectInfo.cardNameOfAbility;
                    offset = effectInfo.offset;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                }

                Vector3 targetPosition = Vector3.zero + offset;

                switch (cardNameOfAbility)
                {
                    case Enumerators.CardNameOfAbility.None:
                        {
                            CreateVfx(targetPosition, true, delayBeforeDestroy, true);
                        }
                        break;
                    case Enumerators.CardNameOfAbility.Bulldozer:
                        {
                            _deactivateUnitImmediately = true;
                            CreateVfx(targetPosition, true, delayBeforeDestroy, true);

                            Transform cameraGroupTransform = GameClient.Get<ICameraManager>().GetGameplayCameras();
                            
                            GameplayPage gameplayPage = GameClient.Get<IUIManager>().GetPage<GameplayPage>();
                            Transform actionReportPivot = GameObject.Find("ActionReportPivot").transform;
                            GameObject pivotParent = new GameObject("PivotParent");                    
                            Vector3 actionReportPivotCachePos = actionReportPivot.position;
                            actionReportPivot.SetParent(pivotParent.transform);
                            CorrectActionReportPanelCoroutine = MainApp.Instance.StartCoroutine
                            (
                                gameplayPage.CorrectReportPanelDuringCameraShake
                                (
                                    cameraGroupTransform, 
                                    VfxObject.transform.Find("Camera Anim/!! Camera shake")
                                )
                            );

                            OnEventEnded += () =>
                            {
                                if (CorrectActionReportPanelCoroutine != null)
                                {
                                    MainApp.Instance.StopCoroutine(CorrectActionReportPanelCoroutine);
                                }
                                CorrectActionReportPanelCoroutine = null;
                                
                                cameraGroupTransform.SetParent(null);
                                cameraGroupTransform.position = Vector3.zero;
                                
                                actionReportPivot.SetParent(null);
                                actionReportPivot.position = actionReportPivotCachePos;
                                gameplayPage.SyncActionReportPanelPositionWithPivot();
                                Object.Destroy(pivotParent);
                            };

                            opponentLineObject = VfxObject.transform.Find("VFX/RubbleUp/BurstToxic").gameObject;
                            playerLineObject = VfxObject.transform.Find("VFX/Rubble/BurstToxic").gameObject;

                            _cardDissapearingPrefab = VfxObject.transform.Find("VFX/CardsDissapearing").gameObject;
                            _unitsViews = units.Select(unit => _battlegroundController.GetCardViewByModel<BoardUnitView>(unit)).ToList();

                            Ability.OnUpdateEvent += OnUpdateEventHandler;

                            Ability.VFXAnimationEnded += () =>
                            {
                                cameraGroupTransform.SetParent(null);
                                cameraGroupTransform.position = Vector3.zero;
                            };
                        }
                        break;
                    case Enumerators.CardNameOfAbility.Molotov:
                        {
                            _unitsViews = units.Select(unit => _battlegroundController.GetCardViewByModel<BoardUnitView>(unit)).ToList();
                            foreach(BoardUnitView unitView in _unitsViews)
                            {
                                targetPosition = unitView.Transform.position;
                                CreateVfx(targetPosition + offset, true, delayBeforeDestroy, true);
                            }                            
                        }
                        break;
                    default:
                        break;
                }
            }

            PlaySound(soundName, delaySound);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayBeforeDestroy);
        }

        #region Bulldozer
        private void OnUpdateEventHandler()
        {
            BoardUnitView unitView;
            for (int i = 0; i < _unitsViews.Count; i++)
            {
                unitView = _unitsViews[i];

                if (unitView == null || unitView is default(BoardUnitView))
                {
                    continue;
                }

                if (unitView.Model.OwnerPlayer.IsLocalPlayer)
                {
                    if (playerLineObject.transform.position.x > unitView.Transform.position.x + 1f)
                    {
                        DestroyUnit(unitView);
                    }
                }
                else
                {
                    if (opponentLineObject.transform.position.x + 1f < unitView.Transform.position.x)
                    {
                        DestroyUnit(unitView);
                    }
                }
            }

            if(_unitsViews.Count == 0)
            {                
                OnEventEnded?.Invoke();
                Ability.OnUpdateEvent -= OnUpdateEventHandler;
            }
        }

        private void DestroyUnit(BoardUnitView unit)
        {
            CreateSubParticle(unit.Transform.position);
            _unitsViews.Remove(unit);
            if (!unit.Model.HasBuffShield)
            {
                unit.ChangeModelVisibility(false);
            }
            if(_deactivateUnitImmediately)
            {
                unit.GameObject.SetActive(false);
            }
            Ability.DestroyUnit(unit.Model);            
        }

        private void CreateSubParticle(Vector3 pos, float duration = 3)
        {
            GameObject subObject = Object.Instantiate(_cardDissapearingPrefab);
            subObject.SetActive(true);
            subObject.transform.position = pos;
            ParticlesController.RegisterParticleSystem(subObject, true, duration);
            string clipTitle = "Bulldozer_Collision_F1_V" + UnityEngine.Random.Range(1, 5).ToString();
            PlaySound(clipTitle, 0);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
        #endregion
    }
}
