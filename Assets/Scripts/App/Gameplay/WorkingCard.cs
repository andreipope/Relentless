using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class WorkingCard
    {
        private CardsController _cardsController;

        public int Id;

        public int CardId;

        public Card LibraryCard;

        public Player Owner;

        public int InitialHealth, InitialDamage, Health, Damage;

        public int InitialCost, RealCost;

        public Enumerators.CardType Type;

        public WorkingCard(Card card, Player player, int id = -1)
        {
            LibraryCard = card.Clone();
            CardId = LibraryCard.Id;
            Owner = player;

            InitialHealth = LibraryCard.Health;
            InitialDamage = LibraryCard.Damage;
            InitialCost = LibraryCard.Cost;
            Health = InitialHealth;
            Damage = InitialDamage;
            RealCost = InitialCost;

            Type = LibraryCard.CardType;

            _cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            if (id == -1)
            {
                Id = _cardsController.GetNewCardInstanceId();
            }
            else
            {
                Id = id;

                if(Id > _cardsController.GetCardInstanceId())
                {
                    _cardsController.SetNewCardInstanceId(Id);
                }
            }
        }

        public bool IsPlayable { get; set; }
    }
}
