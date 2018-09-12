using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public abstract class ReportViewBase
    {
        public GameObject SelfObject;

        protected ILoadObjectsManager LoadObjectsManager;

        protected IGameplayManager GameplayManager;

        protected CardsController CardsController;

        protected ActionsQueueController ActionsQueueController;

        protected Image PreviewImage;

        protected GameActionReport GameAction;

        protected GameObject PlayerAvatarPreviewPrefab;

        protected GameObject AttackingHealthPrefab;

        protected GameObject AttackingPictureObject;

        protected GameObject HealPictureObject;

        private GameObject _reportActionPreviewPanel;

        public ReportViewBase(GameObject prefab, Transform parent, GameActionReport gameAction)
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();
            ActionsQueueController = GameplayManager.GetController<ActionsQueueController>();
            CardsController = GameplayManager.GetController<CardsController>();

            GameAction = gameAction;
            SelfObject = Object.Instantiate(prefab, parent, false);
            SelfObject.transform.SetSiblingIndex(0);
            PreviewImage = SelfObject.transform.Find("Image").GetComponent<Image>();

            OnBehaviourHandler behaviour = SelfObject.GetComponent<OnBehaviourHandler>();
            behaviour.PointerEntered += PointerEnteredHandler;
            behaviour.PointerExited += PointerExitedHandler;

            PlayerAvatarPreviewPrefab =
                LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/PlayerAvatarPreview");
            AttackingHealthPrefab = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/AttackingHealth");

            CreatePreviewPanel();
        }

        public virtual void SetInfo()
        {
        }

        public void PointerExitedHandler(PointerEventData obj)
        {
            _reportActionPreviewPanel.SetActive(false);
        }

        public void PointerEnteredHandler(PointerEventData obj)
        {
            _reportActionPreviewPanel.SetActive(true);
        }

        public GameObject CreateCardPreview(WorkingCard card, Vector3 pos)
        {
            BoardCard boardCard;
            GameObject currentBoardCard;
            CardsController.GetSetOfCard(card.LibraryCard);

            switch (card.LibraryCard.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    currentBoardCard = Object.Instantiate(CardsController.CreatureCardViewPrefab,
                        _reportActionPreviewPanel.transform, false);
                    boardCard = new UnitBoardCard(currentBoardCard);
                    break;
                case Enumerators.CardKind.SPELL:
                    currentBoardCard = Object.Instantiate(CardsController.SpellCardViewPrefab,
                        _reportActionPreviewPanel.transform, false);
                    boardCard = new SpellBoardCard(currentBoardCard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            boardCard.Init(card);
            boardCard.SetHighlightingEnabled(false);
            boardCard.IsPreview = true;
            currentBoardCard.transform.localPosition = pos;
            currentBoardCard.transform.localRotation = Quaternion.Euler(Vector3.zero);
            currentBoardCard.transform.localScale = new Vector2(.4f, .4f);
            currentBoardCard.GetComponent<SortingGroup>().sortingOrder = 1000;
            currentBoardCard.layer = LayerMask.NameToLayer("Ignore Raycast");

            return currentBoardCard;
        }

        public GameObject CreatePlayerPreview(Player player, Vector3 pos)
        {
            GameObject avatar =
                Object.Instantiate(PlayerAvatarPreviewPrefab, _reportActionPreviewPanel.transform, false);
            SpriteRenderer sprite = avatar.transform.Find("Hero").GetComponent<SpriteRenderer>();
            Sprite heroSprite =
                LoadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/CZB_2D_Hero_Portrait_" +
                    player.SelfHero.HeroElement + "_EXP");
            sprite.sprite = heroSprite;
            TextMeshPro hpText = avatar.transform.Find("LivesCircle/DefenceText").GetComponent<TextMeshPro>();
            hpText.text = player.Health.ToString();
            avatar.transform.localPosition = pos;
            avatar.transform.localScale = Vector3.one * 1.6f;
            avatar.GetComponent<SortingGroup>().sortingOrder = 1000;
            avatar.layer = LayerMask.NameToLayer("Ignore Raycast");

            return avatar;
        }

        public void Dispose()
        {
            Object.Destroy(_reportActionPreviewPanel);
            Object.Destroy(SelfObject);
        }

        private void CreatePreviewPanel()
        {
            _reportActionPreviewPanel =
                Object.Instantiate(
                    LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/ContainerGameReportView"));
            _reportActionPreviewPanel.transform.position = new Vector3(-6, 1, 0);
            _reportActionPreviewPanel.SetActive(false);

            AttackingPictureObject = _reportActionPreviewPanel.transform.Find("PictureAttack").gameObject;
            AttackingPictureObject.SetActive(false);

            HealPictureObject = _reportActionPreviewPanel.transform.Find("PictureHeal").gameObject;
            HealPictureObject.SetActive(false);

            SetInfo();
        }
    }
}
