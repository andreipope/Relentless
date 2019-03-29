using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class WorkingCard
    {
        public Player Owner { get; set; }

        public IReadOnlyCard Prototype { get; }

        public CardInstanceSpecificData InstanceCard { get; }

        public InstanceId InstanceId { get; }

        public int TutorialObjectId { get; set; }

        public WorkingCard(IReadOnlyCard cardPrototype, IReadOnlyCard card, Player player, InstanceId? id = null)
            : this(cardPrototype, new CardInstanceSpecificData(card), player, id)
        {
        }

        public WorkingCard(IReadOnlyCard cardPrototype, CardInstanceSpecificData cardInstanceData, Player player, InstanceId? id = null)
        {
            Owner = player;
            Prototype = new Card(cardPrototype);
            InstanceCard = cardInstanceData;

            CardsController cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            if (id == null)
            {
                InstanceId = cardsController.GetNewCardInstanceId();
            }
            else
            {
                InstanceId = id.Value;

                if (cardsController != null && InstanceId.Id > cardsController.GetCardInstanceId().Id)
                {
                    cardsController.SetNewCardInstanceId(InstanceId.Id);
                }
            }
        }

        public override string ToString()
        {
            return $"{{InstanceId: {InstanceId}, Name: {Prototype.Name}}}";
        }
    }
}
