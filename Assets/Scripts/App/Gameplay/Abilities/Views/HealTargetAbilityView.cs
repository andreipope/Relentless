using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HealTargetAbilityView : AbilityViewBase<HealTargetAbility>
    {
        private BattlegroundController _battlegroundController;

        private string _cardName;

        private List<BoardObject> _targets;

        public HealTargetAbilityView(HealTargetAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targets = info as List<BoardObject>;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                foreach (BoardObject boardObject in _targets)
                {
                    switch (boardObject)
                    {
                        case BoardUnitModel unit:
                            targetPosition = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform.position;
                            break;
                        case Player player:
                            targetPosition = player.AvatarObject.transform.position;
                            break;
                    }

                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.position = Utilites.CastVfxPosition(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position);
                    targetPosition = Utilites.CastVfxPosition(targetPosition);
                    VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(ActionCompleted);
                    ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
                }
            }
            else
            {
                ActionCompleted();
            }
        }

        private void ActionCompleted()
        {
            ClearParticles();

            _cardName = "";
            float delayAfter = 0;
            float delayBeforeDestroy = 5f;
            Vector3 offset = Vector3.zero;
            string soundName = string.Empty;
            float delaySound = 0;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();

                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    offset = effectInfo.offset;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                }

                bool isUnit = false;
                BoardUnitModel unitModel = null;

                foreach (BoardObject boardObject in _targets)
                {
                    switch (boardObject)
                    {
                        case BoardUnitModel unit:
                            targetPosition = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform.position;
                            isUnit = true;
                            break;
                        case Player player:
                            targetPosition = player.AvatarObject.transform.position;
                            isUnit = false;
                            break;
                    }

                    CreateVfx(targetPosition + offset, true, delayBeforeDestroy);

                    if (isUnit)
                    {
                        unitModel = boardObject as BoardUnitModel;

                        string objectName = "WalkerMask";
                        switch (unitModel.InitialUnitType)
                        {
                            case Enumerators.CardType.FERAL:
                                objectName = "FeralMask";
                                break;
                            case Enumerators.CardType.HEAVY:
                                objectName = "HeavyMask";
                                break;
                        }
                        VfxObject.transform.Find(objectName).gameObject.SetActive(true);
                    }
                }
            }

            PlaySound(soundName, delaySound);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
