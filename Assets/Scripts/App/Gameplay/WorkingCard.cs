using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class WorkingCard
    {
        private CardsController _cardsController;

        public int InstanceId;

        public Player Owner;

        public IReadOnlyCard LibraryCard;

        public Card InstanceCard;

        public WorkingCard(IReadOnlyCard cardPrototype, IReadOnlyCard card, Player player, int id = -1)
        {
            Owner = player;
            LibraryCard = new Card(cardPrototype);
            InstanceCard = new Card(card);

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
    }
}
