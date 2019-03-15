using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitsAbilityView : AbilityViewBase<DestroyUnitsAbility>
    {
        private BattlegroundController _battlegroundController;

        private ICameraManager _cameraManager;

        private List<BoardUnitModel> _unitsViews;

        #region BulldozerFields

        private GameObject opponentLineObject;

        private GameObject playerLineObject;

        private GameObject _cardDissapearingPrefab;
        #endregion

        public DestroyUnitsAbilityView(DestroyUnitsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();

            _cameraManager = GameClient.Get<ICameraManager>();

            _unitsViews = new List<BoardUnitModel>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _unitsViews = (List<BoardUnitModel>)info;

            float delayBeforeDestroy = 3f;
            float delayAfter = 0;
            Vector3 offset = Vector3.zero;
            Enumerators.CardNameOfAbility cardNameOfAbility = Enumerators.CardNameOfAbility.None;

            string soundName = string.Empty;
            float delaySound = 0;


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

                Vector3 targetPosition = Vector3.zero;

                switch (cardNameOfAbility)
                {
                    case Enumerators.CardNameOfAbility.None:
                        {
                            CreateVfx(targetPosition + offset, true, delayBeforeDestroy, true);
                        }
                        break;
                    case Enumerators.CardNameOfAbility.Bulldozer:
                        {
                            CreateVfx(targetPosition + offset, true, delayBeforeDestroy, true);
                            opponentLineObject = VfxObject.transform.Find("RubbleUp/RubbleSeq").gameObject;
                            playerLineObject = VfxObject.transform.Find("Rubble/RubbleSeq").gameObject;

                            _cardDissapearingPrefab = VfxObject.transform.Find("CardsDissapearing/Tears").gameObject;

                            Ability.OnUpdateEvent += OnUpdateEventHandler;
                        }
                        break;
                    default:
                        break;
                }
            }

            PlaySound(soundName, delaySound);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        #region Bulldozer
        private void OnUpdateEventHandler()
        {
            BoardUnitView unitView;
            for (int i = 0; i < _unitsViews.Count; i++)
            {
                unitView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(_unitsViews[i]);
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
                Ability.OnUpdateEvent -= OnUpdateEventHandler;
            }
        }

        private void DestroyUnit(BoardUnitView unit)
        {
            CreateSubParticle(unit.Transform.position);
            _unitsViews.Remove(unit.Model);
            _cameraManager.ShakeGameplay(Enumerators.ShakeType.Medium);
            Ability.DestroyUnit(unit);
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
