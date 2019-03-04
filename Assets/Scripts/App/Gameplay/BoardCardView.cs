using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.View;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class BoardCardView : IView
    {
        public int CardsAmountDeckEditing;

        public bool CardShouldBeChanged;

        public bool IsNewCard;

        public bool IsPreview;

        protected const float cardToHandSoundKoef = 2f;

        protected ILoadObjectsManager LoadObjectsManager;

        protected ISoundManager SoundManager;

        protected IDataManager DataManager;

        protected IGameplayManager GameplayManager;

        protected ITimerManager TimerManager;

        protected CardsController CardsController;

        protected AbilitiesController AbilitiesController;

        protected BattlegroundController BattlegroundController;

        protected PlayerController PlayerController;

        protected GameObject GlowObject;

        protected SpriteRenderer BackgroundSprite;

        protected GameObject DistibuteCardObject;

        protected TextMeshPro CostText;

        protected TextMeshPro NameText;

        protected TextMeshPro BodyText;

        protected TextMeshPro AmountText;

        protected TextMeshPro AmountTextForArmy;

        protected Transform IconsForArmyPanel;

        protected Animator CardAnimator;

        protected Vector3 PositionOnHand;

        protected Vector3 RotationOnHand;

        protected Vector3 ScaleOnHand;

        protected AnimationEventTriggering AnimationEventTriggering;

        protected OnBehaviourHandler BehaviourHandler;

        protected List<ElementSlotOfCards> ElementSlotsOfCards;

        protected Transform ParentOfEditingGroupUI;

        protected List<BuffOnCardInfoObject> BuffOnCardInfoObjects;

        protected Transform ParentOfLeftBlockOfCardInfo, ParentOfRightBlockOfCardInfo;

        private bool _hasDestroyed = false;

        public BoardCardView(GameObject selfObject)
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            SoundManager = GameClient.Get<ISoundManager>();
            DataManager = GameClient.Get<IDataManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();
            TimerManager = GameClient.Get<ITimerManager>();

            CardsController = GameplayManager.GetController<CardsController>();
            AbilitiesController = GameplayManager.GetController<AbilitiesController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            PlayerController = GameplayManager.GetController<PlayerController>();

            GameObject = selfObject;

            ElementSlotsOfCards = new List<ElementSlotOfCards>();

            CardAnimator = GameObject.GetComponent<Animator>();
            CardAnimator.enabled = false;

            GlowObject = Transform.Find("GlowContainer/Glow").gameObject;
            PictureSprite = Transform.Find("Picture").GetComponent<SpriteRenderer>();
            BackgroundSprite = Transform.Find("Frame").GetComponent<SpriteRenderer>();

            CostText = Transform.Find("GooText").GetComponent<TextMeshPro>();
            NameText = Transform.Find("TitleText").GetComponent<TextMeshPro>();
            BodyText = Transform.Find("BodyText").GetComponent<TextMeshPro>();
            AmountText = Transform.Find("Amount/Text").GetComponent<TextMeshPro>();
            AmountTextForArmy = Transform.Find("AmountForArmy/Text").GetComponent<TextMeshPro>();
            IconsForArmyPanel = Transform.Find("AmountForArmy/RankIcons");

            RemoveCardParticle = Transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>();

            DistibuteCardObject = Transform.Find("DistributeCardObject").gameObject;

            ParentOfEditingGroupUI = Transform.Find("DeckEditingGroupUI");

            costHighlightObject = Transform.Find("CostHighlight").gameObject;

            AnimationEventTriggering = GameObject.GetComponent<AnimationEventTriggering>();
            BehaviourHandler = GameObject.GetComponent<OnBehaviourHandler>();

            AnimationEventTriggering.AnimationEventTriggered += OnAnimationEvent;

            CardsController.UpdateCardsStatusEvent += UpdateCardsStatusEventHandler;

            BehaviourHandler.MouseDownTriggered += MouseDownTriggeredHandler;
            BehaviourHandler.MouseUpTriggered += MouseUpTriggeredHandler;

            BehaviourHandler.Destroying += DestroyingHandler;

#if UNITY_EDITOR
            MainApp.Instance.OnDrawGizmosCalled += OnDrawGizmos;
#endif
        }

        public SpriteRenderer PictureSprite { get; protected set; }

        public ParticleSystem RemoveCardParticle { get; protected set; }

        public Transform Transform => GameObject.transform;

        public GameObject GameObject { get; }

        public GameObject costHighlightObject { get; protected set; }

        public BoardUnitModel BoardUnitModel { get; private set; }

        public HandBoardCard HandBoardCard { get; set; }

        public int FuturePositionOnBoard = 0;

        public virtual void Init(BoardUnitModel boardUnitModel)
        {
            BoardUnitModel = boardUnitModel;

            NameText.text = BoardUnitModel.Card.LibraryCard.Name;
            BodyText.text = BoardUnitModel.Card.LibraryCard.Description;
            CostText.text = BoardUnitModel.Card.LibraryCard.Cost.ToString();

            IsNewCard = true;

            BoardUnitModel.Card.Owner.PlayerCurrentGooChanged += PlayerCurrentGooChangedHandler;

            string rarity = Enum.GetName(typeof(Enumerators.CardRank), BoardUnitModel.Card.LibraryCard.CardRank);

            string setName = BoardUnitModel.Card.LibraryCard.CardSetType.ToString();

            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(BoardUnitModel.Card.LibraryCard.Frame))
            {
                frameName = "Images/Cards/Frames/" + BoardUnitModel.Card.LibraryCard.Frame;
            }

            BackgroundSprite.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(frameName);
            PictureSprite.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(string.Format(
                "Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLowerInvariant(), rarity.ToLowerInvariant(),
                BoardUnitModel.Card.LibraryCard.Picture.ToLowerInvariant()));

            AmountText.transform.parent.gameObject.SetActive(false);
            AmountTextForArmy.transform.parent.gameObject.SetActive(false);
            DistibuteCardObject.SetActive(false);

            if (BoardUnitModel.Card.LibraryCard.CardKind == Enumerators.CardKind.CREATURE)
            {
                ParentOfLeftBlockOfCardInfo = Transform.Find("Group_LeftBlockInfo");
                ParentOfRightBlockOfCardInfo = Transform.Find("Group_RightBlockInfo");

                if (!InternalTools.IsTabletScreen())
                {
                    ParentOfLeftBlockOfCardInfo.transform.localScale = new Vector3(.7f, .7f, .7f);
                    ParentOfLeftBlockOfCardInfo.transform.localPosition = new Vector3(10f, 6.8f, 0f);

                    ParentOfRightBlockOfCardInfo.transform.localScale = new Vector3(.7f, .7f, .7f);
                    ParentOfRightBlockOfCardInfo.transform.localPosition = new Vector3(17f, 6.8f, 0f);
                }
            }
        }

        public virtual void Init(IReadOnlyCard card, int amount = 0)
        {
            BoardUnitModel.Card.LibraryCard = card;

            NameText.text = BoardUnitModel.Card.LibraryCard.Name;
            BodyText.text = BoardUnitModel.Card.LibraryCard.Description;
            AmountText.text = amount.ToString();
            CostText.text = BoardUnitModel.Card.LibraryCard.Cost.ToString();

            string rarity = Enum.GetName(typeof(Enumerators.CardRank), card.CardRank);

            string setName = BoardUnitModel.Card.LibraryCard.CardSetType.ToString();

            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(BoardUnitModel.Card.LibraryCard.Frame))
            {
                frameName = "Images/Cards/Frames/" + BoardUnitModel.Card.LibraryCard.Frame;
            }

            BackgroundSprite.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(frameName);

            PictureSprite.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(string.Format(
                "Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLowerInvariant(), rarity.ToLowerInvariant(), card.Picture.ToLowerInvariant()));

            DistibuteCardObject.SetActive(false);
        }

        public void UpdateCardCost()
        {
            CostText.text = BoardUnitModel.Card.InstanceCard.Cost.ToString();
            UpdateColorOfCost();
        }
        public virtual void UpdateAmount(int amount)
        {
            AmountText.text = amount.ToString();
        }

        public virtual void UpdateCardPositionInHand(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (IsPreview)
                return;

            PositionOnHand = position;
            RotationOnHand = rotation;
            ScaleOnHand = scale;

            if (!IsNewCard)
            {
                UpdatePositionOnHand();
            }
            else if (BattlegroundController.CurrentTurn != 0)
            {
                CardAnimator.enabled = true;
                CardAnimator.SetTrigger("DeckToHand");

                SoundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.CardsMoveSoundVolume * cardToHandSoundKoef);

            }

            IsNewCard = false;
        }

        public virtual void MoveCardFromDeckToCenter()
        {
            CardAnimator.enabled = true;
            CardAnimator.SetTrigger("DeckToCenterDistribute");

            SoundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_MULTIPLE, Constants.CardsMoveSoundVolume);
        }

        public virtual void SetDefaultAnimation()
        {
            if (IsPreview)
                return;

            CardAnimator.enabled = true;
            CardAnimator.SetTrigger("DeckToHandDefault");

            if (DataManager.CachedUserLocalData.Tutorial)
            {
                CardAnimator.SetFloat("Id", 2);
            }
            else
            {
                int id = BoardUnitModel.Card.Owner.CardsInHand.Count;
                CardAnimator.SetFloat("Id", id);
            }

            SoundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_MULTIPLE, Constants.CardsMoveSoundVolume);
        }

        public virtual void OnAnimationEvent(string name)
        {
            switch (name)
            {
                case "DeckToHandEnd":
                    CardAnimator.enabled = false;

                    if (!CardsController.CardDistribution)
                    {
                        UpdatePositionOnHand();
                    }

                    break;
            }
        }

        public virtual bool CanBePlayed(Player owner)
        {
            if (!Constants.DevModeEnabled)
            {
                return PlayerController.IsActive; // && owner.manaStat.effectiveValue >= manaCost;
            }
            else
            {
                return true;
            }
        }

        public virtual bool CanBeBuyed(Player owner)
        {
            if (!Constants.DevModeEnabled)
            {
                if (GameplayManager.AvoidGooCost)
                    return true;

                return owner.CurrentGoo >= BoardUnitModel.Card.InstanceCard.Cost;
            }
            else
            {
                return true;
            }
        }

        public void SetHighlightingEnabled(bool enabled)
        {
            if (GlowObject!= null && GlowObject)
            {
                GlowObject.SetActive(enabled);
            }
        }

        public void Dispose()
        {
            _hasDestroyed = true;
            Object.Destroy(GameObject);
        }

        public void DrawCardFromOpponentDeckToPlayer()
        {
            GameObject.transform.localScale = Vector3.zero;

            GameObject.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.15f);

            CardAnimator.enabled = true;
            CardAnimator.StopPlayback();
            CardAnimator.Play("MoveCardFromOpponentDeckToPlayerHand");

            TimerManager.AddTimer(
                x =>
                {
                    CardAnimator.enabled = false;

                    BattlegroundController.PlayerHandCards.Insert(ItemPosition.End, this);
                    BattlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                },
                null,
                2f);
        }

        // editing deck page
        public void SetAmountOfCardsInEditingPage(bool init, uint maxCopies, int amount, bool isArmy = false)
        {
            CardsAmountDeckEditing = amount;
            if (init)
            {
                AmountTextForArmy.transform.parent.gameObject.SetActive(isArmy);

                foreach (Transform child in ParentOfEditingGroupUI)
                {
                    Object.Destroy(child.gameObject);
                }

                foreach (ElementSlotOfCards item in ElementSlotsOfCards)
                {
                    Object.Destroy(item.SelfObject);
                }

                ElementSlotsOfCards.Clear();

                for (int i = 0; i < maxCopies; i++)
                {
                    ElementSlotsOfCards.Add(new ElementSlotOfCards(ParentOfEditingGroupUI, false));
                }
            }

            for (int i = 0; i < maxCopies; i++)
            {
                if (i >= ElementSlotsOfCards.Count)
                {
                    ElementSlotsOfCards.Add(new ElementSlotOfCards(ParentOfEditingGroupUI, false));
                }

                ElementSlotsOfCards[i].SetStatus(i < amount);
            }

            float offset = 0;
            float spacing = 1.5f;
            float offsetY = 0f;

            if (isArmy)
            {
                offsetY = -0.17f;
                AmountTextForArmy.text = amount.ToString();
                if (BoardUnitModel.Card.LibraryCard.CardKind == Enumerators.CardKind.CREATURE)
                {
                    IconsForArmyPanel.Find("Icon_" + BoardUnitModel.Card.LibraryCard.CardRank.ToString())?.gameObject.SetActive(true);
                }
            }
            InternalTools.GroupHorizontalObjects(ParentOfEditingGroupUI, offset, spacing, offsetY);
        }

        public void DrawTooltipInfoOfUnit(BoardUnitView unit)
        {
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            BuffOnCardInfoObjects = new List<BuffOnCardInfoObject>();

            const float offset = 0f;
            const float spacing = -6.75f;

            BuffOnCardInfoObject buff;

            List<BuffTooltipInfo> buffs = new List<BuffTooltipInfo>();

            // left block info ------------------------------------
            if (unit.Model.Card.LibraryCard.CardRank != Enumerators.CardRank.MINION)
            {
                TooltipContentData.RankInfo rankInfo =
                    DataManager.GetCardRankInfo(unit.Model.Card.LibraryCard.CardRank);
                if (rankInfo != null)
                {
                    TooltipContentData.RankInfo.RankDescription rankDescription = rankInfo.Info.Find(
                        y => y.Element == unit.Model.Card.LibraryCard.CardSetType);

                    buffs.Add(
                        new BuffTooltipInfo
                        {
                            Title = rankInfo.Name,
                            Description = rankDescription.Tooltip,
                            TooltipObjectType = Enumerators.TooltipObjectType.RANK,
                            Value = -1
                        });
                }
            }

            if (unit.Model.InitialUnitType != Enumerators.CardType.WALKER)
            {
                TooltipContentData.CardTypeInfo cardTypeInfo = DataManager.GetCardTypeInfo(unit.Model.InitialUnitType);
                if (cardTypeInfo != null)
                {
                    buffs.Add(
                        new BuffTooltipInfo
                        {
                            Title = cardTypeInfo.Name,
                            Description = cardTypeInfo.Tooltip,
                            TooltipObjectType = Enumerators.TooltipObjectType.UNIT_TYPE,
                            Value = -1
                        });
                }
            }

            if (unit.Model.Card.LibraryCard.Abilities != null && !unit.Model.WasDistracted)
            {
                foreach (AbilityData abil in unit.Model.Card.LibraryCard.Abilities)
                {
                    TooltipContentData.GameMechanicInfo gameMechanicInfo = DataManager.GetGameMechanicInfo(abil.GameMechanicDescriptionType);
                    if (gameMechanicInfo != null)
                    {
                        buffs.Add(
                            new BuffTooltipInfo
                            {
                                Title = gameMechanicInfo.Name,
                                Description = gameMechanicInfo.Tooltip,
                                TooltipObjectType = Enumerators.TooltipObjectType.ABILITY,
                                Value = GetValueOfAbilityByType(abil)
                            });
                    }
                }
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                if (i >= 3)
                {
                    break;
                }

                if (BuffOnCardInfoObjects.Find(x => x.BuffTooltipInfo.Title.Equals(buffs[i].Title)) != null)
                {
                    continue;
                }

                buff = new BuffOnCardInfoObject(buffs[i], ParentOfLeftBlockOfCardInfo, offset + spacing * i);

                BuffOnCardInfoObjects.Add(buff);
            }

            float cardSize = 7.2f;
            float centerOffset = -7f;

            if (!InternalTools.IsTabletScreen())
            {
                cardSize = 6.6f;
                centerOffset = -10f;
            }

            InternalTools.GroupVerticalObjects(ParentOfLeftBlockOfCardInfo, 0f, centerOffset, cardSize);

            Transform parent = buffs.Count > 0 ? ParentOfRightBlockOfCardInfo : ParentOfLeftBlockOfCardInfo;

            buffs.Clear();

            // right block info ------------------------------------
            foreach (Enumerators.GameMechanicDescriptionType mechanicType in unit.Model.GameMechanicDescriptionsOnUnit)
            {
                TooltipContentData.GameMechanicInfo gameMechanicInfo = DataManager.GetGameMechanicInfo(mechanicType);

                if (BuffOnCardInfoObjects.Find(x => x.BuffTooltipInfo.Title == gameMechanicInfo.Name) != null)
                    continue;

                if (gameMechanicInfo != null)
                {
                    buffs.Add(
                        new BuffTooltipInfo
                        {
                            Title = gameMechanicInfo.Name,
                            Description = gameMechanicInfo.Tooltip,
                            TooltipObjectType = Enumerators.TooltipObjectType.BUFF,
                            Value = -1
                        });
                }
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                if (i >= 3)
                {
                    break;
                }

                if (BuffOnCardInfoObjects.Find(x => x.BuffTooltipInfo.Title.Equals(buffs[i].Title)) != null)
                {
                    continue;
                }

                buff = new BuffOnCardInfoObject(buffs[i], parent, offset + spacing * i);

                BuffOnCardInfoObjects.Add(buff);
            }

            buffs.Clear();

            InternalTools.GroupVerticalObjects(parent, 0f, centerOffset, cardSize);
        }

        public void DrawTooltipInfoOfCard(BoardCardView boardCard)
        {
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            if (boardCard.BoardUnitModel.Card.LibraryCard.CardKind == Enumerators.CardKind.SPELL)
                return;

            BuffOnCardInfoObjects = new List<BuffOnCardInfoObject>();

            const float offset = 0f;
            const float spacing = -6.75f;

            BuffOnCardInfoObject buff;

            List<BuffTooltipInfo> buffs = new List<BuffTooltipInfo>();

            // left block info ------------------------------------
            if (boardCard.BoardUnitModel.Card.LibraryCard.CardRank != Enumerators.CardRank.MINION)
            {
                TooltipContentData.RankInfo rankInfo = DataManager.GetCardRankInfo(boardCard.BoardUnitModel.Card.LibraryCard.CardRank);
                if (rankInfo != null)
                {
                    TooltipContentData.RankInfo.RankDescription rankDescription = rankInfo.Info.Find(
                        y => y.Element == boardCard.BoardUnitModel.Card.LibraryCard.CardSetType);

                    buffs.Add(
                        new BuffTooltipInfo
                        {
                            Title = rankInfo.Name,
                            Description = rankDescription.Tooltip,
                            TooltipObjectType = Enumerators.TooltipObjectType.RANK,
                            Value = -1
                        });
                }
            }

            if (boardCard.BoardUnitModel.Card.InstanceCard.CardType != Enumerators.CardType.WALKER)
            {
                TooltipContentData.CardTypeInfo cardTypeInfo = DataManager.GetCardTypeInfo(boardCard.BoardUnitModel.Card.InstanceCard.CardType);
                if (cardTypeInfo != null)
                {
                    buffs.Add(
                        new BuffTooltipInfo
                        {
                            Title = cardTypeInfo.Name,
                            Description = cardTypeInfo.Tooltip,
                            TooltipObjectType = Enumerators.TooltipObjectType.UNIT_TYPE,
                            Value = -1
                        });
                }
            }

            if (boardCard.BoardUnitModel.Card.LibraryCard.Abilities != null)
            {
                foreach (AbilityData abil in boardCard.BoardUnitModel.Card.LibraryCard.Abilities)
                {
                    TooltipContentData.GameMechanicInfo gameMechanicInfo = DataManager.GetGameMechanicInfo(abil.GameMechanicDescriptionType);
                    if (gameMechanicInfo != null)
                    {
                        buffs.Add(
                            new BuffTooltipInfo
                            {
                                Title = gameMechanicInfo.Name,
                                Description = gameMechanicInfo.Tooltip,
                                TooltipObjectType = Enumerators.TooltipObjectType.ABILITY,
                                Value = GetValueOfAbilityByType(abil)
                            });
                    }
                }
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                if (i >= 3)
                {
                    break;
                }

                if (BuffOnCardInfoObjects.Find(x => x.BuffTooltipInfo.Title.Equals(buffs[i].Title)) != null)
                {
                    continue;
                }

                buff = new BuffOnCardInfoObject(buffs[i], ParentOfLeftBlockOfCardInfo, offset + spacing * i);

                BuffOnCardInfoObjects.Add(buff);
            }

            buffs.Clear();

            float cardSize = 7.2f;
            float centerOffset = -7f;

            if (!InternalTools.IsTabletScreen())
            {
                cardSize = 6.6f;
                centerOffset = -10f;
            }

            InternalTools.GroupVerticalObjects(ParentOfLeftBlockOfCardInfo, 0f, centerOffset, cardSize);
        }

        protected void UpdatePositionOnHand()
        {
            if (IsPreview || _hasDestroyed)
                return;

            Transform.DOScale(ScaleOnHand, 0.5f);
            Transform.DOMove(PositionOnHand, 0.5f);
            Transform.DORotate(RotationOnHand, 0.5f);
        }

        private void DestroyingHandler(GameObject obj)
        {
        }

        private void PlayerCurrentGooChangedHandler(int obj)
        {
            UpdateCardsStatusEventHandler(BoardUnitModel.Card.Owner);
        }

        private void UpdateColorOfCost()
        {
            if (BoardUnitModel.Card.InstanceCard.Cost > BoardUnitModel.Card.LibraryCard.Cost)
            {
                CostText.color = Color.red;
            }
            else if (BoardUnitModel.Card.InstanceCard.Cost < BoardUnitModel.Card.LibraryCard.Cost)
            {
                CostText.color = Color.green;
            }
            else
            {
                CostText.color = Color.white;
            }
        }

        private void UpdateCardsStatusEventHandler(Player player)
        {
            if (IsPreview)
                return;

            if (CanBePlayed(player) && CanBeBuyed(player))
            {
                SetHighlightingEnabled(true);
            }
            else
            {
                SetHighlightingEnabled(false);
            }
        }

        private void MouseUpTriggeredHandler(GameObject obj)
        {
            if (!CardsController.CardDistribution)
            {
            }
        }

        private void MouseDownTriggeredHandler(GameObject obj)
        {
            if (!CardsController.CardDistribution)
                return;

            CardShouldBeChanged = !CardShouldBeChanged;

            DistibuteCardObject.SetActive(CardShouldBeChanged);
        }

        private int GetValueOfAbilityByType(AbilityData ability)
        {
            switch (ability.GameMechanicDescriptionType)
            {
                case Enumerators.GameMechanicDescriptionType.DelayedX:
                    return ability.Delay;
                default:
                    return ability.Value;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (GameObject == null)
            {
                MainApp.Instance.OnDrawGizmosCalled -= OnDrawGizmos;
                return;
            }

            if (BoardUnitModel.Card == null)
                return;

            DebugCardInfoDrawer.Draw(Transform.position, BoardUnitModel.Card.InstanceId.Id, BoardUnitModel.Card.LibraryCard.Name);
        }
#endif

        public class BuffTooltipInfo
        {
            public string Title, Description;

            public Enumerators.TooltipObjectType TooltipObjectType;

            public int Value;
        }

        public class BuffOnCardInfoObject
        {
            public BuffTooltipInfo BuffTooltipInfo;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _selfObject;

            private readonly SpriteRenderer _buffIconPicture;

            private readonly TextMeshPro _callTypeText;

            private readonly TextMeshPro _descriptionText;

            public BuffOnCardInfoObject(BuffTooltipInfo buffTooltipInfo, Transform parent, float offsetY)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                BuffTooltipInfo = buffTooltipInfo;

                _selfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/Gameplay/Tooltips/Tooltip_BuffOnCardInfo"), parent, false);

                Transform.localPosition = new Vector3(0, offsetY, 0f);

                _callTypeText = _selfObject.transform.Find("Text_CallType").GetComponent<TextMeshPro>();
                _descriptionText = _selfObject.transform.Find("Text_Description").GetComponent<TextMeshPro>();

                _buffIconPicture = _selfObject.transform.Find("Image_IconBackground/Image_BuffIcon")
                    .GetComponent<SpriteRenderer>();

                _callTypeText.text = "    " + ReplaceXByValue(buffTooltipInfo.Title, buffTooltipInfo.Value).ToUpperInvariant();
                _descriptionText.text = ReplaceXByValue(buffTooltipInfo.Description, buffTooltipInfo.Value);

                switch (buffTooltipInfo.TooltipObjectType)
                {
                    case Enumerators.TooltipObjectType.RANK:
                        _buffIconPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsRanks/battleground_rank_icon_" +
                            buffTooltipInfo.Title.Replace(" ", string.Empty).ToLowerInvariant() + "_large");
                        break;
                    case Enumerators.TooltipObjectType.ABILITY:
                    case Enumerators.TooltipObjectType.BUFF:
                        _buffIconPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsBuffTypes/battleground_mechanic_icon_" +
                            buffTooltipInfo.Title.Replace(" ", string.Empty).ToLowerInvariant() + "_large");
                        break;
                    case Enumerators.TooltipObjectType.UNIT_TYPE:
                        _buffIconPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsUnitTypes/battleground_mechanic_icon_" +
                            buffTooltipInfo.Title.Replace(" ", string.Empty).ToLowerInvariant() + "_large");
                        break;
                    default:
                        _buffIconPicture.sprite = null;
                        break;
                }
            }

            public Transform Transform => _selfObject.transform;

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
            public GameObject SelfObject;

            public GameObject UsedObject, FreeObject;

            public ElementSlotOfCards(Transform parent, bool used)
            {
                SelfObject =
                    Object.Instantiate(
                        GameClient.Get<ILoadObjectsManager>()
                            .GetObjectByPath<GameObject>("Prefabs/Gameplay/Element_SlotOfCards"), parent, false);

                FreeObject = SelfObject.transform.Find("Object_Free").gameObject;
                UsedObject = SelfObject.transform.Find("Object_Used").gameObject;

                SetStatus(used);
            }

            public void SetStatus(bool used)
            {
                if (used)
                {
                    FreeObject.SetActive(false);
                    UsedObject.SetActive(true);
                }
                else
                {
                    FreeObject.SetActive(true);
                    UsedObject.SetActive(false);
                }
            }
        }
    }
}
