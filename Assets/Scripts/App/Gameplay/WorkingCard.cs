using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class WorkingCard
    {
        private CardsController _cardsController;

        public int InstanceId;

        public Player Owner;

        public IReadOnlyCard LibraryCard;

        public CardInstanceSpecificData InstanceCard;

        public WorkingCard(IReadOnlyCard cardPrototype, IReadOnlyCard card, Player player, int id = -1)
            : this(cardPrototype, new CardInstanceSpecificData(card), player, id)
        {
        }

        public WorkingCard(IReadOnlyCard cardPrototype, CardInstanceSpecificData cardInstanceData, Player player, int id = -1)
        {
            Owner = player;
            LibraryCard = new Card(cardPrototype);
            InstanceCard = cardInstanceData;

            _cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            if (id == -1)
            {
                InstanceId = _cardsController.GetNewCardInstanceId();
            }
            else
            {
                InstanceId = id;

                if (InstanceId > _cardsController.GetCardInstanceId())
                {
                    _cardsController.SetNewCardInstanceId(InstanceId);
                }
            }
        }

        public override string ToString()
        {
            return $"{{InstanceId: {InstanceId}, Name: {LibraryCard.Name}}}";
        }
    }

}
