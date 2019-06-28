using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Loom.ZombieBattleground
{
    public class DeckBuilderCard : MonoBehaviour, IScrollHandler
    {
        public HordeEditingTab Page;

        public IReadOnlyCard Card;

        public bool IsHordeItem;

        private MultiPointerClickHandler _multiPointerClickHandler;
        private ITutorialManager _tutorialManager;

        public void OnScroll(PointerEventData eventData)
        {
            //Page?.ScrollCardList(IsHordeItem, eventData.scrollDelta);
        }

        private void Awake()
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _multiPointerClickHandler = gameObject.AddComponent<MultiPointerClickHandler>();
            _multiPointerClickHandler.SingleClickReceived += SingleClickAction;
            _multiPointerClickHandler.DoubleClickReceived += DoubleClickAction;
        }

        private void SingleClickAction()
        {
            //Page?.SelectCard(this, Card);
        }

        private void DoubleClickAction()
        {
            if (_tutorialManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.IsGameplayTutorial() &&
                (_tutorialManager.CurrentTutorialStep.ToMenuStep().CardsInteractingLocked ||
                 !_tutorialManager.CurrentTutorialStep.ToMenuStep().CanDoubleTapCards))
                return;

            if (!IsHordeItem)
            {
                Page?.AddCardToDeck(Card,true);

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardAdded);
            }
            else
            {
                Page?.RemoveCardFromDeck(Card,true);

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardRemoved);
            }
        }
    }
}
