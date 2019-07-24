using System.Collections.Generic;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class CardInfoWithSearchPopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CardInfoWithSearchPopup));

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ITutorialManager _tutorialManager;

        public GameObject Self { get; private set; }

        private TextMeshProUGUI _textHeader,
                                _textDescription;

        private Button _buttonAdd,
                       _buttonRemove,
                       _buttonBack,
                       _buttonLeftArrow,
                       _buttonRightArrow,
                       _buttonOk;

        private TMP_InputField _inputFieldSearch;

        private Transform _groupCreatureCard;

        private GameObject _cardCreaturePrefab;

        private const float BoardCardScale = 0.8f;

        private List<IReadOnlyCard> _cardList;

        private int _currentCardIndex;


        private UnitCardUI _unitCardUi;

        public enum PopupType
        {
            NONE,
            ADD_CARD,
            REMOVE_CARD
        }

        private PopupType _currentPopupType;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _cardList = new List<IReadOnlyCard>();
            _currentCardIndex = -1;
        }

        public void Dispose()
        {
            if (_cardList != null)
                _cardList.Clear();
        }

        public void Hide()
        {
            Dispose();

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
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardInfoWithSearchPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/CreatureCard_UI");

            _buttonLeftArrow = Self.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);

            _buttonRightArrow = Self.transform.Find("Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);

            _buttonOk = Self.transform.Find("Panel/Button_Ok").GetComponent<Button>();
            _buttonOk.onClick.AddListener(ButtonOkHandler);

            _buttonAdd = Self.transform.Find("Panel/Button_AddToDeck").GetComponent<Button>();
            _buttonAdd.onClick.AddListener(ButtonAddCardHandler);

            _buttonRemove = Self.transform.Find("Panel/Button_Remove").GetComponent<Button>();
            _buttonRemove.onClick.AddListener(ButtonRemoveCardHandler);

            _buttonBack = Self.transform.Find("Background/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            //_inputFieldSearch = Self.transform.Find("InputText_SearchDeckName").GetComponent<TMP_InputField>();
            //_inputFieldSearch.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            //_inputFieldSearch.text = "";

            _textDescription = Self.transform.Find("Panel/Text_CardDesc").GetComponent<TextMeshProUGUI>();

            _groupCreatureCard = Self.transform.Find("Group_CreatureCard");

            CreateUnitCard();
            UpdateCardDetails();
            UpdatePopupType();
        }

        private void CreateUnitCard()
        {
            GameObject go = Object.Instantiate(_cardCreaturePrefab);
            go.transform.SetParent(_groupCreatureCard);
            go.transform.localScale = Vector3.one * BoardCardScale;
            go.transform.localPosition = Vector3.zero;

            _unitCardUi = new UnitCardUI();
            _unitCardUi.Init(go);
        }

        private void UpdateCardDetails()
        {
            _unitCardUi.FillCardData(_cardList[_currentCardIndex] as Card);
            _textDescription.text = !string.IsNullOrEmpty(_cardList[_currentCardIndex].FlavorText) ? _cardList[_currentCardIndex].FlavorText : string.Empty;
        }

        public void Show(object data)
        {
            if (data is object[] param)
            {
                _cardList = (List<IReadOnlyCard>)param[0];
                IReadOnlyCard card = (IReadOnlyCard)param[1];
                _currentCardIndex = _cardList.IndexOf(card);
                _currentPopupType = (PopupType)param[2];

            }
            Show();
        }

        public void Update()
        {
        }

        #region UI Handlers

        private void ButtonBackHandler()
        {
            PlayClickSound();
            Hide();
        }

        private void ButtonAddCardHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonAdd.name))
                return;

            PlayClickSound();
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.AddCardToDeck
            (
                _cardList[_currentCardIndex],
                true
            );
            Hide();
        }

        private void ButtonOkHandler()
        {
            PlayClickSound();
            Hide();
        }

        private void ButtonRemoveCardHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRemove.name))
                return;

            PlayClickSound();
            _uiManager.GetPage<HordeSelectionWithNavigationPage>().HordeEditTab.RemoveCardFromDeck
            (
                _cardList[_currentCardIndex],
                true
            );
            Hide();
        }

        private void ButtonLeftArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonLeftArrow.name))
                return;

            PlayClickSound();
            MoveCardIndex(-1);
        }

        private void ButtonRightArrowHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRightArrow.name))
                return;

            PlayClickSound();
            MoveCardIndex(1);
        }

        #endregion

        private void UpdatePopupType()
        {
            switch(_currentPopupType)
            {
                case PopupType.NONE:
                    _buttonOk.gameObject.SetActive(true);
                    _buttonAdd.gameObject.SetActive(false);
                    _buttonRemove.gameObject.SetActive(false);
                    break;
                case PopupType.ADD_CARD:
                    _buttonOk.gameObject.SetActive(false);
                    _buttonAdd.gameObject.SetActive(true);
                    _buttonRemove.gameObject.SetActive(false);
                    break;
                case PopupType.REMOVE_CARD:
                    _buttonOk.gameObject.SetActive(false);
                    _buttonAdd.gameObject.SetActive(false);
                    _buttonRemove.gameObject.SetActive(true);
                    break;
                default:
                    return;
            }
        }

        private void MoveCardIndex(int direction)
        {
            if(_cardList == null)
                Log.Info($"Current Card List in {nameof(CardInfoWithSearchPopup)} is null");

            if (_cardList.Count <= 1)
            {
                _currentCardIndex = _cardList.Count-1;
            }
            else
            {
                int newIndex = _currentCardIndex + direction;

                if (newIndex < 0)
                    newIndex = _cardList.Count - 1;
                else if (newIndex >= _cardList.Count)
                    newIndex = 0;

                _currentCardIndex = newIndex;
            }
            UpdateCardDetails();
        }

        #region Util

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #endregion
    }
}
