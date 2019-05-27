using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class RestoreDefRandomlySplitAbilityView : AbilityViewBase<RestoreDefRandomlySplitAbility>
    {
        private BattlegroundController _battlegroundController;

        private string _cardName;

        private List<IBoardObject> _targets;

        public RestoreDefRandomlySplitAbilityView(RestoreDefRandomlySplitAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targets = info as List<IBoardObject>;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                foreach (IBoardObject boardObject in _targets)
                {
                    switch (boardObject)
                    {
                        case CardModel unit:
                            targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(unit).Transform.position;
                            break;
                        case Player player:
                            targetPosition = Ability.TargetPlayer.AvatarObject.transform.position;
                            break;
                    }

                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.position = Utilites.CastVfxPosition(_battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position);
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
                }

                bool isUnit = false;
                BoardUnitView unitModel = null;
                foreach (object boardObject in _targets)
                {
                    switch (boardObject)
                    {
                        case BoardUnitView unit:
                            targetPosition = unit.Transform.position;
                            isUnit = true;
                            break;
                        case Player player:
                            targetPosition = player.AvatarObject.transform.position;
                            isUnit = false;
                            break;
                    }

                    if (isUnit)
                    {
                        CreateVfx(targetPosition + offset, true, delayBeforeDestroy);

                        unitModel = boardObject as BoardUnitView;
                       
                        string objectName = "WalkerMask";
                        switch (unitModel.Model.InitialUnitType)
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
            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
