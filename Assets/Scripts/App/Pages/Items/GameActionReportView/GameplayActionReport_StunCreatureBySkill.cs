using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReportStunCreatureBySkill : ReportViewBase
    {
        private Player _callerPlayer;

        private BoardUnit _skillUsedOnUnit;

        private GameObject _stunnedUnitPreviewObject, _skillOwnerObject;

        public GameplayActionReportStunCreatureBySkill(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = GameAction.Parameters[0] as Player;
            _skillUsedOnUnit = GameAction.Parameters[1] as BoardUnit;

            PreviewImage.sprite = _skillUsedOnUnit.Sprite;

            _skillOwnerObject = CreatePlayerPreview(_callerPlayer, Vector3.zero);
            _stunnedUnitPreviewObject = CreateCardPreview(_skillUsedOnUnit.Card, Vector3.right * 6);
        }

        public override void OnPointerEnterEventHandler(PointerEventData obj)
        {
            base.OnPointerEnterEventHandler(obj);
        }

        public override void OnPointerExitEventHandler(PointerEventData obj)
        {
            base.OnPointerExitEventHandler(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
