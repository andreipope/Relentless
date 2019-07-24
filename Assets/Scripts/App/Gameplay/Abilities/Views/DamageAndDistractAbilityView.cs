using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageAndDistractAbilityView : AbilityViewBase<DamageAndDistractAbility>
    {
        private readonly MouldId _lawnmowerCardMouldId = new MouldId(114);

        private List<BoardUnitView> _unitsViews;

        private BattlegroundController _battlegroundController;
        
        public Coroutine CorrectActionReportPanelCoroutine;

        #region LawnmowerFields

        private GameObject _lineObject;

        private GameObject _cardDissapearingPrefab;

        private BoardUnitView _unitView;
        #endregion

        public DamageAndDistractAbilityView(DamageAndDistractAbility ability) : base(ability)
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
            bool justPosition = false;
            Enumerators.CardNameOfAbility cardNameOfAbility = Enumerators.CardNameOfAbility.None;


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
                    justPosition = true;
                }

                Vector3 targetPosition = VfxObject.transform.position + offset;

                if (Ability.CardModel != null && Ability.CardModel.Prototype.CardKey.MouldId == _lawnmowerCardMouldId)
                {
                    float posY = Ability.PlayerCallerOfAbility.IsLocalPlayer ? 2f : -1.45f;
                    Vector3 newTargetPosition = new Vector3(targetPosition.x, posY, targetPosition.z - 7f);

                    CreateVfx(newTargetPosition, true, delayBeforeDestroy, true);

                    Transform cameraVFXObj = VfxObject.transform.Find("Camera Anim/!! Camera shake");
                    Transform cameraGroupTransform = GameClient.Get<ICameraManager>().GetGameplayCameras();
                    cameraGroupTransform.SetParent(cameraVFXObj);

                    Vector3 cameraPosition = newTargetPosition * -1;
                    cameraGroupTransform.localPosition = new Vector3(cameraPosition.x, cameraGroupTransform.localPosition.y, cameraPosition.y);

                    GameplayPage gameplayPage = GameClient.Get<IUIManager>().GetPage<GameplayPage>();
                    Transform actionReportPivot = GameObject.Find("ActionReportPivot").transform;
                    GameObject pivotParent = new GameObject("PivotParent");                    
                    Vector3 actionReportPivotCachePos = actionReportPivot.position;
                    actionReportPivot.SetParent(pivotParent.transform);
                    CorrectActionReportPanelCoroutine = MainApp.Instance.StartCoroutine
                    (
                        gameplayPage.CorrectReportPanelDuringCameraShake()
                    );

                    Ability.VFXAnimationEnded += () =>
                    {
                        cameraGroupTransform.SetParent(null);
                        cameraGroupTransform.position = Vector3.zero;
                        if (CorrectActionReportPanelCoroutine != null)
                        {
                            MainApp.Instance.StopCoroutine(CorrectActionReportPanelCoroutine);
                        }
                        CorrectActionReportPanelCoroutine = null;
                        actionReportPivot.SetParent(null);
                        actionReportPivot.position = actionReportPivotCachePos;
                        gameplayPage.SyncActionReportPanelPositionWithPivot();
                        Object.Destroy(pivotParent);
                    };

                    _lineObject = VfxObject.transform.Find("BurstToxic").gameObject;
                    _cardDissapearingPrefab = VfxObject.transform.Find("CardsDissapearing").gameObject;
                    _unitsViews = units.Select(unit => _battlegroundController.GetCardViewByModel<BoardUnitView>(unit)).ToList();

                    Ability.OnUpdateEvent += OnUpdateEventHandler;
                }
                else
                {
                    foreach (Enumerators.Target target in Ability.AbilityTargets)
                    {
                        switch (target)
                        {
                            case Enumerators.Target.OPPONENT_ALL_CARDS:
                                CustomCreateVfx(offset, true, delayBeforeDestroy, justPosition);
                                break;
                            case Enumerators.Target.PLAYER_ALL_CARDS:
                                foreach (CardModel cardPlayer in Ability.PlayerCallerOfAbility.CardsOnBoard)
                                {
                                    BoardUnitView cardPlayerView = _battlegroundController.GetCardViewByModel<BoardUnitView>(cardPlayer);
                                    CreateVfx(cardPlayerView.Transform.position, true);
                                }
                                break;
                        }
                    }
                }

            }
            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        private void CustomCreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            ClearParticles();

            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }

        #region Lawnmower
        private void OnUpdateEventHandler()
        {
            if (_unitsViews == null || _unitsViews.Count == 0 || _lineObject == null)
            {
                Ability.OnUpdateEvent -= OnUpdateEventHandler;
                return;
            }

            for (int i = 0; i < _unitsViews.Count; i++)
            {
                _unitView = _unitsViews[i];

                if (_unitView != null && _unitView.GameObject != null && _unitView.GameObject && _unitView.Model.CurrentDefense > 0)
                {
                    if (_lineObject.transform.position.x + 1f < _unitView.Transform.position.x)
                    {
                        Ability.OneActionCompleted(_unitView.Model);
                        CreateSubParticle(_unitView.Transform.position);
                        _unitsViews.Remove(_unitView);
                    }
                }
                else
                {
                    _unitsViews.Remove(_unitView);
                }
            }
        }

        private void CreateSubParticle(Vector3 pos, float duration = 3)
        {
            GameObject subObject = Object.Instantiate(_cardDissapearingPrefab);
            subObject.SetActive(true);
            subObject.transform.position = pos;
            ParticlesController.RegisterParticleSystem(subObject, true, duration);
            string clipTitle = "Lawnmover_Mangled_F1_V" + UnityEngine.Random.Range(1, 5).ToString();
            PlaySound(clipTitle, 0);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
        #endregion
    }
}
