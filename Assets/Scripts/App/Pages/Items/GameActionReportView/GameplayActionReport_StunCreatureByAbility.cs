using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GameplayActionReportStunCreatureByAbility : ReportViewBase
    {
        private BoardUnitModel _abilityUnitModelOwner;
        private BoardUnitModel _abilityUsedOnUnitModel;

        private GameObject _stunnedUnitPreviewObject, _abilityOwnerObject;

        public GameplayActionReportStunCreatureByAbility(
            GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityUnitModelOwner = GameAction.Parameters[0] as BoardUnitModel;
            _abilityUsedOnUnitModel = GameAction.Parameters[1] as BoardUnitModel;

            _abilityOwnerObject = CreateCardPreview(_abilityUnitModelOwner.Card, Vector3.zero, out BoardCard abilityOwnerBoardCard);
            _stunnedUnitPreviewObject = CreateCardPreview(_abilityUsedOnUnitModel.Card, Vector3.zero);

            PreviewImage.sprite = abilityOwnerBoardCard.PictureSprite.sprite;
        }
    }
}
