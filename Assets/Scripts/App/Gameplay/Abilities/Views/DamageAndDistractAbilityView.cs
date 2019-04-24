using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageAndDistractAbilityView : AbilityViewBase<DamageAndDistractAbility>
    {
        private const int LawnmowerCardId = 114;

        private List<BoardUnitView> _unitsViews;

        private BattlegroundController _battlegroundController;

        private ICameraManager _cameraManager;

        #region LawnmowerFields

        private GameObject _lineObject;

        private GameObject _cardDissapearingPrefab;

        private BoardUnitView _unitView;
        #endregion

        public DamageAndDistractAbilityView(DamageAndDistractAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();

            _cameraManager = GameClient.Get<ICameraManager>();

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

                Vector3 targetPosition = Vector3.zero;

                if (Ability.CardModel != null && Ability.CardModel.Prototype.MouldId == LawnmowerCardId)
                {
                    CreateVfx(targetPosition + offset, true, delayBeforeDestroy, true);
                    VfxObject.transform.position = Ability.PlayerCallerOfAbility.IsLocalPlayer ? Vector3.up * 2.05f : Vector3.up * -1.45f;
                    _lineObject = VfxObject.transform.Find("Lawnmover/BurstToxic").gameObject;
                    _cardDissapearingPrefab = VfxObject.transform.Find("Lawnmover/CardsDissapearing/Tears").gameObject;
                    _unitsViews = units.Select(unit => _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit)).ToList();

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
                                    BoardUnitView cardPlayerView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(cardPlayer);
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
                        _cameraManager.ShakeGameplay(Enumerators.ShakeType.Medium);
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
