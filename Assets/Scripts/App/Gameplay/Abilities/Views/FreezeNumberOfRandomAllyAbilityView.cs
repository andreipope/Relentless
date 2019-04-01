using DG.Tweening;
using Loom.ZombieBattleground.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FreezeNumberOfRandomAllyAbilityView : AbilityViewBase<FreezeNumberOfRandomAllyAbility>
    {
        private BattlegroundController _battlegroundController;

        private List<IBoardObject> _allies;

        public FreezeNumberOfRandomAllyAbilityView(FreezeNumberOfRandomAllyAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
            _allies = new List<IBoardObject>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (info != null)
            {
                _allies = (List<IBoardObject>)info;
            }
            ActionCompleted();
        }

        private void ActionCompleted()
        {
            ClearParticles();

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                for (int i = 0; i < _allies.Count; i++)
                {
                    object ally = _allies[i];
                    switch (ally)
                    {
                        case Player player:
                            targetPosition = Utilites.CastVfxPosition(player.AvatarObject.transform.position);
                            CreateVfx(targetPosition, true, 5f, true);
                            break;
                        case CardModel unit:
                            targetPosition = Utilites.CastVfxPosition(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform.position);
                            CreateVfx(targetPosition, true, 5f);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(ally), ally, null);
                    }
                }
            }

            Ability.InvokeVFXAnimationEnded();
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
