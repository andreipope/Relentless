using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class LowerCostOfCardInHand : IAbility
    {
        private BoardObject _abilityUnitOwner;

        public NewAbilityData AbilityData { get; private set; }

        private IGameplayManager _gameplayManager;
        private CardsController _cardsController;

        public void Init(NewAbilityData data, BoardObject owner)
        {
            AbilityData = data;
            _abilityUnitOwner = owner;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _cardsController = _gameplayManager.GetController<CardsController>();
        }

        public void CallAction(object target)
        {
            _cardsController.LowGooCostOfCardInHand(_abilityUnitOwner.OwnerPlayer, null, AbilityData.GooCostReduction);
        }
    }
}
