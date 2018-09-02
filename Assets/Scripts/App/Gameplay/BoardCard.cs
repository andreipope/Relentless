// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Gameplay;
using LoomNetwork.CZB.Helpers;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class BoardCard
    {
        public int cardsAmountDeckEditing;

        public bool cardShouldBeChanged;

        public bool isNewCard;

        public bool isPreview;

        public int initialCost;

        public Card libraryCard;

        protected ILoadObjectsManager _loadObjectsManager;

        protected ISoundManager _soundManager;

        protected IDataManager _dataManager;

        protected IGameplayManager _gameplayManager;

        protected ITimerManager _timerManager;

        protected CardsController _cardsController;

        protected AbilitiesController _abilitiesController;

        protected BattlegroundController _battlegroundController;

        protected SpriteRenderer glowSprite;

        protected SpriteRenderer pictureSprite;

        protected SpriteRenderer backgroundSprite;

        protected GameObject distibuteCardObject;

        protected TextMeshPro costText;

        protected TextMeshPro nameText;

        protected TextMeshPro bodyText;

        protected TextMeshPro amountText;

        // protected GameObject previewCard;
        protected Animator cardAnimator;

        protected Vector3 positionOnHand;

        protected Vector3 rotationOnHand;

        protected Vector3 scaleOnHand;

        protected AnimationEventTriggering animationEventTriggering;

        protected OnBehaviourHandler behaviourHandler;

        protected List<ElementSlotOfCards> _elementSlotsOfCards;

        protected Transform _parentOfEditingGroupUI;

        protected List<BuffOnCardInfoObject> _buffOnCardInfoObjects;

        protected Transform _parentOfLeftBlockOfCardInfo, _parentOfRightBlockOfCardInfo;

        public BoardCard(GameObject selfObject)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            gameObject = selfObject;

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

            // previewCard = _loadObjectsManager.GetObjectByPath<GameObject>("");
            animationEventTriggering = gameObject.GetComponent<AnimationEventTriggering>();
            behaviourHandler = gameObject.GetComponent<OnBehaviourHandler>();

            animationEventTriggering.OnAnimationEvent += OnAnimationEvent;

            _cardsController.UpdateCardsStatusEvent += UpdateCardsStatusEventHandler;

            behaviourHandler.OnMouseDownEvent += OnMouseDownEventHandler;
            behaviourHandler.OnMouseUpEvent += OnMouseUpEventHandler;

            behaviourHandler.OnDestroyEvent += OnDestroyEventHandler;
        }

        public int manaCost { get; protected set; }

        public ParticleSystem removeCardParticle { get; protected set; }

        public Transform transform => gameObject.transform;

        public GameObject gameObject { get; }

        public int CurrentTurn { get; set; }

        public WorkingCard WorkingCard { get; private set; }

        public HandBoardCard HandBoardCard { get; set; }

        public virtual void Init(WorkingCard card)
        {
            WorkingCard = card;

            libraryCard = WorkingCard.libraryCard;

            nameText.text = libraryCard.name;
            bodyText.text = libraryCard.description;
            costText.text = libraryCard.cost.ToString();

            isNewCard = true;

            initialCost = WorkingCard.initialCost;
            manaCost = initialCost;

            WorkingCard.owner.PlayerGooChangedEvent += PlayerGooChangedEventHandler;

            string rarity = Enum.GetName(typeof(Enumerators.CardRank), WorkingCard.libraryCard.cardRank);

            string setName = libraryCard.cardSetType.ToString();

            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(libraryCard.frame))
            {
                frameName = "Images/Cards/Frames/" + libraryCard.frame;
            }

            backgroundSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
            pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), WorkingCard.libraryCard.picture.ToLower()));

            amountText.transform.parent.gameObject.SetActive(false);
            distibuteCardObject.SetActive(false);

            if (libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                _parentOfLeftBlockOfCardInfo = transform.Find("Group_LeftBlockInfo");
                _parentOfRightBlockOfCardInfo = transform.Find("Group_RightBlockInfo");

                if (!InternalTools.IsTabletScreen())
                {
                    _parentOfLeftBlockOfCardInfo.transform.localScale = new Vector3(.7f, .7f, .7f);
                    _parentOfLeftBlockOfCardInfo.transform.localPosition = new Vector3(10f, 6.8f, 0f);

                    _parentOfRightBlockOfCardInfo.transform.localScale = new Vector3(.7f, .7f, .7f);
                    _parentOfRightBlockOfCardInfo.transform.localPosition = new Vector3(17f, 6.8f, 0f);
                }
            }
        }

        public virtual void Init(Card card, int amount = 0)
        {
            libraryCard = card;

            nameText.text = libraryCard.name;
            bodyText.text = libraryCard.description;
            amountText.text = amount.ToString();
            costText.text = libraryCard.cost.ToString();

            initialCost = libraryCard.cost;
            manaCost = initialCost;

            string rarity = Enum.GetName(typeof(Enumerators.CardRank), card.cardRank);

            string setName = libraryCard.cardSetType.ToString();

            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(libraryCard.frame))
            {
                frameName = "Images/Cards/Frames/" + libraryCard.frame;
            }

            backgroundSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);

            pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), card.picture.ToLower()));

            distibuteCardObject.SetActive(false);
        }

        public void SetCardCost(int value, bool changeRealCost = false)
        {
            if (changeRealCost)
            {
                WorkingCard.libraryCard.cost = value;
                WorkingCard.realCost = value;
                manaCost = WorkingCard.realCost;
                costText.text = manaCost.ToString();
            } else
            {
                manaCost = value;
                costText.text = manaCost.ToString();
            }

            UpdateColorOfCost();
        }

        public void ChangeCardCostOn(int value, bool changeRealCost = false)
        {
            if (changeRealCost)
            {
                WorkingCard.libraryCard.cost += value;
                WorkingCard.realCost += value;
                manaCost = WorkingCard.realCost;
                costText.text = manaCost.ToString();
            } else
            {
                manaCost = WorkingCard.realCost + value;
                costText.text = manaCost.ToString();
            }

            UpdateColorOfCost();
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
            } else if (CurrentTurn != 0)
            {
                cardAnimator.enabled = true;
                cardAnimator.SetTrigger("DeckToHand");

                _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
            }

            isNewCard = false;
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
            {
                cardAnimator.SetFloat("Id", 2);
            } else
            {
                cardAnimator.SetFloat("Id", id);
            }

            _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_MULTIPLE, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
        }

        public virtual void OnAnimationEvent(string name)
        {
            switch (name)
            {
                case "DeckToHandEnd":
                    cardAnimator.enabled = false;

                    if (!_cardsController.CardDistribution)
                    {
                        UpdatePositionOnHand();
                    }

                    break;
            }
        }

        public virtual bool CanBePlayed(Player owner)
        {
#if !DEV_MODE
            return _gameplayManager.GetController<PlayerController>().IsActive; // && owner.manaStat.effectiveValue >= manaCost;
#else
            return true;
#endif
        }

        public virtual bool CanBeBuyed(Player owner)
        {
#if !DEV_MODE
            return owner.Goo >= manaCost;
#else
            return true;
#endif
        }

        public void IsHighlighted()
        {
            // return glowSprite.enabled;
        }

        public void SetHighlightingEnabled(bool enabled)
        {
            if ((glowSprite != null) && glowSprite)
            {
                glowSprite.enabled = enabled;
            }
        }

        public void Dispose()
        {
            Object.Destroy(gameObject);
        }

        public void ReturnCardToDeck()
        {
            if (!_cardsController.CardDistribution)

                return;

            _cardsController.ReturnCardToDeck(
                this,
                () =>
                {
                    WorkingCard.owner.DistributeCard();
                });
        }

        public void DrawCardFromOpponentDeckToPlayer()
        {
            gameObject.transform.localScale = Vector3.zero;

            gameObject.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.15f);

            cardAnimator.enabled = true;
            cardAnimator.StopPlayback();
            cardAnimator.Play("MoveCardFromOpponentDeckToPlayerHand");

            _timerManager.AddTimer(
                x =>
                {
                    cardAnimator.enabled = false;

                    _battlegroundController.playerHandCards.Add(this);

                    _battlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                },
                null,
                2f);
        }

        // editing deck page
        public void SetAmountOfCardsInEditingPage(bool init, uint maxCopies, int amount)
        {
            cardsAmountDeckEditing = amount;
            if (init)
            {
                foreach (Transform child in _parentOfEditingGroupUI)
                {
                    Object.Destroy(child.gameObject);
                }

                foreach (ElementSlotOfCards item in _elementSlotsOfCards)
                {
                    Object.Destroy(item.selfObject);
                }

                _elementSlotsOfCards.Clear();

                for (int i = 0; i < maxCopies; i++)
                {
                    _elementSlotsOfCards.Add(new ElementSlotOfCards(_parentOfEditingGroupUI, false));
                }
            }

            for (int i = 0; i < maxCopies; i++)
            {
                if (i >= _elementSlotsOfCards.Count)
                {
                    _elementSlotsOfCards.Add(new ElementSlotOfCards(_parentOfEditingGroupUI, false));
                }

                _elementSlotsOfCards[i].SetStatus(i < amount);
            }

            float offset = 0.5f;

            if (maxCopies > 3)
            {
                offset = 0.8f;
            } else if (maxCopies > 2)
            {
                offset = 0.5f;
            } else if (maxCopies > 1)
            {
                offset = 0.7f;
            }

            InternalTools.GroupHorizontalObjects(_parentOfEditingGroupUI, offset, 2f);
        }

        public void DrawTooltipInfoOfUnit(BoardUnit unit)
        {
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            _buffOnCardInfoObjects = new List<BuffOnCardInfoObject>();

            float offset = 0f;
            float spacing = -6.75f;

            BuffOnCardInfoObject buff = null;

            List<BuffTooltipInfo> buffs = new List<BuffTooltipInfo>();

            // left block info ------------------------------------
            if (unit.Card.libraryCard.cardRank != Enumerators.CardRank.MINION)
            {
                TooltipContentData.RankInfo rankInfo = _dataManager.GetRankInfoByType(unit.Card.libraryCard.cardRank.ToString());
                if (rankInfo != null)
                {
                    TooltipContentData.RankInfo.RankDescription rankDescription = rankInfo.info.Find(y => y.element.ToLower().Equals(_cardsController.GetSetOfCard(unit.Card.libraryCard).ToLower()));

                    buffs.Add(new BuffTooltipInfo { title = rankInfo.name, description = rankDescription.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.RANK, value = -1 });
                }
            }

            if ((unit.InitialUnitType != Enumerators.CardType.WALKER) && (unit.InitialUnitType != Enumerators.CardType.NONE))
            {
                TooltipContentData.BuffInfo buffInfo = _dataManager.GetBuffInfoByType(unit.InitialUnitType.ToString());
                if (buffInfo != null)
                {
                    buffs.Add(new BuffTooltipInfo { title = buffInfo.name, description = buffInfo.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.UNIT_TYPE, value = -1 });
                }
            }

            if (unit.Card.libraryCard.abilities != null)
            {
                foreach (AbilityData abil in unit.Card.libraryCard.abilities)
                {
                    TooltipContentData.BuffInfo buffInfo = _dataManager.GetBuffInfoByType(abil.buffType);
                    if (buffInfo != null)
                    {
                        buffs.Add(new BuffTooltipInfo { title = buffInfo.name, description = buffInfo.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.ABILITY, value = GetValueOfAbilityByType(abil) });
                    }
                }
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                if (i >= 3)
                {
                    break;
                }

                if (_buffOnCardInfoObjects.Find(x => x.buffTooltipInfo.title.Equals(buffs[i].title)) != null)
                {
                    continue;
                }

                buff = new BuffOnCardInfoObject(buffs[i], _parentOfLeftBlockOfCardInfo, offset + (spacing * i));

                _buffOnCardInfoObjects.Add(buff);
            }

            float cardSize = 7.2f;
            float centerOffset = -7f;

            if (!InternalTools.IsTabletScreen())
            {
                cardSize = 6.6f;
                centerOffset = -10f;
            }

            InternalTools.GroupVerticalObjects(_parentOfLeftBlockOfCardInfo, 0f, centerOffset, cardSize);

            Transform parent = buffs.Count > 0?_parentOfRightBlockOfCardInfo:_parentOfLeftBlockOfCardInfo;

            buffs.Clear();

            // right block info ------------------------------------

            // IMPROVE!!!
            foreach (AbilityBase abil in _abilitiesController.GetAbilitiesConnectedToUnit(unit))
            {
                TooltipContentData.BuffInfo buffInfo = _dataManager.GetBuffInfoByType(abil.AbilityData.buffType);
                if (buffInfo != null)
                {
                    buffs.Add(new BuffTooltipInfo { title = buffInfo.name, description = buffInfo.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.BUFF, value = -1 });
                }
            }

            // IMPROVE!!!
            foreach (Enumerators.BuffType buffOnUnit in unit.BuffsOnUnit)
            {
                TooltipContentData.BuffInfo buffInfo = _dataManager.GetBuffInfoByType(buffOnUnit.ToString());
                if (buffInfo != null)
                {
                    buffs.Add(new BuffTooltipInfo { title = buffInfo.name, description = buffInfo.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.BUFF, value = -1 });
                }
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                if (i >= 3)
                {
                    break;
                }

                if (_buffOnCardInfoObjects.Find(x => x.buffTooltipInfo.title.Equals(buffs[i].title)) != null)
                {
                    continue;
                }

                buff = new BuffOnCardInfoObject(buffs[i], parent, offset + (spacing * i));

                _buffOnCardInfoObjects.Add(buff);
            }

            buffs.Clear();

            InternalTools.GroupVerticalObjects(parent, 0f, centerOffset, cardSize);
        }

        public void DrawTooltipInfoOfCard(BoardCard boardCard)
        {
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            if (boardCard.WorkingCard.libraryCard.cardKind == Enumerators.CardKind.SPELL)

                return;

            _buffOnCardInfoObjects = new List<BuffOnCardInfoObject>();

            float offset = 0f;
            float spacing = -6.75f;

            BuffOnCardInfoObject buff = null;

            List<BuffTooltipInfo> buffs = new List<BuffTooltipInfo>();

            // left block info ------------------------------------
            if (boardCard.WorkingCard.libraryCard.cardRank != Enumerators.CardRank.MINION)
            {
                TooltipContentData.RankInfo rankInfo = _dataManager.GetRankInfoByType(boardCard.WorkingCard.libraryCard.cardRank.ToString());
                if (rankInfo != null)
                {
                    TooltipContentData.RankInfo.RankDescription rankDescription = rankInfo.info.Find(y => y.element.ToLower().Equals(_cardsController.GetSetOfCard(boardCard.WorkingCard.libraryCard).ToLower()));

                    buffs.Add(new BuffTooltipInfo { title = rankInfo.name, description = rankDescription.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.RANK, value = -1 });
                }
            }

            if ((boardCard.WorkingCard.type != Enumerators.CardType.WALKER) && (boardCard.WorkingCard.type != Enumerators.CardType.NONE))
            {
                TooltipContentData.BuffInfo buffInfo = _dataManager.GetBuffInfoByType(boardCard.WorkingCard.type.ToString());
                if (buffInfo != null)
                {
                    buffs.Add(new BuffTooltipInfo { title = buffInfo.name, description = buffInfo.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.UNIT_TYPE, value = -1 });
                }
            }

            if (boardCard.WorkingCard.libraryCard.abilities != null)
            {
                foreach (AbilityData abil in boardCard.WorkingCard.libraryCard.abilities)
                {
                    TooltipContentData.BuffInfo buffInfo = _dataManager.GetBuffInfoByType(abil.buffType);
                    if (buffInfo != null)
                    {
                        buffs.Add(new BuffTooltipInfo { title = buffInfo.name, description = buffInfo.tooltip, tooltipObjectType = Enumerators.TooltipObjectType.ABILITY, value = GetValueOfAbilityByType(abil) });
                    }
                }
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                if (i >= 3)
                {
                    break;
                }

                if (_buffOnCardInfoObjects.Find(x => x.buffTooltipInfo.title.Equals(buffs[i].title)) != null)
                {
                    continue;
                }

                buff = new BuffOnCardInfoObject(buffs[i], _parentOfLeftBlockOfCardInfo, offset + (spacing * i));

                _buffOnCardInfoObjects.Add(buff);
            }

            buffs.Clear();

            float cardSize = 7.2f;
            float centerOffset = -7f;

            if (!InternalTools.IsTabletScreen())
            {
                cardSize = 6.6f;
                centerOffset = -10f;
            }

            InternalTools.GroupVerticalObjects(_parentOfLeftBlockOfCardInfo, 0f, centerOffset, cardSize);
        }

        public void ClearBuffsOnUnit()
        {
            if (_buffOnCardInfoObjects != null)
            {
                foreach (BuffOnCardInfoObject item in _buffOnCardInfoObjects)
                {
                    item.Dispose();
                }

                _buffOnCardInfoObjects.Clear();
                _buffOnCardInfoObjects = null;
            }
        }

        protected virtual void UpdatePositionOnHand()
        {
            if (isPreview)

                return;

            transform.DOScale(scaleOnHand, 0.5f);
            transform.DOMove(positionOnHand, 0.5f);
            transform.DORotate(rotationOnHand, 0.5f);
        }

        private void OnDestroyEventHandler(GameObject obj)
        {
        }

        private void PlayerGooChangedEventHandler(int obj)
        {
            UpdateCardsStatusEventHandler(WorkingCard.owner);
        }

        private void UpdateColorOfCost()
        {
            if (manaCost > initialCost)
            {
                costText.color = Color.red;
            } else if (manaCost < initialCost)
            {
                costText.color = Color.green;
            } else
            {
                costText.color = Color.white;
            }
        }

        private void UpdateCardsStatusEventHandler(Player player)
        {
            if (isPreview)

                return;

            if (CanBePlayed(player) && CanBeBuyed(player))
            {
                SetHighlightingEnabled(true);
            } else
            {
                SetHighlightingEnabled(false);
            }
        }

        private void OnMouseUpEventHandler(GameObject obj)
        {
            if (!_cardsController.CardDistribution)
            {
            }
        }

        private void OnMouseDownEventHandler(GameObject obj)
        {
            if (!_cardsController.CardDistribution)

                return;

            cardShouldBeChanged = !cardShouldBeChanged;

            distibuteCardObject.SetActive(cardShouldBeChanged);
        }

        private int GetValueOfAbilityByType(AbilityData ability)
        {
            switch (ability.buffType)
            {
                case "DELAYED":
                    return ability.delay;
                default: return ability.value;
            }
        }

        public class BuffTooltipInfo
        {
            public string title, description;

            public Enumerators.TooltipObjectType tooltipObjectType;

            public int value;
        }

        public class BuffOnCardInfoObject
        {
            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _selfObject;

            private readonly SpriteRenderer _buffIconPicture;

            private readonly TextMeshPro _callTypeText;

            private readonly TextMeshPro _descriptionText;

            public BuffTooltipInfo buffTooltipInfo;

            public BuffOnCardInfoObject(BuffTooltipInfo buffTooltipInfo, Transform parent, float offsetY)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                this.buffTooltipInfo = buffTooltipInfo;

                _selfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tooltips/Tooltip_BuffOnCardInfo"), parent, false);

                transform.localPosition = new Vector3(0, offsetY, 0f);

                _callTypeText = _selfObject.transform.Find("Text_CallType").GetComponent<TextMeshPro>();
                _descriptionText = _selfObject.transform.Find("Text_Description").GetComponent<TextMeshPro>();

                _buffIconPicture = _selfObject.transform.Find("Image_IconBackground/Image_BuffIcon").GetComponent<SpriteRenderer>();

                _callTypeText.text = "    " + ReplaceXByValue(buffTooltipInfo.title, buffTooltipInfo.value).ToUpper();
                _descriptionText.text = ReplaceXByValue(buffTooltipInfo.description, buffTooltipInfo.value);

                switch (buffTooltipInfo.tooltipObjectType)
                {
                    case Enumerators.TooltipObjectType.RANK:
                        _buffIconPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/IconsRanks/battleground_rank_icon_" + buffTooltipInfo.title.Replace(" ", string.Empty).ToLower() + "_large");
                        break;
                    case Enumerators.TooltipObjectType.ABILITY:
                    case Enumerators.TooltipObjectType.BUFF:
                        _buffIconPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/IconsBuffTypes/battleground_mechanic_icon_" + buffTooltipInfo.title.Replace(" ", string.Empty).ToLower() + "_large");
                        break;
                    case Enumerators.TooltipObjectType.UNIT_TYPE:
                        _buffIconPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/IconsUnitTypes/battleground_mechanic_icon_" + buffTooltipInfo.title.Replace(" ", string.Empty).ToLower() + "_large");
                        break;
                    default:
                        _buffIconPicture.sprite = null;
                        break;
                }
            }

            public Transform transform => _selfObject.transform;

            public void Dispose()
            {
                Object.Destroy(_selfObject);
            }

            private string ReplaceXByValue(string val, int intVal)
            {
                return val.Replace("X", intVal.ToString());
            }
        }

        public class ElementSlotOfCards
        {
            public GameObject selfObject;

            public GameObject usedObject, freeObject;

            public ElementSlotOfCards(Transform parent, bool used)
            {
                selfObject = Object.Instantiate(GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/Element_SlotOfCards"), parent, false);

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
                } else
                {
                    freeObject.SetActive(true);
                    usedObject.SetActive(false);
                }
            }
        }
    }
}
