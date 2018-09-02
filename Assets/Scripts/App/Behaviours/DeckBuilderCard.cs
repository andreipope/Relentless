using LoomNetwork.CZB.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class DeckBuilderCard : MonoBehaviour, IScrollHandler
    {
        public DeckEditingPage Scene;

        public Card Card;

        public bool IsActive;

        public bool IsHordeItem = false;

        private MultiPointerClickHandler _multiPointerClickHandler;

        public void OnScroll(PointerEventData eventData)
        {
            if (Scene != null)
            {
                Scene.ScrollCardList(IsHordeItem, eventData.scrollDelta);
            }
        }

        private void Awake()
        {
            IsActive = true;
            _multiPointerClickHandler = gameObject.AddComponent<MultiPointerClickHandler>();
            _multiPointerClickHandler.SingleClickReceived += SingleClickAction;
            _multiPointerClickHandler.DoubleClickReceived += DoubleClickAction;
        }

        private void SingleClickAction()
        {
            Scene.SelectCard(this, Card);
        }

        private void DoubleClickAction()
        {
            if (!IsHordeItem)
            {
                Scene.AddCardToDeck(this, Card);
            }
            else
            {
                Scene.RemoveCardFromDeck(this, Card);
            }
        }
    }
}
