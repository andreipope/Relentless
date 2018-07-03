// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;

namespace LoomNetwork.CZB
{
    public class GameplayPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IPlayerManager _playerManager;
        private IDataManager _dataManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private ITimerManager _timerManager;


        private BattlegroundController _battlegroundController;

        private GameObject _selfPage,
                           _playedCardPrefab;

        private Button _buttonBack;

        private List<CardInGraveyard> _cards;
        private PlayerSkillItem _playerSkill,
                                _opponentSkill;

        private PlayerManaBarItem _playerManaBar,
                                  _opponentManaBar;

        private List<CardZoneOnBoardStatus> _deckStatus,
                             _graveyardStatus;

        private TextMeshPro _playerHealthText,
                            _opponentHealthText,
                            _playerCardDeckCountText,
                            _opponentCardDeckCountText,
                            _playerNameText,
                            _opponentNameText;

        private SpriteRenderer _playerDeckStatusTexture,
                               _opponentDeckStatusTexture,
                               _playerGraveyardStatusTexture,
                               _opponentGraveyardStatusTexture;

        private GameObject _zippingVFX;

        private int _graveYardTopOffset;

        private int _currentDeckId;

        private bool _isPlayerInited = false;
        private int topOffset;

        private ReportPanelItem _reportGameActionsPanel;

        private GameObject _endTurnButton;

        public int CurrentDeckId
        {
            set { _currentDeckId = value; }
            get { return _currentDeckId; }
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _timerManager = GameClient.Get<ITimerManager>();

      
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/GameplayPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonBack = _selfPage.transform.Find("BackButtonFrame/BackButton").GetComponent<Button>();

            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);

            _playedCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");
            _cards = new List<CardInGraveyard>();

            _gameplayManager.OnGameInitializedEvent += OnGameInitializedEventHandler;
            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;


