using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GameplayActionReportStunCreatureByAbility : ReportViewBase
    {
        private BoardUnit _abilityUnitOwner;

        private BoardUnit _abilityUsedOnUnit;

        private GameObject _stunnedUnitPreviewObject, _abilityOwnerObject;

        public GameplayActionReportStunCreatureByAbility(
            GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityUnitOwner = GameAction.Parameters[0] as BoardUnit;
            _abilityUsedOnUnit = GameAction.Parameters[1] as BoardUnit;

            PreviewImage.sprite = _abilityUnitOwner.Sprite;

            _abilityOwnerObject = CreateCardPreview(_abilityUnitOwner.Card, Vector3.zero);
            _stunnedUnitPreviewObject = CreateCardPreview(_abilityUsedOnUnit.Card, Vector3.zero);
        }
    }
}
