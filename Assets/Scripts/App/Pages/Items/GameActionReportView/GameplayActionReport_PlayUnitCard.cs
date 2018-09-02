using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_PlayUnitCard : ReportViewBase
    {
        private Player _callerPlayer;

        private BoardUnit _playedCard;

        private GameObject _playedCardPreviewObject;

        public GameplayActionReport_PlayUnitCard(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = gameAction.parameters[0] as Player;
            _playedCard = gameAction.parameters[1] as BoardUnit;

            previewImage.sprite = _playedCard.sprite;

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
