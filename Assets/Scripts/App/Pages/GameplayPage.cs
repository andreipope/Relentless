using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace GrandDevs.CZB
{
    public class GameplayPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IPlayerManager _playerManager;

        private GameObject _selfPage,
                           _cardGraveyard,  
                           _playedCardPrefab;

        private MenuButtonNoGlow _buttonBack;

        private List<CardInGraveyard> _cards;
        private PlayerSkillItem _playerSkill,
                                _opponentSkill;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/GameplayPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            
            _buttonBack = _selfPage.transform.Find("BackButton").GetComponent<MenuButtonNoGlow>();
            _buttonBack.onClickEvent.AddListener(OnBackButtonClick);

            _cardGraveyard = _selfPage.transform.Find("CardGraveyard").gameObject;
            _playedCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");
            _cards = new List<CardInGraveyard>();

            _playerManager.OnBoardCardKilled += AddCardToGraveyard;
            _playerManager.OnLocalPlayerSetUp += SetUpPlayer;

            Hide();
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
                                               cardToDestroy.transform.Find("PictureMask/Picture").GetComponent<SpriteRenderer>().sprite));
                GameObject.Destroy(cardToDestroy.gameObject);
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
            GameUI gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
            //int playerHeroId = GameManager.Instance.playerDecks[GameManager.Instance.currentDeckId].heroeId;
            //TODO:↓
            //int playerHeroId = 0, opponentHeroId = 1;
            Hero currentPlayerHero = GameManager.Instance.heroes[GameManager.Instance.playerDecks[GameManager.Instance.currentDeckId].heroeId];
            Hero currentOpponentHero = GameManager.Instance.heroes[GameManager.Instance.opponentHeroId];

            if (currentPlayerHero != null)
            {
                gameUI.SetPlayerName(currentPlayerHero.name);
				_playerSkill = new PlayerSkillItem(GameObject.Find("Player/Spell"), currentPlayerHero.skill);
                GameObject.Find("Player/Avatar/Icon").GetComponent<SpriteRenderer>().sprite = 
                    GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>("Images/Avatar_" + currentPlayerHero.element.ToString());
            }
            if (currentOpponentHero != null)
            {
                gameUI.SetOpponentName(currentOpponentHero.name);
                _opponentSkill = new PlayerSkillItem(GameObject.Find("Opponent/Spell"), currentOpponentHero.skill);
				GameObject.Find("Opponent/Avatar/Icon").GetComponent<SpriteRenderer>().sprite =
					GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>("Images/Avatar_" + currentOpponentHero.element.ToString());
            }
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
        }

        public void Dispose()
        {

        }

        #region Buttons Handlers
        public void OnBackButtonClick()
        {
            if (NetworkingUtils.GetLocalPlayer().isServer)
            {
                GameNetworkManager.Instance.StopHost();
            }
            else
            {
                GameNetworkManager.Instance.StopClient();
            }

            GameClient.Get<IAppStateManager>().ChangeAppState(GrandDevs.CZB.Common.Enumerators.AppState.MAIN_MENU);
        }
        
        #endregion
    }

    public class PlayerSkillItem
    {
        public GameObject selfObject;
        public SpriteRenderer icon;
        public TextMeshPro costText;
        public HeroSkill skill;

        private ILoadObjectsManager _loader;

        public PlayerSkillItem(GameObject gameObject, HeroSkill skill)
        {
            _loader = GameClient.Get<ILoadObjectsManager>();
            selfObject = gameObject;
            this.skill = skill;
            icon = selfObject.transform.Find("SpellIcon/Icon").GetComponent<SpriteRenderer>();
            costText = selfObject.transform.Find("SpellCost/SpellCostText").GetComponent<TextMeshPro>();

            string iconPath = "";

            switch (skill.skillType)
            {
                case Enumerators.SkillType.FIREBALL:
                    iconPath = "Images/hero_power_01";
                    break;
                case Enumerators.SkillType.HEAL:
                default:
                    iconPath = "Images/hero_power_02";
                    break;
            }

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
            image = selfObject.GetComponent<Image>();

            if (sprite != null)
                image.sprite = sprite;
        }

        public void Dispose()
        {
            if (selfObject != null)
                GameObject.Destroy(selfObject);
        }
    }
}