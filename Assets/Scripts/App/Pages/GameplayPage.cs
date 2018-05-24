using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using GrandDevs.CZB.Data;
using CCGKit;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;

namespace GrandDevs.CZB
{
    public class GameplayPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
		private IPlayerManager _playerManager;
		private IDataManager _dataManager;

        private GameObject _selfPage,
                           _cardGraveyard,  
                           _playedCardPrefab;

        private Button _buttonBack;

        private List<CardInGraveyard> _cards;
        private PlayerSkillItem _playerSkill,
                                _opponentSkill;

        private List<CardZoneStatus> _deckStatus,
                             _graveyardStatus;

        private SpriteRenderer _playerDeckStatusTexture,
                               _opponentDeckStatusTexture,
                               _playerGraveyardStatusTexture,
                               _opponentGraveyardStatusTexture;

		private int _currentDeckId;

        public int CurrentDeckId
		{
			set { _currentDeckId = value; }
            get { return _currentDeckId; }
        }

        private bool _isPlayerInited = false;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/GameplayPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonBack = _selfPage.transform.Find("BackButtonFrame/BackButton").GetComponent<Button>();

            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);

            _cardGraveyard = _selfPage.transform.Find("CardGraveyard").gameObject;
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
        }

        private void OnPlayerDeckZoneChanged(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
                _playerDeckStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearest = _deckStatus.OrderBy(x => Math.Abs(x.percent - percent)).First();

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

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).ToList();

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

            if (index == 0)
                _opponentDeckStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearest = _deckStatus.OrderBy(x => Math.Abs(x.percent - percent)).First();

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

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).ToList();

                CardZoneStatus nearest = null;

                if (nearestObjects[0].percent > 0)
                    nearest = nearestObjects[0];
                else
                    nearest = nearestObjects[1];

                _opponentGraveyardStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private int GetPercentFromMaxDeck(int index)
        {
            return 100 * index / (int)Constants.DECK_MAX_SIZE;
        }

        //TODO: pass parameters here and apply corresponding texture, since previews have not the same textures as cards
        public void AddCardToGraveyard(CCGKit.RuntimeCard card)
        {
            //Debug.Log("AddCardToGraveyard for player: "+card.ownerPlayer.id);

            BoardCreature cardToDestroy = _playerManager.PlayerGraveyardCards.Find(x => x.card == card);
            if (cardToDestroy == null)
                cardToDestroy = _playerManager.OpponentGraveyardCards.Find(x => x.card == card);

            if (cardToDestroy != null)
            {
                _cards.Add(new CardInGraveyard(GameObject.Instantiate(_playedCardPrefab, _cardGraveyard.transform),
                                               cardToDestroy.transform.Find("GraphicsAnimation/PictureRoot/CreaturePicture").GetComponent<SpriteRenderer>().sprite));

                GameClient.Get<ITimerManager>().AddTimer((x) =>
                {
                    cardToDestroy.transform.DOShakePosition(.7f, 0.25f, 10, 90, false, false); // CHECK SHAKE!!
                }, null, 1f);

                GameClient.Get<ITimerManager>().AddTimer((x) =>
                {
					cardToDestroy.transform.DOKill();
					GameObject.Destroy(cardToDestroy.gameObject);

                }, null, 2f);
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
            var player = NetworkingUtils.GetHumanLocalPlayer();

            player.deckZone.onZoneChanged += OnPlayerDeckZoneChanged;
            player.opponentDeckZone.onZoneChanged += OnOpponentDeckZoneChanged;

            GameUI gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();

            int heroId = GameClient.Get<IGameplayManager>().PlayerHeroId = _dataManager.CachedDecksData.decks[_currentDeckId].heroId;
            int opponentHeroId = GameClient.Get<IGameplayManager>().OpponentHeroId = UnityEngine.Random.Range(0, _dataManager.CachedHeroesData.heroes.Count);

            var _skillsIcons = new Dictionary<Enumerators.SkillType, string>();
            _skillsIcons.Add(Enumerators.SkillType.FIRE_DAMAGE, "Images/hero_power_01");
            _skillsIcons.Add(Enumerators.SkillType.HEAL, "Images/hero_power_02");
            _skillsIcons.Add(Enumerators.SkillType.CARD_RETURN, "Images/hero_power_03");
            _skillsIcons.Add(Enumerators.SkillType.FREEZE, "Images/hero_power_04");
            _skillsIcons.Add(Enumerators.SkillType.TOXIC_DAMAGE, "Images/hero_power_05");
            _skillsIcons.Add(Enumerators.SkillType.HEAL_ANY, "Images/hero_power_06");

            Hero currentPlayerHero = _dataManager.CachedHeroesData.heroes[heroId];
            Hero currentOpponentHero = _dataManager.CachedHeroesData.heroes[opponentHeroId];

            if (currentPlayerHero != null)
            {
                gameUI.SetPlayerName(currentPlayerHero.name);
				_playerSkill = new PlayerSkillItem(GameObject.Find("Player/Spell"), currentPlayerHero.skill, _skillsIcons[currentPlayerHero.skill.skillType]);

                var heroTexture = _loadObjectsManager.GetObjectByPath<Texture2D>("Images/Heroes/CZB_2D_Hero_Portrait_" + currentPlayerHero.element.ToString() + "_EXP");

                var transfHeroObject = GameObject.Find("Player/Avatar/Hero_Object").transform;

                for (int i = 0; i < transfHeroObject.childCount; i++)
                    transfHeroObject.GetChild(i).GetComponent<Renderer>().material.mainTexture = heroTexture;

                var heroHighlight = _loadObjectsManager.GetObjectByPath<Sprite>
                    ("Images/Heroes/CZB_2D_Hero_Decor_" + currentPlayerHero.element.ToString() + "_EXP");
                GameObject.Find("Player/Avatar/HeroHighlight").GetComponent<SpriteRenderer>().sprite = heroHighlight;
            }
            if (currentOpponentHero != null)
            {
                gameUI.SetOpponentName(currentOpponentHero.name);
                _opponentSkill = new PlayerSkillItem(GameObject.Find("Opponent/Spell"), currentOpponentHero.skill, _skillsIcons[currentOpponentHero.skill.skillType]);

                var heroTexture = _loadObjectsManager.GetObjectByPath<Texture2D>("Images/Heroes/CZB_2D_Hero_Portrait_" + currentOpponentHero.element.ToString() + "_EXP");

                var transfHeroObject = GameObject.Find("Opponent/Avatar/Hero_Object").transform;

                for (int i = 0; i < transfHeroObject.childCount; i++)
                    transfHeroObject.GetChild(i).GetComponent<Renderer>().material.mainTexture = heroTexture;

               var heroHighlight = _loadObjectsManager.GetObjectByPath<Sprite>
                  ("Images/Heroes/CZB_2D_Hero_Decor_" + currentOpponentHero.element.ToString() + "_EXP");

                GameObject.Find("Opponent/Avatar/HeroHighlight").GetComponent<SpriteRenderer>().sprite = heroHighlight;
            }


            _playerDeckStatusTexture = GameObject.Find("Player/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _opponentDeckStatusTexture = GameObject.Find("Opponent/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _playerGraveyardStatusTexture = GameObject.Find("Player/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();
            _opponentGraveyardStatusTexture = GameObject.Find("Opponent/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();

            _isPlayerInited = true;
        }

        public void Update()
        {
            if (!_selfPage.activeSelf)
                return;

            //Debug.Log("Player id: " + _playerManager.playerInfo.id);
            //Debug.Log("Opponent id: " + _playerManager.opponentInfo.id);
        }

        public void Show()
        {
            _selfPage.SetActive(true);
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
                if (NetworkingUtils.GetLocalPlayer().isServer)
                {
                    GameNetworkManager.Instance.StopHost();
                }
                else
                {
                    GameNetworkManager.Instance.StopClient();
                }

                if (GameClient.Get<ITutorialManager>().IsTutorial)
                {
                    GameClient.Get<ITutorialManager>().CancelTutorial();
                }

                _uiManager.HidePopup<YourTurnPopup>();
                GameClient.Get<IAppStateManager>().ChangeAppState(GrandDevs.CZB.Common.Enumerators.AppState.MAIN_MENU);
            };
            _uiManager.DrawPopup<ConfirmationPopup>(callback);
        }
        
        #endregion
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

        public CardZoneStatus( Enumerators.CardZoneType cardZone, Sprite statusSprite, int percent)
        {
            this.cardZone = cardZone;
            this.statusSprite = statusSprite;
            this.percent = percent;
        }
    }
}