// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public abstract class ReportViewBase
    {
        public GameObject selfObject;

        protected ILoadObjectsManager loadObjectsManager;
        protected IGameplayManager gameplayManager;
        protected CardsController cardsController;
        protected ActionsQueueController actionsQueueController;

        protected Image previewImage;

        protected GameActionReport gameAction;

        protected GameObject playerAvatarPreviewPrefab;
        protected GameObject attackingHealthPrefab;

        private GameObject reportActionPreviewPanel;

        public ReportViewBase() { }

        public ReportViewBase(GameObject prefab, Transform parent, GameActionReport gameAction)
        {
            loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            gameplayManager = GameClient.Get<IGameplayManager>();
            actionsQueueController = gameplayManager.GetController<ActionsQueueController>();
            cardsController = gameplayManager.GetController<CardsController>();

            this.gameAction = gameAction;
            selfObject = MonoBehaviour.Instantiate(prefab, parent, false);
            previewImage = selfObject.transform.Find("Image").GetComponent<Image>();

            reportActionPreviewPanel = MonoBehaviour.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/ContainerGameReportView"));//new GameObject(gameAction.actionType.ToString());
            reportActionPreviewPanel.transform.position = new Vector3(-6, 1, 0);
            reportActionPreviewPanel.SetActive(false);


            var behaviour = selfObject.GetComponent<OnBehaviourHandler>();
            behaviour.OnPointerEnterEvent += OnPointerEnterEventHandler;
            behaviour.OnPointerExitEvent += OnPointerExitEventHandler;

            playerAvatarPreviewPrefab = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/PlayerAvatarPreview");
            attackingHealthPrefab = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/AttackingHealth");

            SetInfo();
        }

        public virtual void SetInfo()
        {

        }

        public virtual void OnPointerExitEventHandler(PointerEventData obj)
        {
            if (reportActionPreviewPanel != null && reportActionPreviewPanel) // hack delete it!
                reportActionPreviewPanel.SetActive(false);
        }

        public virtual void OnPointerEnterEventHandler(PointerEventData obj)
        {
            if(reportActionPreviewPanel != null && reportActionPreviewPanel) // hack delete it!
            reportActionPreviewPanel.SetActive(true);
        }

        public GameObject CreateCardPreview(WorkingCard card, Vector3 pos, bool highlight)
        {
            GameObject currentBoardCard = null;
            string cardSetName = string.Empty;
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            BoardCard boardCard = null;
            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                currentBoardCard = MonoBehaviour.Instantiate(cardsController.creatureCardViewPrefab, reportActionPreviewPanel.transform, false);
                boardCard = new UnitBoardCard(currentBoardCard);

            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                currentBoardCard = MonoBehaviour.Instantiate(cardsController.spellCardViewPrefab, reportActionPreviewPanel.transform, false);
                boardCard = new SpellBoardCard(currentBoardCard);
            }

            boardCard.Init(card, cardSetName);
            if (highlight)
                highlight = boardCard.CanBePlayed(card.owner) && boardCard.CanBeBuyed(card.owner);
            boardCard.SetHighlightingEnabled(highlight);
            boardCard.isPreview = true;

            //var newPos = pos;
            //newPos.y += 2.0f;
            currentBoardCard.transform.localPosition = pos;
            currentBoardCard.transform.localRotation = Quaternion.Euler(Vector3.zero);
            currentBoardCard.transform.localScale = new Vector2(.4f, .4f);
            currentBoardCard.GetComponent<SortingGroup>().sortingOrder = 1000;
            currentBoardCard.layer = LayerMask.NameToLayer("Ignore Raycast");

            return currentBoardCard;
            //currentCardPreview.transform.DOMoveY(newPos.y + 1.0f, 0.1f);
        }

        public GameObject CreatePlayerPreview(Player player)
        {
            GameObject avatar = MonoBehaviour.Instantiate(playerAvatarPreviewPrefab, reportActionPreviewPanel.transform, false);
            var sprite = avatar.transform.Find("Hero").GetComponent<SpriteRenderer>();
            var heroSprite = loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/CZB_2D_Hero_Portrait_" + player.SelfHero.heroElement.ToString() + "_EXP");
            sprite.sprite = heroSprite;
            var hpText = avatar.transform.Find("LivesCircle/DefenceText").GetComponent<TextMeshPro>();
            hpText.text = player.HP.ToString();
            avatar.transform.localPosition = new Vector3(5f, 0, 0);
            avatar.transform.localScale = Vector3.one * 1.6f;
            avatar.GetComponent<SortingGroup>().sortingOrder = 1000;
            avatar.layer = LayerMask.NameToLayer("Ignore Raycast");

            return avatar;
        }

        public virtual void Dispose()
        {
            MonoBehaviour.Destroy(reportActionPreviewPanel);
        }
    }
}
