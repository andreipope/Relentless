using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GameplayActionReportStunCreatureBySkill : ReportViewBase
    {
        private Player _callerPlayer;

        private BoardUnitModel _skillUsedOnUnitModel;

        private GameObject _stunnedUnitPreviewObject, _skillOwnerObject;

        public GameplayActionReportStunCreatureBySkill(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = GameAction.Parameters[0] as Player;
            _skillUsedOnUnitModel = GameAction.Parameters[1] as BoardUnitModel;

            _skillOwnerObject = CreatePlayerPreview(_callerPlayer, Vector3.zero);
            _stunnedUnitPreviewObject = CreateCardPreview(_skillUsedOnUnitModel.Card, Vector3.right * 6, out BoardCard skillUsedOnUnitBoardCard);
            PreviewImage.sprite = skillUsedOnUnitBoardCard.PictureSprite.sprite;
        }
    }
}
