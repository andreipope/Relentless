using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbilityView : AbilityViewBase<MassiveDamageAbility>
    {
        private static readonly MouldId LawnmowerCardId = new MouldId(114);

        private List<BoardUnitView> _unitsViews;

        private List<BoardObject> _targets;

        private BattlegroundController _battlegroundController;

        private ICameraManager _cameraManager;


        public MassiveDamageAbilityView(MassiveDamageAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();

            _cameraManager = GameClient.Get<ICameraManager>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targets = info as List<BoardObject>;
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

                foreach (Enumerators.Target target in Ability.AbilityTargets)
                {
                    switch (target)
                    {
                        case Enumerators.Target.OPPONENT_ALL_CARDS:
                            CustomCreateVfx(offset, true, delayBeforeDestroy, justPosition);
                            break;
                        case Enumerators.Target.PLAYER_ALL_CARDS:
                            foreach (BoardUnitModel cardPlayer in Ability.PlayerCallerOfAbility.CardsOnBoard)
                            {
                                BoardUnitView cardPlayerView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(cardPlayer);
                                CreateVfx(cardPlayerView.Transform.position, true);
                            }
                            break;
                    }
                }
            }
            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        private void CustomCreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            int playerPos = Ability.PlayerCallerOfAbility.IsLocalPlayer ? 1 : -1;

            if (!justPosition)
            {
                pos = Utilites.CastVfxPosition(pos * playerPos);
            }
            else
            {
                pos = pos * playerPos;
            }
            ClearParticles();

            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
