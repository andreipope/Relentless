using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Loom.ZombieBattleground
{
    public class DeckBuilderCard : MonoBehaviour, IScrollHandler
    {
        public HordeEditingPage Scene;

        public IReadOnlyCard Card;

        public bool IsHordeItem;

        private MultiPointerClickHandler _multiPointerClickHandler;

        public void OnScroll(PointerEventData eventData)
        {
            Scene?.ScrollCardList(IsHordeItem, eventData.scrollDelta);
        }

        private void Awake()
        {
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

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardAdded);
            }
            else
            {
                Scene.RemoveCardFromDeck(this, Card);

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardRemoved);
            }
        }
    }
}
