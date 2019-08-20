using System;
using System.Collections.Generic;
using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Localization;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public abstract class BoardCardView : ICardView
    {
        private static readonly ILog Log = Logging.GetLog(nameof(BoardCardView));

        public int CardsAmountDeckEditing;

        public int MaxCopies;

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

        protected SpriteRenderer RankIconSprite;

        protected SpriteRenderer SetIconSprite;

        protected GameObject DistibuteCardObject;

        protected TextMeshPro CostText;

        protected TextMeshPro NameText;

        protected TextMeshPro BodyText;

        protected TextMeshPro AmountText;

        protected GameObject AmountTrayWithRadio, AmountTrayWithCounter;

        protected Animator CardAnimator;

        public Vector3 PositionOnHand { get; private set; }

        public Vector3 RotationOnHand { get; private set; }

        public Vector3 ScaleOnHand { get; private set; }

        protected AnimationEventTriggering AnimationEventTriggering;

        protected OnBehaviourHandler BehaviourHandler;

        protected GameObject BulletPointGroup;

        protected List<BuffOnCardInfoObject> BuffOnCardInfoObjects;

        protected Transform ParentOfLeftBlockOfCardInfo, ParentOfRightBlockOfCardInfo;

        private bool _hasDestroyed = false;

        public BoardCardView(GameObject selfObject, CardModel cardModel)
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

            CardAnimator = GameObject.GetComponent<Animator>();
            CardAnimator.enabled = false;

            GlowObject = Transform.Find("GlowContainer/Glow").gameObject;
            PictureSprite = Transform.Find("Picture").GetComponent<SpriteRenderer>();
            BackgroundSprite = Transform.Find("Frame").GetComponent<SpriteRenderer>();
            RankIconSprite = Transform.Find("RankIcon").GetComponent<SpriteRenderer>();
            SetIconSprite = Transform.Find("SetIcon").GetComponent<SpriteRenderer>();

            CostText = Transform.Find("GooText").GetComponent<TextMeshPro>();
            NameText = Transform.Find("TitleText").GetComponent<TextMeshPro>();
            BodyText = Transform.Find("BodyText").GetComponent<TextMeshPro>();
            AmountText = Transform.Find("AmountWithCounterTray/Text").GetComponent<TextMeshPro>();

            AmountTrayWithRadio = Transform.Find("AmountWithRadioTray").gameObject;
            AmountTrayWithCounter = Transform.Find("AmountWithCounterTray").gameObject;

            RemoveCardParticle = Transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>();

            DistibuteCardObject = Transform.Find("DistributeCardObject").gameObject;

            costHighlightObject = Transform.Find("CostHighlight").gameObject;

            AnimationEventTriggering = GameObject.GetComponent<AnimationEventTriggering>();
            BehaviourHandler = GameObject.GetComponent<OnBehaviourHandler>();

            AnimationEventTriggering.AnimationEventTriggered += OnAnimationEvent;

            CardsController.UpdateCardsStatusEvent += UpdateCardsStatusEventHandler;

            BehaviourHandler.MouseDownTriggered += MouseDownTriggeredHandler;
            BehaviourHandler.MouseUpTriggered += MouseUpTriggeredHandler;

            BehaviourHandler.Destroying += DestroyingHandler;

            Model = cardModel;

            NameText.text = Model.Card.Prototype.Name;
            BodyText.text = Model.Card.Prototype.Description;
            CostText.text = Model.Card.Prototype.Cost.ToString();

            IsNewCard = true;

            string rarity = Enum.GetName(typeof(Enumerators.CardRank), Model.Card.Prototype.Rank);

            string factionName = Model.Card.Prototype.Faction.ToString();
            string frameName = string.Format("Images/Cards/Frames/frame_{0}", factionName);
            string rankName = string.Format("Images/IconsRanks/rank_icon_{0}", rarity.ToLowerInvariant());

            if (!string.IsNullOrEmpty(Model.Card.Prototype.Frame))
            {
                frameName = "Images/Cards/Frames/" + Model.Card.Prototype.Frame;
            }

            BackgroundSprite.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(frameName);
            RankIconSprite.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(rankName);

            string setName = $"Images/IconsSet/set_icon_{Model.Card.Prototype.CardKey.Variant.ToString().ToLowerInvariant()}";
            SetIconSprite.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(setName);

            Model.CardPictureWasUpdated += PictureUpdatedEvent;
            PictureUpdatedEvent();

            SetAmount(AmountTrayType.None,0);
            DistibuteCardObject.SetActive(false);

            if (Model.Card.Prototype.Kind == Enumerators.CardKind.CREATURE)
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

            if (Model.Card.Owner != null)
            {
                Model.Card.Owner.PlayerCurrentGooChanged += PlayerCurrentGooChangedHandler;
            }

#if UNITY_EDITOR
            MainApp.Instance.OnDrawGizmosCalled += OnDrawGizmos;
#endif
        }

        public SpriteRenderer PictureSprite { get; protected set; }

        public ParticleSystem RemoveCardParticle { get; protected set; }

        public Transform Transform => GameObject.transform;

        public GameObject GameObject { get; }

        public GameObject costHighlightObject { get; protected set; }

        public CardModel Model { get; }

        public HandBoardCard HandBoardCard { get; set; }

        public enum AmountTrayType
        {
            None,
            Radio,
            Counter
        }

        public void SetAmount(AmountTrayType amountTrayType, int amount = -1, int maxCopies = -1)
        {
            AmountTrayWithRadio.SetActive(amountTrayType == AmountTrayType.Radio);
            AmountTrayWithCounter.SetActive(amountTrayType == AmountTrayType.Counter);

            switch(amountTrayType)
            {
                case AmountTrayType.Counter:
                    AmountText.text = amount.ToString();
                    break;
                case AmountTrayType.Radio:
                    if (maxCopies > 4)
                        break;

                    if (MaxCopies != maxCopies)
                    {
                        Object.Destroy(BulletPointGroup);
                        BulletPointGroup = null;
                    }
                    if(BulletPointGroup == null)
                    {
                        BulletPointGroup = Object.Instantiate
                        (
                            GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>
                            (
                                $"Prefabs/UI/Elements/BulletPoint_{maxCopies}"
                            ),
                            AmountTrayWithRadio.transform,
                            false
                        );
                    }

                    Transform bullet;
                    bool isGlow;
                    for (int i = 0; i < maxCopies; ++i)
                    {
                        bullet = BulletPointGroup.transform.Find($"Bullet_{i}");
                        isGlow = i < amount;
                        bullet.Find("Bullet_Glow").gameObject.SetActive(isGlow);
                        bullet.Find("Bullet_Normal").gameObject.SetActive(!isGlow);
                    }
                    break;
                default:
                    break;
            }

            CardsAmountDeckEditing = amount;
            MaxCopies = maxCopies;
        }

        public int FuturePositionOnBoard = 0;

        public void UpdateCardCost()
        {
            CostText.text = Model.CurrentCost.ToString();
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
                int id = Model.Card.Owner.CardsInHand.Count;
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
            Model.CardPictureWasUpdated -= PictureUpdatedEvent;
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
            if (unit.Model.Card.Prototype.Rank != Enumerators.CardRank.MINION && Constants.RankSystemEnabled)
            {
                TooltipContentData.RankInfo rankInfo =
                    DataManager.GetCardRankInfo(unit.Model.Card.Prototype.Rank);
                if (rankInfo != null)
                {
                    TooltipContentData.RankInfo.RankDescription rankDescription = rankInfo.Info.Find(
                        y => y.Element == unit.Model.Card.Prototype.Faction);

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
                            Description = Enum.TryParse($"GameData_BuffsToolTip_{LocalizationUtil.CapitalizedText(cardTypeInfo.Type.ToString())}", out LocalizationTerm term) ?
                                    LocalizationUtil.GetLocalizedString(term, cardTypeInfo.Tooltip) :
                                    cardTypeInfo.Tooltip,
                            TooltipObjectType = Enumerators.TooltipObjectType.UNIT_TYPE,
                            Value = -1
                        });
                }
            }

            if (unit.Model.Card.InstanceCard.Abilities != null && !unit.Model.WasDistracted)
            {
                foreach (AbilityData abil in unit.Model.Card.InstanceCard.Abilities)
                {
                    if (abil.GameMechanicDescription == Enumerators.GameMechanicDescription.Reanimate && unit.Model.IsReanimated)
                        continue;

                    TooltipContentData.GameMechanicInfo gameMechanicInfo = DataManager.GetGameMechanicInfo(abil.GameMechanicDescription);
                    if (gameMechanicInfo != null)
                    {
                        buffs.Add(
                            new BuffTooltipInfo
                            {
                                Title = gameMechanicInfo.Name,
                                Description = Enum.TryParse($"GameData_BuffsToolTip_{LocalizationUtil.CapitalizedText(gameMechanicInfo.Type.ToString())}", out LocalizationTerm term) ?
                                    LocalizationUtil.GetLocalizedString(term, gameMechanicInfo.Tooltip) :
                                    gameMechanicInfo.Tooltip,
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
            foreach (Enumerators.GameMechanicDescription mechanicType in unit.Model.GameMechanicDescriptionsOnUnit)
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

        public void DrawTooltipInfoOfCard(BoardCardView boardCardView)
        {
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            if (boardCardView.Model.Card.Prototype.Kind == Enumerators.CardKind.ITEM)
                return;

            BuffOnCardInfoObjects = new List<BuffOnCardInfoObject>();

            const float offset = 0f;
            const float spacing = -6.75f;

            BuffOnCardInfoObject buff;

            List<BuffTooltipInfo> buffs = new List<BuffTooltipInfo>();

            // left block info ------------------------------------
            if (boardCardView.Model.Card.Prototype.Rank != Enumerators.CardRank.MINION && Constants.RankSystemEnabled)
            {
                TooltipContentData.RankInfo rankInfo = DataManager.GetCardRankInfo(boardCardView.Model.Card.Prototype.Rank);
                if (rankInfo != null)
                {
                    TooltipContentData.RankInfo.RankDescription rankDescription = rankInfo.Info.Find(
                        y => y.Element == boardCardView.Model.Card.Prototype.Faction);

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

            if (boardCardView.Model.Card.InstanceCard.CardType != Enumerators.CardType.WALKER)
            {
                TooltipContentData.CardTypeInfo cardTypeInfo = DataManager.GetCardTypeInfo(boardCardView.Model.Card.InstanceCard.CardType);
                if (cardTypeInfo != null)
                {
                    buffs.Add(
                        new BuffTooltipInfo
                        {
                            Title = cardTypeInfo.Name,
                            Description = Enum.TryParse($"GameData_BuffsToolTip_{LocalizationUtil.CapitalizedText(cardTypeInfo.Type.ToString())}", out LocalizationTerm term) ?
                                    LocalizationUtil.GetLocalizedString(term, cardTypeInfo.Tooltip) :
                                    cardTypeInfo.Tooltip,
                            TooltipObjectType = Enumerators.TooltipObjectType.UNIT_TYPE,
                            Value = -1
                        });
                }
            }

            if (boardCardView.Model.Card.InstanceCard.Abilities != null)
            {
                foreach (AbilityData abil in boardCardView.Model.Card.InstanceCard.Abilities)
                {
                    TooltipContentData.GameMechanicInfo gameMechanicInfo = DataManager.GetGameMechanicInfo(abil.GameMechanicDescription);
                    if (gameMechanicInfo != null)
                    {
                        buffs.Add(
                            new BuffTooltipInfo
                            {
                                Title = gameMechanicInfo.Name,
                                Description = Enum.TryParse($"GameData_BuffsToolTip_{LocalizationUtil.CapitalizedText(gameMechanicInfo.Type.ToString())}", out LocalizationTerm term) ?
                                    LocalizationUtil.GetLocalizedString(term, gameMechanicInfo.Tooltip) :
                                    gameMechanicInfo.Tooltip,
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
            if (IsPreview || _hasDestroyed || GameObject == null)
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
            UpdateCardsStatusEventHandler(Model.Card.Owner);
        }

        private void UpdateColorOfCost()
        {
            if (Model.CurrentCost > Model.Card.Prototype.Cost)
            {
                CostText.color = Color.red;
            }
            else if (Model.CurrentCost < Model.Card.Prototype.Cost)
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

            if (Model.CanBePlayed(player) && Model.CanBeBuyed(player))
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
            switch (ability.GameMechanicDescription)
            {
                case Enumerators.GameMechanicDescription.DelayedX:
                    return ability.Delay;
                default:
                    return ability.Value;
            }
        }

        private void PictureUpdatedEvent()
        {
            if(PictureSprite != null)
                PictureSprite.sprite = Model.CardPicture;
        }

        public override string ToString()
        {
            return $"([{GetType().Name}] {nameof(Model)}: {Model})";
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (GameObject == null)
            {
                MainApp.Instance.OnDrawGizmosCalled -= OnDrawGizmos;
                return;
            }

            if (Model.Card == null)
                return;

            DebugCardInfoDrawer.Draw(Transform.position, Model.Card.InstanceId.Id, Model.Card.Prototype.Name);
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

            private readonly TextMeshPro _triggerText;

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

                _triggerText = _selfObject.transform.Find("Text_CallType").GetComponent<TextMeshPro>();
                _descriptionText = _selfObject.transform.Find("Text_Description").GetComponent<TextMeshPro>();

                _buffIconPicture = _selfObject.transform.Find("Image_IconBackground/Image_BuffIcon")
                    .GetComponent<SpriteRenderer>();

                _triggerText.text = "    " + ReplaceXByValue(buffTooltipInfo.Title, buffTooltipInfo.Value).ToUpperInvariant();
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
    }
}
