// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using System;
using LoomNetwork.CZB.Common;
using DG.Tweening;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;
using LoomNetwork.CZB.Helpers;

namespace LoomNetwork.CZB
{
    public class BoardCard
    {
        protected ILoadObjectsManager _loadObjectsManager;
        protected ISoundManager _soundManager;
        protected IDataManager _dataManager;
        protected IGameplayManager _gameplayManager;

        protected CardsController _cardsController;

        private GameObject _selfObject;

        protected SpriteRenderer glowSprite;
        protected SpriteRenderer pictureSprite;
        protected SpriteRenderer backgroundSprite;

        protected GameObject distibuteCardObject;

        protected TextMeshPro costText;
        protected TextMeshPro nameText;
        protected TextMeshPro bodyText;
        protected TextMeshPro amountText;

      //  protected GameObject previewCard;

        protected Animator cardAnimator;

        protected Vector3 positionOnHand;
        protected Vector3 rotationOnHand;
        protected Vector3 scaleOnHand;

        protected AnimationEventTriggering animationEventTriggering;
        protected OnBehaviourHandler behaviourHandler;

        protected List<ElementSlotOfCards> _elementSlotsOfCards;
        protected Transform _parentOfEditingGroupUI;
        public int cardsAmountDeckEditing = 0;

        public bool cardShouldBeDistributed = false;

        public bool isNewCard = false;
        public bool isPreview;

        public Card libraryCard;

        public int manaCost { get; protected set; }

        public ParticleSystem removeCardParticle { get; protected set; }

        public Transform transform { get { return _selfObject.transform; } }
        public GameObject gameObject { get { return _selfObject; } }

        public int CurrentTurn { get; set; }

        public WorkingCard WorkingCard { get; private set; }

        public HandBoardCard HandBoardCard { get; set; }

        public BoardCard(GameObject selfObject)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();

            _selfObject = selfObject;

            _elementSlotsOfCards = new List<ElementSlotOfCards>();

            cardAnimator = gameObject.GetComponent<Animator>();
            cardAnimator.enabled = false;

            glowSprite = transform.Find("Glow").GetComponent<SpriteRenderer>();
            pictureSprite = transform.Find("Picture").GetComponent<SpriteRenderer>();
            backgroundSprite = transform.Find("Frame").GetComponent<SpriteRenderer>();

            costText = transform.Find("GooText").GetComponent<TextMeshPro>();
            nameText = transform.Find("TitleText").GetComponent<TextMeshPro>();
            bodyText = transform.Find("BodyText").GetComponent<TextMeshPro>();
            amountText = transform.Find("Amount/Text").GetComponent<TextMeshPro>();

            removeCardParticle = transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>();

            distibuteCardObject = transform.Find("DistributeCardObject").gameObject;

            _parentOfEditingGroupUI = transform.Find("DeckEditingGroupUI");

            //   previewCard = _loadObjectsManager.GetObjectByPath<GameObject>("");

            animationEventTriggering = _selfObject.GetComponent<AnimationEventTriggering>();
            behaviourHandler = _selfObject.GetComponent<OnBehaviourHandler>();

            animationEventTriggering.OnAnimationEvent += OnAnimationEvent;

            _cardsController.UpdateCardsStatusEvent += UpdateCardsStatusEventHandler;

            behaviourHandler.OnMouseDownEvent += OnMouseDownEventHandler;
            behaviourHandler.OnMouseUpEvent += OnMouseUpEventHandler;
        }

        public virtual void Init(WorkingCard card)
        {
            WorkingCard = card;

            libraryCard = WorkingCard.libraryCard;

            nameText.text = libraryCard.name;
            bodyText.text = libraryCard.description;
            costText.text = libraryCard.cost.ToString();

            isNewCard = true;

            manaCost = libraryCard.cost;

            WorkingCard.owner.PlayerGooChangedEvent += PlayerGooChangedEventHandler;

            var rarity = Enum.GetName(typeof(Enumerators.CardRank), WorkingCard.libraryCard.cardRank);

            var setName = libraryCard.cardSetType.ToString();

            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(libraryCard.frame))
                frameName = "Images/Cards/Frames/" + libraryCard.frame;

            backgroundSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
            pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), WorkingCard.libraryCard.picture.ToLower()));

            amountText.transform.parent.gameObject.SetActive(false);
        }

        private void PlayerGooChangedEventHandler(int obj)
        {
            UpdateCardsStatusEventHandler(WorkingCard.owner);
        }

        public virtual void Init(Card card, int amount = 0)
        {
            libraryCard = card;

            nameText.text = libraryCard.name;
            bodyText.text = libraryCard.description;
            amountText.text = amount.ToString();
            costText.text = libraryCard.cost.ToString();

            manaCost = libraryCard.cost;

            var rarity = Enum.GetName(typeof(Enumerators.CardRank), card.cardRank);

            var setName = libraryCard.cardSetType.ToString();

            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(libraryCard.frame))
                frameName = "Images/Cards/Frames/" + libraryCard.frame;

            backgroundSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);

            pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), card.picture.ToLower()));
        }


        public void SetCardCost(int cost)
        {
            libraryCard.cost = cost;
            manaCost = libraryCard.cost;
            costText.text = manaCost.ToString();
        }

        public virtual void UpdateAmount(int amount)
        {
            amountText.text = amount.ToString();
        }

        public virtual void UpdateCardPositionInHand(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (isPreview)
                return;

            positionOnHand = position;
            rotationOnHand = rotation;
            scaleOnHand = scale;

            if (!isNewCard)
            {
                UpdatePositionOnHand();
            }
            else if (CurrentTurn != 0)
            {
                cardAnimator.enabled = true;
                cardAnimator.SetTrigger("DeckToHand");

                _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
            }

            isNewCard = false;
        }

        protected virtual void UpdatePositionOnHand()
        {
            if (isPreview)
                return;
            transform.DOScale(scaleOnHand, 0.5f);
            transform.DOMove(positionOnHand, 0.5f);
            transform.DORotate(rotationOnHand, 0.5f);
        }

        public virtual void MoveCardFromDeckToCenter()
        {
            cardAnimator.enabled = true;
            cardAnimator.SetTrigger("DeckToCenterDistribute");

            _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_MULTIPLE, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
        }

        public virtual void SetDefaultAnimation(int id)
        {
            if (isPreview)
                return;

            cardAnimator.enabled = true;
            cardAnimator.SetTrigger("DeckToHandDefault");

            if (_dataManager.CachedUserLocalData.tutorial)
                cardAnimator.SetFloat("Id", 2);
            else
                cardAnimator.SetFloat("Id", id);

            _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_MULTIPLE, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
        }

        public virtual void OnAnimationEvent(string name)
        {
            switch (name)
            {
                case "DeckToHandEnd":
                    cardAnimator.enabled = false;

                    if(!_cardsController.CardDistribution)
                        UpdatePositionOnHand();
                    break;
                default:
                    break;
            }
        }

        public virtual bool CanBePlayed(Player owner)
        {
            if (Constants.DEV_MODE)
                return true;

            return _gameplayManager.GetController<PlayerController>().IsActive;// && owner.manaStat.effectiveValue >= manaCost;
        }

        public virtual bool CanBeBuyed(Player owner)
        {
            if (Constants.DEV_MODE)
                return true;

            return owner.Goo >= manaCost;
        }

        public void IsHighlighted()
        {
            //return glowSprite.enabled;
        }

        public void SetHighlightingEnabled(bool enabled)
        {
            if (glowSprite != null && glowSprite)
                glowSprite.enabled = enabled;
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_selfObject);
        }


        private void UpdateCardsStatusEventHandler(Player player)
        {
            if (isPreview)
                return;

            if (CanBePlayed(player) && CanBeBuyed(player))
                SetHighlightingEnabled(true);
            else
                SetHighlightingEnabled(false);
        }

        public void ReturnCardToDeck()
        {
            if (!_cardsController.CardDistribution)
                return;

            _cardsController.ReturnCardToDeck(this, () =>
            {
                WorkingCard.owner.DistributeCard();
            });
        }

        // editing deck page
        public void SetAmountOfCardsInEditingPage(bool init, uint maxCopies, int amount)
        {
            cardsAmountDeckEditing = amount;
            if (init)
            {
                foreach (Transform child in _parentOfEditingGroupUI)
                {
                    MonoBehaviour.Destroy(child.gameObject);
                }
                foreach (var item in _elementSlotsOfCards)
                    MonoBehaviour.Destroy(item.selfObject);
                _elementSlotsOfCards.Clear();

                for (int i = 0; i < maxCopies; i++)
                    _elementSlotsOfCards.Add(new ElementSlotOfCards(_parentOfEditingGroupUI, false));
            }

            for (int i = 0; i < maxCopies; i++)
            {
                if (i >= _elementSlotsOfCards.Count)
                    _elementSlotsOfCards.Add(new ElementSlotOfCards(_parentOfEditingGroupUI, false));

                _elementSlotsOfCards[i].SetStatus(i < amount);
            }

            float offset = 0.5f;

            if (maxCopies > 3)
                offset = 0.8f;
            else if (maxCopies > 2)
                offset = 0.5f;
            else if (maxCopies > 1)
                offset = 0.7f;

            InternalTools.GroupHorizontalObjects(_parentOfEditingGroupUI, offset, 2f);
        }

        private void OnMouseUpEventHandler(GameObject obj)
        {
            if (!_cardsController.CardDistribution)
                return;
        }

        private void OnMouseDownEventHandler(GameObject obj)
        {
            if (!_cardsController.CardDistribution)
                return;

            cardShouldBeDistributed = !cardShouldBeDistributed;

            distibuteCardObject.SetActive(cardShouldBeDistributed);
        }


        public class ElementSlotOfCards
        {
            public GameObject selfObject;

            public GameObject usedObject,
                              freeObject;

            public ElementSlotOfCards(Transform parent, bool used)
            {
                selfObject = MonoBehaviour.Instantiate(GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/Element_SlotOfCards"), parent, false);

                freeObject = selfObject.transform.Find("Object_Free").gameObject;
                usedObject = selfObject.transform.Find("Object_Used").gameObject;

                SetStatus(used);
            }

            public void SetStatus(bool used)
            {
                if (used)
                {
                    freeObject.SetActive(false);
                    usedObject.SetActive(true);
                }
                else
                {
                    freeObject.SetActive(true);
                    usedObject.SetActive(false);
                }
            }
        }
    }
}