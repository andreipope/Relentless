using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class CardInfoWithSearchPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        public GameObject Self { get; private set; }

        private TextMeshProUGUI _textCardName,
                                _textCardDescription;

        private Button _buttonAdd,
                       _buttonRemove,
                       _buttonBack,
                       _buttonLeftArrow,
                       _buttonRightArrow;

        private TMP_InputField _inputFieldSearch;

        private Transform _groupCreatureCard;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }
        
        public void Dispose()
        {
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
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardInfoWithSearchPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _buttonLeftArrow = Self.transform.Find("Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrow.onClick.AddListener(ButtonLeftArrowHandler);
            _buttonLeftArrow.onClick.AddListener(PlayClickSound);
            
            _buttonRightArrow = Self.transform.Find("Button_RightArrow").GetComponent<Button>();
            _buttonRightArrow.onClick.AddListener(ButtonRightArrowHandler);
            _buttonRightArrow.onClick.AddListener(PlayClickSound);
            
            _buttonAdd = Self.transform.Find("Button_AddToDeck").GetComponent<Button>();
            _buttonAdd.onClick.AddListener(ButtonAddCardHandler);
            _buttonAdd.onClick.AddListener(PlayClickSound);
            
            _buttonRemove = Self.transform.Find("Button_Remove").GetComponent<Button>();
            _buttonRemove.onClick.AddListener(ButtonRemoveCardHandler);
            _buttonRemove.onClick.AddListener(PlayClickSound);
            
            _buttonBack = Self.transform.Find("Background/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);
            _buttonBack.onClick.AddListener(PlayClickSound);
            
            _inputFieldSearch = Self.transform.Find("InputText_SearchDeckName").GetComponent<TMP_InputField>();
            _inputFieldSearch.onEndEdit.AddListener(OnInputFieldSearchEndedEdit);
            _inputFieldSearch.text = "";
            
            _textCardName = Self.transform.Find("Text_CardName").GetComponent<TextMeshProUGUI>();            
            _textCardDescription = Self.transform.Find("Text_CardDesc").GetComponent<TextMeshProUGUI>();

            _groupCreatureCard = Self.transform.Find("Group_CreatureCard");
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #region UI Handlers
        
        private void ButtonBackHandler()
        {
            Hide();
        }
        
        private void ButtonAddCardHandler()
        {

        }
        
        private void ButtonRemoveCardHandler()
        {

        }
        
        private void ButtonLeftArrowHandler()
        {

        }
        
        private void ButtonRightArrowHandler()
        {

        }
        
        public void OnInputFieldSearchEndedEdit(string value)
        {
        
        }

        #endregion

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