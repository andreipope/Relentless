using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MulliganPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        public event Action<List<CardModel>> MulliganCards;

        private ILoadObjectsManager _loadObjectsManager;
        private ISoundManager _soundManager;
        private IUIManager _uiManager;
        private IGameplayManager _gameplayManager;

        private GameObject _unitCardPrefab;
        private Transform _cardsParentTransform;

        private Button _buttonContinue;

        private List<MulliganUnitCard> _mulliganCards;

        private const float UnitCardSize = 0.55f;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _unitCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/CreatureCard_UI");
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
              _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/MulliganPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _buttonContinue = Self.transform.Find("Button_Continue").GetComponent<ButtonShiftingContent>();
            _buttonContinue.onClick.AddListener(ContinueButtonOnClickHandler);

            _cardsParentTransform = Self.transform.Find("Cards").transform;

            FillCards();
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private void FillCards()
        {
            _mulliganCards = new List<MulliganUnitCard>();

            for (int i = 0; i < _gameplayManager.CurrentPlayer.MulliganCards.Count; i++)
            {
                CardModel card = _gameplayManager.CurrentPlayer.MulliganCards[i];

                GameObject cardObj = Object.Instantiate(_unitCardPrefab, _cardsParentTransform, false);
                cardObj.transform.localScale = Vector3.one * UnitCardSize;

                UnitCardUI unitCardUi = new UnitCardUI();
                unitCardUi.Init(cardObj);
                unitCardUi.FillCardData((Card)card.Prototype, 0);

                MulliganUnitCard mulliganUnitCard = new MulliganUnitCard(unitCardUi, true);
                _mulliganCards.Add(mulliganUnitCard);

                MultiPointerClickHandler multiPointerClickHandler = cardObj.AddComponent<MultiPointerClickHandler>();
                multiPointerClickHandler.SingleClickReceived += () => { BoardCardSingleClickHandler(mulliganUnitCard); };
            }
        }

        private void BoardCardSingleClickHandler(MulliganUnitCard mulliganUnitCard)
        {
            mulliganUnitCard.SelectCard(!mulliganUnitCard.IsSelected);
        }



        public void ContinueButtonOnClickHandler()
        {
            if (GameClient.Get<IMatchManager>().MatchType != Enumerators.MatchType.PVP)
            {
                // TODO : Not sure what to send here
                //_gameplayManager.GetController<CardsController>().CardsDistribution(_mulliganCards.FindAll((x) => x.CardShouldBeChanged).Select((k) => k.CardModel).ToList());
            }

            // TODO : Not sure what to send here
            //InvokeMulliganCardsEvent(_mulliganCardItems.FindAll((x) => !x.CardShouldBeChanged).Select((k) => k.CardModel).ToList());

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<MulliganPopup>();
        }

        public void InvokeMulliganCardsEvent(List<CardModel> cards)
        {
            MulliganCards?.Invoke(cards);
        }
    }

    public class MulliganUnitCard
    {
        private readonly UnitCardUI _unitCardUi;
        public bool IsSelected;

        private readonly GameObject _crossGameObject;
        private const float CrossScaleSize = 2f;

        public MulliganUnitCard(UnitCardUI unitCardUi, bool isSelected)
        {
            _unitCardUi = unitCardUi;

            GameObject crossPrefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/UI/Cards/MulliganCross");
            _crossGameObject = Object.Instantiate(crossPrefab, unitCardUi.GetGameObject().transform, false);
            _crossGameObject.transform.localScale = Vector3.one * CrossScaleSize;
            SelectCard(isSelected);
        }

        public void SelectCard(bool select)
        {
            IsSelected = select;
            _crossGameObject.SetActive(!IsSelected);
            _unitCardUi.GrayScaleCard(IsSelected);
        }
    }
}