            _deckStatus = new List<CardZoneOnBoardStatus>();
            _deckStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.DECK, null, 0));
            _deckStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_single"), 15));
            _deckStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_couple"), 40));
            _deckStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_bunch"), 60));
            _deckStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.DECK, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_full"), 80));

            _graveyardStatus = new List<CardZoneOnBoardStatus>();
            _graveyardStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.GRAVEYARD, null, 0));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_single"), 10));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_couple"), 40));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_bunch"), 75));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType.GRAVEYARD, _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_full"), 100));


            _graveYardTopOffset = 0;

            _reportGameActionsPanel = new ReportPanelItem(_selfPage.transform.Find("ActionReportPanel").gameObject);

            Hide();
        }

        private void OnGameEndedEventHandler()
        {
            ClearGraveyard();

            if (_reportGameActionsPanel != null)
                _reportGameActionsPanel.Clear();
        }

        public void Hide()
        {
            _selfPage.SetActive(false);

            _isPlayerInited = false;
        }

        public void Dispose()
        {

        }

        public void Update()
        {
            if (!_selfPage.activeSelf)
                return;
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



        public void SetEndTurnButtonStatus(bool status)
        {
            _endTurnButton.GetComponent<EndTurnButton>().SetEnabled(status);
            // _endTurnButton.SetActive(status);
        }

        public void ClearGraveyard()
        {
            foreach (var item in _cards)
            {
                item.Dispose();
            }
            _cards.Clear();
        }

        //TODO: pass parameters here and apply corresponding texture, since previews have not the same textures as cards
        public void OnBoardCardKilledEventHandler(BoardUnit cardToDestroy)
        {
            if (cardToDestroy == null)
                return;

            bool isOpponentCard = cardToDestroy.ownerPlayer == _gameplayManager.GetLocalPlayer() ? false : true;


            //_cards.Add(new CardInGraveyard(GameObject.Instantiate(_playedCardPrefab, _cardGraveyard.transform),
            //                               cardToDestroy.transform.Find("GraphicsAnimation/PictureRoot/CreaturePicture").GetComponent<SpriteRenderer>().sprite));
            //if (_cards.Count > 4)
            //{
            //    _graveYardTopOffset = -66 - 120 * (_cards.Count - 5);
            //}
            //for (int j = _cards.Count-1; j >= 0; j--)
            //_cards[j].selfObject.transform.SetAsLastSibling();

            cardToDestroy.transform.position = new Vector3(cardToDestroy.transform.position.x, cardToDestroy.transform.position.y, cardToDestroy.transform.position.z + 0.2f);

            _timerManager.AddTimer((x) =>
            {
                cardToDestroy.transform.DOShakePosition(.7f, 0.25f, 10, 90, false, false); // CHECK SHAKE!!

                string cardDeathSoundName = cardToDestroy.Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_DEATH;

                _soundManager.PlaySound(Enumerators.SoundType.CARDS, cardDeathSoundName, Constants.ZOMBIES_SOUND_VOLUME, Enumerators.CardSoundType.DEATH);

                float soundLength = _soundManager.GetSoundLength(Enumerators.SoundType.CARDS, cardDeathSoundName);

                _timerManager.AddTimer((t) =>
                {
                    cardToDestroy.ownerPlayer.BoardCards.Remove(cardToDestroy);
                    cardToDestroy.ownerPlayer.RemoveCardFromBoard(cardToDestroy.Card);
                    cardToDestroy.ownerPlayer.AddCardToGraveyard(cardToDestroy.Card);

                    cardToDestroy.transform.DOKill();
                    MonoBehaviour.Destroy(cardToDestroy.gameObject);

                    _timerManager.AddTimer((f) =>
                    {
                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
                        _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer();
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

        private void DelayedCardDestroy(object[] card)
        {
            BoardUnit cardToDestroy = (BoardUnit)card[0];
            if (cardToDestroy != null)
            {
                cardToDestroy.transform.DOKill();
                GameObject.Destroy(cardToDestroy.gameObject);
            }
        }

        public void StartGame()
        {
            if (_battlegroundController == null)
            {
                _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

                _battlegroundController.OnBoardCardKilledEvent += OnBoardCardKilledEventHandler;
                _battlegroundController.OnPlayerGraveyardUpdatedEvent += OnPlayerGraveyardUpdatedEventHandler;
                _battlegroundController.OnOpponentGraveyardUpdatedEvent += OnOpponentGraveyardUpdatedEventHandler;
            }

            int deckId = _gameplayManager.PlayerDeckId = _currentDeckId;
            int opponentdeckId = _gameplayManager.OpponentDeckId = UnityEngine.Random.Range(0, _dataManager.CachedOpponentDecksData.decks.Count);

            int heroId = _dataManager.CachedDecksData.decks[_currentDeckId].heroId;
            int hopponentId = _dataManager.CachedOpponentDecksData.decks[opponentdeckId].heroId;

            Hero currentPlayerHero = _dataManager.CachedHeroesData.Heroes[heroId];
            Hero currentOpponentHero = _dataManager.CachedHeroesData.Heroes[hopponentId];


            _playerDeckStatusTexture = GameObject.Find("Player/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _opponentDeckStatusTexture = GameObject.Find("Opponent/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _playerGraveyardStatusTexture = GameObject.Find("Player/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();
            _opponentGraveyardStatusTexture = GameObject.Find("Opponent/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();

            _playerHealthText = GameObject.Find("Player/Avatar/LivesCircle/DefenceText").GetComponent<TextMeshPro>();
            _opponentHealthText = GameObject.Find("Opponent/Avatar/LivesCircle/DefenceText").GetComponent<TextMeshPro>();

            _playerManaBar = new PlayerManaBarItem(GameObject.Find("PlayerManaBar"));
            _opponentManaBar = new PlayerManaBarItem(GameObject.Find("OpponentManaBar"));


            // improve find to get it from OBJECTS ON BOARD!!
            _playerNameText = GameObject.Find("Player/NameBoard/NameText").GetComponent<TextMeshPro>();
            _opponentNameText = GameObject.Find("Opponent/NameBoard/NameText").GetComponent<TextMeshPro>();


            _playerCardDeckCountText = GameObject.Find("Player/CardDeckText").GetComponent<TextMeshPro>();
            _opponentCardDeckCountText = GameObject.Find("Opponent/CardDeckText").GetComponent<TextMeshPro>();

            _endTurnButton = GameObject.Find("EndTurnButton");

            if (currentPlayerHero != null)
            {
                SetHeroInfo(currentPlayerHero, "Player");
                _playerNameText.name = currentPlayerHero.name;
            }
            if (currentOpponentHero != null)
            {
                SetHeroInfo(currentOpponentHero, "Opponent");
                _opponentNameText.name = currentOpponentHero.name;
            }

            _isPlayerInited = true;
        }

        public void SetHeroInfo(Hero hero, string objectName)
        {
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


        public void SetPlayerDeckCards(int cards)
        {
            _playerCardDeckCountText.text = cards.ToString();
            if (cards == 0 && _playerDeckStatusTexture.gameObject.activeInHierarchy)
                _playerDeckStatusTexture.gameObject.SetActive(false);
        }

        public void SetOpponentDeckCards(int cards)
        {
            _opponentCardDeckCountText.text = cards.ToString();
            if (cards == 0 && _opponentDeckStatusTexture.gameObject.activeInHierarchy)
                _opponentDeckStatusTexture.gameObject.SetActive(false);
        }


        private int GetPercentFromMaxDeck(int index)
        {
            return 100 * index / (int)Constants.DECK_MAX_SIZE;
        }

        #region event handlers

        private void OnGameInitializedEventHandler()
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

            OnPlayerDeckZoneChanged(player.CardsInDeck.Count);
            OnPlayerHPChanged(player.HP, player.HP);
            OnPlayerManaChanged(player.Mana, player.Mana);
            OnOpponentDeckZoneChanged(opponent.CardsInDeck.Count);
            OnOpponentHPChanged(opponent.HP, opponent.HP);
            OnOpponentManaChanged(opponent.Mana, opponent.Mana);
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

        private void OnPlayerGraveyardUpdatedEventHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
                _playerGraveyardStatusTexture.sprite = _graveyardStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).ToList();

                CardZoneOnBoardStatus nearest = null;

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

        private void OnOpponentGraveyardUpdatedEventHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
                _opponentGraveyardStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).ToList();

                CardZoneOnBoardStatus nearest = null;

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

            if (health > 9)
                _playerHealthText.color = Color.white;
            else
                _playerHealthText.color = Color.red;
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

            if (health > 9)
                _opponentHealthText.color = Color.white;
            else
                _opponentHealthText.color = Color.red;
        }

        private void OnOpponentManaChanged(int oldMana, int mana)
        {
            if (!_isPlayerInited)
                return;
            _opponentManaBar.SetMana(mana);
        }

        private void OnStartTurnEventHandler()
        {
            _zippingVFX.SetActive(_gameplayManager.GetController<PlayerController>().IsActive);
        }


        #endregion


        #region Buttons Handlers
        public void BackButtonOnClickHandler()
        {
            Action callback = () =>
            {
                _uiManager.HidePopup<YourTurnPopup>();

                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU, true);

                _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            };

            _uiManager.DrawPopup<ConfirmationPopup>(callback);
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        #endregion
    }

}