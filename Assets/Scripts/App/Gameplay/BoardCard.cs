// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using System;
using LoomNetwork.CZB.Common;
using DG.Tweening;
using LoomNetwork.CZB.Data;

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

        protected TextMeshPro costText;
        protected TextMeshPro nameText;
        protected TextMeshPro bodyText;
        protected TextMeshPro amountText;

      //  protected GameObject previewCard;

        protected Animator cardAnimator;

        protected Vector3 positionOnHand;
        protected Vector3 rotationOnHand;

        protected AnimationEventTriggering animationEventTriggering;
        protected OnBehaviourHandler behaviourHandler;

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

            //   previewCard = _loadObjectsManager.GetObjectByPath<GameObject>("");

            animationEventTriggering = _selfObject.GetComponent<AnimationEventTriggering>();
            behaviourHandler = _selfObject.GetComponent<OnBehaviourHandler>();

            animationEventTriggering.OnAnimationEvent += OnAnimationEvent;

            _cardsController.UpdateCardsStatusEvent += UpdateCardsStatusEventHandler;
        }

        public virtual void Init(WorkingCard card, string setName = "")
        {
            WorkingCard = card;

            libraryCard = WorkingCard.libraryCard;

            nameText.text = libraryCard.name;
            bodyText.text = libraryCard.description;
            costText.text = libraryCard.cost.ToString();

            isNewCard = true;

            manaCost = libraryCard.cost;


            var rarity = Enum.GetName(typeof(Enumerators.CardRank), WorkingCard.libraryCard.cardRank);

            backgroundSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity));
            pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), WorkingCard.libraryCard.picture.ToLower()));

            amountText.transform.parent.gameObject.SetActive(false);
        }

        public virtual void Init(Card card, string setName = "", int amount = 0)
        {
            libraryCard = card;

            nameText.text = libraryCard.name;
            bodyText.text = libraryCard.description;
            amountText.text = amount.ToString();
            costText.text = libraryCard.cost.ToString();

            manaCost = libraryCard.cost;

            var rarity = Enum.GetName(typeof(Enumerators.CardRank), card.cardRank);

            backgroundSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity));

            pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), card.picture.ToLower()));
        }


        public virtual void UpdateAmount(int amount)
        {
            amountText.text = amount.ToString();
        }

        public virtual void UpdateCardPositionInHand(Vector3 position, Vector3 rotation)
        {
            if (isPreview)
                return;

            positionOnHand = position;
            rotationOnHand = rotation;

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

            transform.DOMove(positionOnHand, 0.5f);
            transform.DORotate(rotationOnHand, 0.5f);
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

            return owner.Mana >= manaCost;
        }

        public void IsHighlighted()
        {
            //return glowSprite.enabled;
        }

        public void SetHighlightingEnabled(bool enabled)
        {
            if (glowSprite != null && glowSprite)
            {
                glowSprite.enabled = enabled;
            }
            else
            {
                Debug.Log(_selfObject + " glow doesnt exists ");
            }
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
    }
}