// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_PlaySpellCard : ReportViewBase
    {
        private Player _callerPlayer;

        private BoardCard _playedCard;

        private GameObject _playedCardPreviewObject;

        public GameplayActionReport_PlaySpellCard(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = gameAction.parameters[0] as Player;

            if (gameAction.parameters.Length > 1)
            {
                _playedCard = gameAction.parameters[1] as BoardCard;

                string rarity = Enum.GetName(typeof(Enumerators.CardRank), _playedCard.WorkingCard.libraryCard.cardRank);
                string cardSetName = cardsController.GetSetOfCard(_playedCard.WorkingCard.libraryCard);
                previewImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(), _playedCard.WorkingCard.libraryCard.picture.ToLower()));

                _playedCardPreviewObject = CreateCardPreview(_playedCard.WorkingCard, Vector3.zero);
            }
        }

        public override void OnPointerEnterEventHandler(PointerEventData obj)
        {
            base.OnPointerEnterEventHandler(obj);
        }

        public override void OnPointerExitEventHandler(PointerEventData obj)
        {
            base.OnPointerExitEventHandler(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
