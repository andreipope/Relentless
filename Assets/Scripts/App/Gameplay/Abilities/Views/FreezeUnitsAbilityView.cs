using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FreezeUnitsAbilityView : AbilityViewBase<FreezeUnitsAbility>
    {
        private BattlegroundController _battlegroundController;

        private Player _opponent;

        public FreezeUnitsAbilityView(FreezeUnitsAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if(info != null)
            {
                _opponent = info as Player;
            }
            ActionCompleted();
        }

        private void ActionCompleted()
        {
            ClearParticles();

            float delayAfter = 0;
            float delayBeforeDestroy = 5f;
            Vector3 offset = Vector3.zero;

            string soundName = string.Empty;
            float soundDelay = 0;

            Enumerators.AbilityEffectInfoPositionType positionType = Enumerators.AbilityEffectInfoPositionType.Target;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();

                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    offset = effectInfo.offset;
                    soundName = effectInfo.soundName;
                    positionType = effectInfo.positionInfo.type;
                    soundDelay = effectInfo.delayForSound;
                }

                Vector3 position = Vector3.zero;
                switch (positionType)
                {
                    case Enumerators.AbilityEffectInfoPositionType.Target:
                        {
                            foreach (CardModel unit in _opponent.CardsOnBoard)
                            {
                                BoardUnitView unitView = _battlegroundController.GetCardViewByModel<BoardUnitView>(unit);
                                position = unitView.Transform.position;
                                CreateVfx(position, true, delayBeforeDestroy, true);
                            }
                        }
                        break;
                    case Enumerators.AbilityEffectInfoPositionType.Overlord:
                        {
                            position = Ability.PlayerCallerOfAbility.AvatarObject.transform.position;
                            CreateVfx(position, true, delayBeforeDestroy, true);
                            if(!Ability.PlayerCallerOfAbility.IsLocalPlayer)
                            {
                                VfxObject.transform.eulerAngles = new Vector3(VfxObject.transform.eulerAngles.x, VfxObject.transform.eulerAngles.y, 180);
                            }
                        }
                        break;
                    default:
                        break;
                }

            }

            PlaySound(soundName, soundDelay);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
