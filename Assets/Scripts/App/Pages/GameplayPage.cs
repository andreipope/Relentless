using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using GrandDevs.CZB.Data;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace GrandDevs.CZB
{
    public class GameplayPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IPlayerManager _playerManager;
        private IDataManager _dataManager;
        private IGameplayManager _gameplayManager;

        private BattlegroundController _battlegroundController;

        private GameObject _selfPage,
                           _playedCardPrefab;
        private VerticalLayoutGroup _cardGraveyard;

        private Button _buttonBack;

        private List<CardInGraveyard> _cards;
        private PlayerSkillItem _playerSkill,
                                _opponentSkill;

        private PlayerManaBarItem _playerManaBar,
                                  _opponentManaBar;

        private List<CardZoneStatus> _deckStatus,
                             _graveyardStatus;

        private TextMeshPro _playerHealthText,
                            _opponentHealthText,
                            _playerCardDeckCountText,
                            _opponentCardDeckCountText;

        private SpriteRenderer _playerDeckStatusTexture,
                               _opponentDeckStatusTexture,
                               _playerGraveyardStatusTexture,
                               _opponentGraveyardStatusTexture;

        private GameObject _zippingVFX;

        private int _graveYardTopOffset;

        private int _currentDeckId;

        public int CurrentDeckId
        {
            set { _currentDeckId = value; }
            get { return _currentDeckId; }
        }

        private bool _isPlayerInited = false;
        private int topOffset;


        private GameObject _endTurnButton;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/GameplayPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonBack = _selfPage.transform.Find("BackButtonFrame/BackButton").GetComponent<Button>();

            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);


            _cardGraveyard = _selfPage.transform.Find("CardGraveyard").GetComponent<VerticalLayoutGroup>();
            _playedCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");
            _cards = new List<CardInGraveyard>();

            _playerManager.OnBoardCardKilled += AddCardToGraveyard;
            _playerManager.OnLocalPlayerSetUp += SetUpPlayer;

            Hide();

            _deckStatus = new List<CardZoneStatus>();
            _deckStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.DECK, null, 0));
            _deckStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_single"), 15));
            _deckStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_couple"), 40));
            _deckStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_bunch"), 60));
            _deckStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_full"), 80));

            _graveyardStatus = new List<CardZoneStatus>();
            _graveyardStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.GRAVEYARD, null, 0));
            _graveyardStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_single"), 10));
            _graveyardStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_couple"), 40));
            _graveyardStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_bunch"), 75));
            _graveyardStatus.Add(new CardZoneStatus(Enumerators.CardZoneType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_full"), 100));
            //scene.OpenPopup<PopupTurnStart>("PopupTurnStart", null, false);

            _playerManager.OnPlayerGraveyardUpdatedEvent += OnPlayerGraveyardZoneChanged;
            _playerManager.OnOpponentGraveyardUpdatedEvent += OnOpponentGraveyardZoneChanged;

            _graveYardTopOffset = 0;
        }

        public void SetEndTurnButtonStatus(bool status)
        {
            _endTurnButton.GetComponent<EndTurnButton>().SetEnabled(status);
           // _endTurnButton.SetActive(status);
        }

        private void OnPlayerDeckZoneChanged(int index)
        {
            if (!_isPlayerInited)
                return;

            _playerCardDeckCountText.text = index.ToString();

            if (index == 0)
                _playerDeckStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearest = _deckStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).First();

                _playerDeckStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnPlayerGraveyardZoneChanged(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
                _playerGraveyardStatusTexture.sprite = _graveyardStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).ToList();

                CardZoneStatus nearest = null;

                if (nearestObjects[0].percent > 0)
                    nearest = nearestObjects[0];
                else
                    nearest = nearestObjects[1];

                _playerGraveyardStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnOpponentDeckZoneChanged(int index)
        {
            if (!_isPlayerInited)
                return;

            _opponentCardDeckCountText.text = index.ToString();

            if (index == 0)
                _opponentDeckStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearest = _deckStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).First();

                _opponentDeckStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnOpponentGraveyardZoneChanged(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
                _opponentGraveyardStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).ToList();

                CardZoneStatus nearest = null;

                if (nearestObjects[0].percent > 0)
                    nearest = nearestObjects[0];
                else
                    nearest = nearestObjects[1];

                _opponentGraveyardStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnPlayerHPChanged(int oldHealth, int health)
        {
            if (!_isPlayerInited)
                return;
            _playerHealthText.text = health.ToString();
        }

        private void OnPlayerManaChanged(int oldMana, int mana)
        {
            if (!_isPlayerInited)
                return;
            _playerManaBar.SetMana(mana);
        }

        private void OnOpponentHPChanged(int oldHealth, int health)
        {
            if (!_isPlayerInited)
                return;
            _opponentHealthText.text = health.ToString();
        }

        private void OnOpponentManaChanged(int oldMana, int mana)
        {
            if (!_isPlayerInited)
                return;
            _opponentManaBar.SetMana(mana);
        }

        private int GetPercentFromMaxDeck(int index)
        {
            return 100 * index / (int)Constants.DECK_MAX_SIZE;
        }

        //TODO: pass parameters here and apply corresponding texture, since previews have not the same textures as cards
        public void AddCardToGraveyard(WorkingCard card)
        {
            bool isOpponentCard = false;

            var localPlayer = _gameplayManager.GetLocalPlayer();
            var opponentPlayer = _gameplayManager.GetOpponentPlayer();

            //Debug.Log("AddCardToGraveyard for player: "+card.ownerPlayer.id);

            BoardCreature cardToDestroy = _playerManager.PlayerGraveyardCards.Find(x => x.Card == card);

            if (cardToDestroy == null)
            {
                cardToDestroy = _playerManager.OpponentGraveyardCards.Find(x => x.Card == card);

                if (cardToDestroy == null)
                {
                    // optimize it!! fix for summonned zombie
                    cardToDestroy = localPlayer.BoardCards.Find(x => x.Card == card && card.health <= 0);

                    if (cardToDestroy != null)
                    {
                        localPlayer.BoardCards.Remove(cardToDestroy);
                    }
                }

                isOpponentCard = true;
            }

            if (cardToDestroy != null && cardToDestroy.gameObject)
            {
                _cards.Add(new CardInGraveyard(GameObject.Instantiate(_playedCardPrefab, _cardGraveyard.transform),
                                               cardToDestroy.transform.Find("GraphicsAnimation/PictureRoot/CreaturePicture").GetComponent<SpriteRenderer>().sprite));
                if (_cards.Count > 4)
                {
                    _graveYardTopOffset = -66 - 120 * (_cards.Count - 5);
                }
                //for (int j = _cards.Count-1; j >= 0; j--)
                //_cards[j].selfObject.transform.SetAsLastSibling();

                cardToDestroy.transform.position = new Vector3(cardToDestroy.transform.position.x, cardToDestroy.transform.position.y, cardToDestroy.transform.position.z + 0.2f);

                GameClient.Get<ITimerManager>().AddTimer((x) =>
                {
                    if (cardToDestroy != null && cardToDestroy.gameObject)
                        cardToDestroy.transform.DOShakePosition(.7f, 0.25f, 10, 90, false, false); // CHECK SHAKE!!


                    var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);
                    string cardDeathSoundName = libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_DEATH;
                    Debug.Log(cardDeathSoundName);

                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, cardDeathSoundName, Constants.ZOMBIES_SOUND_VOLUME, Enumerators.CardSoundType.DEATH);

                    float soundLength = GameClient.Get<ISoundManager>().GetSoundLength(Enumerators.SoundType.CARDS, cardDeathSoundName);

                    GameClient.Get<ITimerManager>().AddTimer((t) =>
                    {
                        if (isOpponentCard)
                        {
                            opponentPlayer.BoardCards.Remove(cardToDestroy);
                        }
                        else
                        {
                            localPlayer.BoardCards.Remove(cardToDestroy);
                        }

                        if (cardToDestroy != null && cardToDestroy.gameObject)
                        {
                            cardToDestroy.transform.DOKill();
                            GameObject.Destroy(cardToDestroy.gameObject);
                        }

                        GameClient.Get<ITimerManager>().AddTimer((f) =>
                        {
                            _battlegroundController.RearrangeTopBoard();
                            _battlegroundController.RearrangeBottomBoard();
                        }, null, Time.deltaTime, false);

                    }, null, soundLength);

                }, null, 1f);


                //  if (!gameEnded)
                // {

                //   GameClient.Get<ITimerManager>().AddTimer((x) =>
                //  {
                //  }, null, Constants.DELAY_TO_PLAY_DEATH_SOUND_OF_CREATURE);
                //}


                //GameClient.Get<ITimerManager>().AddTimer(DelayedCardDestroy, new object[] { cardToDestroy }, 0.7f);
            }
        }

        private void DelayedCardDestroy(object[] card)
        {
            BoardCreature cardToDestroy = (BoardCreature)card[0];
            if (cardToDestroy != null)
            {
                cardToDestroy.transform.DOKill();
                GameObject.Destroy(cardToDestroy.gameObject);
            }
        }

        public void ClearGraveyard()
        {
            foreach (var item in _cards)
            {
                item.Dispose();
            }
            _cards.Clear();
        }

        private void SetUpPlayer()
        {
            var player = _gameplayManager.GetLocalPlayer();
            var opponent = _gameplayManager.GetOpponentPlayer();

            player.DeckChangedEvent += OnPlayerDeckZoneChanged;
            player.PlayerHPChangedEvent += OnPlayerHPChanged;
            player.PlayerManaChangedEvent += OnPlayerManaChanged;
            opponent.DeckChangedEvent += OnOpponentDeckZoneChanged;
            opponent.PlayerHPChangedEvent += OnOpponentHPChanged;
            opponent.PlayerManaChangedEvent += OnOpponentManaChanged;

            player.OnStartTurnEvent += OnStartTurnEventHandler;
        }

        public void Update()
        {
            if (!_selfPage.activeSelf)
                return;

            if (_cardGraveyard.padding.top > _graveYardTopOffset)
            {
                float offset = Mathf.Lerp((float)_cardGraveyard.padding.top, (float)_graveYardTopOffset, Time.deltaTime * 2);
                _cardGraveyard.padding = new RectOffset(0, 0, Mathf.FloorToInt(offset), 0);
            }
            //Debug.Log("Player id: " + _playerManager.playerInfo.id);
            //Debug.Log("Opponent id: " + _playerManager.opponentInfo.id);
        }

        public void Show()
        {
            if (_zippingVFX == null)
            {
                _zippingVFX = GameObject.Find("Background/Zapping").gameObject;
                _zippingVFX.SetActive(false);
            }
            _selfPage.SetActive(true);
            StartGame();
        }

        public void StartGame()
        {
            if(_battlegroundController == null)
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            int deckId = GameClient.Get<IGameplayManager>().PlayerDeckId = _currentDeckId;
            int opponentdeckId = GameClient.Get<IGameplayManager>().OpponentDeckId = UnityEngine.Random.Range(0, GameClient.Get<IDataManager>().CachedOpponentDecksData.decks.Count);

            int heroId = _dataManager.CachedDecksData.decks[_currentDeckId].heroId;
            int hopponentId = _dataManager.CachedOpponentDecksData.decks[opponentdeckId].heroId;

            Hero currentPlayerHero = _dataManager.CachedHeroesData.Heroes[heroId];
            Hero currentOpponentHero = _dataManager.CachedHeroesData.Heroes[hopponentId];

            if (currentPlayerHero != null)
            {
                SetHeroInfo(currentPlayerHero, "Player");
            }
            if (currentOpponentHero != null)
            {
                SetHeroInfo(currentOpponentHero, "Opponent");
            }

            _playerDeckStatusTexture = GameObject.Find("Player/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _opponentDeckStatusTexture = GameObject.Find("Opponent/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _playerGraveyardStatusTexture = GameObject.Find("Player/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();
            _opponentGraveyardStatusTexture = GameObject.Find("Opponent/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();

            _playerHealthText = GameObject.Find("Player/Avatar/LivesCircle/DefenceText").GetComponent<TextMeshPro>();
            _opponentHealthText = GameObject.Find("Opponent/Avatar/LivesCircle/DefenceText").GetComponent<TextMeshPro>();

            _playerManaBar = new PlayerManaBarItem(GameObject.Find("PlayerManaBar"));
            _opponentManaBar = new PlayerManaBarItem(GameObject.Find("OpponentManaBar"));

            _endTurnButton = GameObject.Find("EndTurnButton");


            _isPlayerInited = true;
        }
        public void SetHeroInfo(Hero hero, string objectName)
        {
            GameObject.Find("GameUI").GetComponent<GameUI>().SetPlayerName(hero.name);

            new PlayerSkillItem(GameObject.Find(objectName + "/Spell"), hero.skill, "Images/HeroesIcons/hero_icon_" + hero.heroElement.ToString());

            var heroTexture = _loadObjectsManager.GetObjectByPath<Texture2D>("Images/Heroes/CZB_2D_Hero_Portrait_" + hero.heroElement.ToString() + "_EXP");
            var transfHeroObject = GameObject.Find(objectName + "/Avatar/Hero_Object").transform;

            Material heroAvatarMaterial = new Material(Shader.Find("Sprites/Default"));
            heroAvatarMaterial.mainTexture = heroTexture;

            for (int i = 0; i < transfHeroObject.childCount; i++)
                transfHeroObject.GetChild(i).GetComponent<Renderer>().material = heroAvatarMaterial;

            var heroHighlight = _loadObjectsManager.GetObjectByPath<Sprite>
                ("Images/Heroes/CZB_2D_Hero_Decor_" + hero.heroElement.ToString() + "_EXP");
            GameObject.Find(objectName + "/Avatar/HeroHighlight").GetComponent<SpriteRenderer>().sprite = heroHighlight;
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            ClearGraveyard();

            _isPlayerInited = false;
        }

        public void Dispose()
        {

        }

        #region Buttons Handlers
        public void BackButtonOnClickHandler()
        {
            Action callback = () =>
            {
                _uiManager.HidePopup<YourTurnPopup>();

                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU, true);

                GameClient.Get<ISoundManager>().CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            };

            _uiManager.DrawPopup<ConfirmationPopup>(callback);
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        #endregion

        private void OnStartTurnEventHandler()
        {
            _zippingVFX.SetActive(_gameplayManager.GetController<PlayerController>().IsActive);
        }
    }

    public class PlayerSkillItem
    {
        public GameObject selfObject;
        public SpriteRenderer icon;
        public TextMeshPro costText;
        //public HeroSkill skill;

        private ILoadObjectsManager _loader;

        public PlayerSkillItem(GameObject gameObject, HeroSkill skill, string iconPath)
        {
            _loader = GameClient.Get<ILoadObjectsManager>();
            selfObject = gameObject;
            // this.skill = skill;
            icon = selfObject.transform.Find("Icon").GetComponent<SpriteRenderer>();
            costText = selfObject.transform.Find("SpellCost/SpellCostText").GetComponent<TextMeshPro>();

            Sprite sp = _loader.GetObjectByPath<Sprite>(iconPath);
            if (sp != null)
                icon.sprite = sp;
        }
    }

    public class CardInGraveyard
    {
        public GameObject selfObject;
        public Image image;

        public CardInGraveyard(GameObject gameObject, Sprite sprite = null)
        {
            selfObject = gameObject;
            image = selfObject.transform.Find("Image").GetComponent<Image>();

            if (sprite != null)
                image.sprite = sprite;
        }

        public void Dispose()
        {
            if (selfObject != null)
                GameObject.Destroy(selfObject);
        }
    }

    public class CardZoneStatus
    {
        public Enumerators.CardZoneType cardZone;
        public int percent;
        public Sprite statusSprite;

        public CardZoneStatus(Enumerators.CardZoneType cardZone, Sprite statusSprite, int percent)
        {
            this.cardZone = cardZone;
            this.statusSprite = statusSprite;
            this.percent = percent;
        }
    }

    public class PlayerManaBarItem
    {
        private GameObject selfObject;
        private TextMeshPro _manaText;
        private List<ManaBottleItem> _manaBottles;

        public PlayerManaBarItem() { }

        public PlayerManaBarItem(GameObject gameObject)
        {
            selfObject = gameObject;
            _manaText = selfObject.transform.Find("ManaAmount/Text").GetComponent<TextMeshPro>();
            _manaBottles = new List<ManaBottleItem>();
            GameObject bottle = null;
            for (int i = 0; i < selfObject.transform.childCount; i++)
            {
                bottle = selfObject.transform.GetChild(i).gameObject;
                if (bottle.name.Contains("ManaIcon"))
                    _manaBottles.Add(new ManaBottleItem(bottle));
            }
        }

        public void SetMana(int mana)
        {
            _manaText.text = mana.ToString();
            for (var i = 0; i < _manaBottles.Count; i++)
            {
                if (i < mana)
                {
                    _manaBottles[i].Active();
                }
                else
                {
                    _manaBottles[i].Disactive();
                }
            }
        }
    }

    public class ManaBottleItem
    {
        public GameObject selfObject;

        private SpriteRenderer _fullBoottle,
                               _glowBottle; 

        public ManaBottleItem() { }

        public ManaBottleItem(GameObject gameObject)
        {
            selfObject = gameObject;
            _fullBoottle = selfObject.transform.Find("ManaIconBlue/goobottle_goo").GetComponent<SpriteRenderer>();
            _glowBottle = selfObject.transform.Find("ManaIconBlue/glow_goo").GetComponent<SpriteRenderer>();
        }

        public void Active()
        {
            _fullBoottle.DOFade(1.0f, 0.5f);
            _glowBottle.DOFade(1.0f, 0.5f);
        }

        public void Disactive()
        {
            _fullBoottle.DOFade(0.0f, 0.5f);
            _glowBottle.DOFade(0.0f, 0.5f);
        }
    }
}