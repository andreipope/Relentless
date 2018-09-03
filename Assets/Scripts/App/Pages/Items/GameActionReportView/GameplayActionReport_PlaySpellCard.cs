using System;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GameplayActionReportPlaySpellCard : ReportViewBase
    {
        private Player _callerPlayer;

        private BoardCard _playedCard;

        private GameObject _playedCardPreviewObject;

        public GameplayActionReportPlaySpellCard(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = GameAction.Parameters[0] as Player;

            if (GameAction.Parameters.Length > 1)
            {
                _playedCard = GameAction.Parameters[1] as BoardCard;

                string rarity = Enum.GetName(typeof(Enumerators.CardRank),
                    _playedCard.WorkingCard.LibraryCard.CardRank);
                string cardSetName = CardsController.GetSetOfCard(_playedCard.WorkingCard.LibraryCard);
                PreviewImage.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(
                    string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(),
                        _playedCard.WorkingCard.LibraryCard.Picture.ToLower()));

                _playedCardPreviewObject = CreateCardPreview(_playedCard.WorkingCard, Vector3.zero);
            }
        }
    }
}
