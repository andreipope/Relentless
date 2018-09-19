using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class CostsLessIfCreatureTypeInHand : IAbility
    {
        private WorkingCard _workingCard;

        private BoardCard _boardCard;

        private AbilityEnumerator.FactionType _factionType;

        private IGameplayManager _gameplayManager;

        private ITimerManager _timerManager;

        private CardsController _cardsController;

        private BoardObject _abilityOwner;

        public NewAbilityData AbilityData { get; private set; }

        public void Init(NewAbilityData data, BoardObject owner)
        {
            AbilityData = data;
            _abilityOwner = owner;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _cardsController = _gameplayManager.GetController<CardsController>();

            if (_abilityOwner is BoardUnit unit)
            {
                _workingCard = unit.Card;
            }
            else if (_abilityOwner is BoardSpell spell)
            {
                _workingCard = spell.Card;
            }
            else if (_abilityOwner is HandBoardCard handBoardCard)
            {
                _workingCard = handBoardCard.BoardCard.WorkingCard;
                _boardCard = handBoardCard.BoardCard;
            }

            foreach (AbilityRestrictionData restrictionData in AbilityData.Restrictions)
            {
                if (restrictionData.Faction != AbilityEnumerator.FactionType.NONE)
                {
                    _factionType = restrictionData.Faction;
                    break;
                }
            }

            _abilityOwner.OwnerPlayer.HandChanged += HandChangedHandler;
            _abilityOwner.OwnerPlayer.CardPlayed += CardPlayedHandler;

            _timerManager.AddTimer(x =>
            {
                CallAction(null);
            },
            null,
            0.5f);
        }

        public void CallAction(object target)
        {
            if (!_abilityOwner.OwnerPlayer.CardsInHand.Contains(_workingCard))
                return;

            //int gooCost = _abilityOwner.OwnerPlayer.CardsInHand.FindAll(x => x.LibraryCard.CardSetType == _factionType && x != _workingCard).Count * 1; // implement variable
            //_cardsController.SetGooCostOfCardInHand(_abilityOwner.OwnerPlayer, _workingCard, _workingCard.RealCost + gooCost, _boardCard);
        }

        private void CardPlayedHandler(WorkingCard card)
        {
            if (!card.Equals(_workingCard))
                return;

            _abilityOwner.OwnerPlayer.HandChanged -= HandChangedHandler;
            _abilityOwner.OwnerPlayer.CardPlayed -= CardPlayedHandler;
        }

        private void HandChangedHandler(int obj)
        {
            CallAction(null);
        }
    }
}
