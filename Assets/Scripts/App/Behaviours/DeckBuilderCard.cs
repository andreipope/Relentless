using LoomNetwork.CZB.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class DeckBuilderCard : MonoBehaviour, IScrollHandler
    {
        public DeckEditingPage scene;

        public Card card;

        public bool isActive;

        public bool isHordeItem = false;

        private MultiPointerClickHandler _multiPointerClickHandler;

        public void OnScroll(PointerEventData eventData)
        {
            if (scene != null)
            {
                scene.ScrollCardList(isHordeItem, eventData.scrollDelta);
            }
        }

        private void Awake()
        {
            isActive = true;
            _multiPointerClickHandler = gameObject.AddComponent<MultiPointerClickHandler>();
            _multiPointerClickHandler.SingleClickReceived += SingleClickAction;
            _multiPointerClickHandler.DoubleClickReceived += DoubleClickAction;
        }

        private void SingleClickAction()
        {
            scene.SelectCard(this, card);
        }

        private void DoubleClickAction()
        {
            if (!isHordeItem)
            {
                scene.AddCardToDeck(this, card);
            } else
            {
                scene.RemoveCardFromDeck(this, card);
            }
        }
    }
}
