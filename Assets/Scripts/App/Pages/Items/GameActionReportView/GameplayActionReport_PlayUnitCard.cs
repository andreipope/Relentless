using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReportPlayUnitCard : ReportViewBase
    {
        private Player _callerPlayer;

        private BoardUnit _playedCard;

        private GameObject _playedCardPreviewObject;

        public GameplayActionReportPlayUnitCard(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = GameAction.Parameters[0] as Player;
            _playedCard = GameAction.Parameters[1] as BoardUnit;

            PreviewImage.sprite = _playedCard.Sprite;

            _playedCardPreviewObject = CreateCardPreview(_playedCard.Card, Vector3.zero);
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
