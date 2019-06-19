

using System;
using log4net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class RenamePopup : IUIPopup
    {
        private static readonly ILog Log = Logging.GetLog(nameof(RenamePopup));

        private Button _buttonSaveRenameDeck;
        private Button _buttonCancel;
        private Button _buttonContinue;

        private TMP_InputField _inputFieldRenameDeckName;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;

        public GameObject Self { get; private set;  }

        public static Action<string> OnSelectDeckName;

        private string _deckName = string.Empty;
        private bool _isCreatingNewDeck;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/RenamePopup"),
                _uiManager.Canvas2.transform,
                false);

             _inputFieldRenameDeckName = Self.transform.Find("Tab_Rename/Panel_Content/InputText_DeckName").GetComponent<TMP_InputField>();
            _inputFieldRenameDeckName.onEndEdit.AddListener(OnInputFieldRenameEndedEdit);
            _inputFieldRenameDeckName.text = "Deck Name";

            _buttonContinue = Self.transform.Find("Tab_Rename/Panel_Deco/Button_Continue").GetComponent<Button>();
            _buttonContinue.onClick.AddListener(ButtonContinueHandler);

            _buttonSaveRenameDeck = Self.transform.Find("Tab_Rename/Panel_Deco/Button_Save").GetComponent<Button>();
            _buttonSaveRenameDeck.onClick.AddListener(ButtonSaveRenameDeckHandler);

            _buttonCancel = Self.transform.Find("Tab_Rename/Panel_Deco/Button_Cancel").GetComponent<Button>();
            _buttonCancel.onClick.AddListener(ButtonCancelHandler);

            _myDeckPage = GameClient.Get<IUIManager>().GetPage<HordeSelectionWithNavigationPage>();

            if (_isCreatingNewDeck)
            {
                _buttonSaveRenameDeck.gameObject.SetActive(false);
                _buttonContinue.gameObject.SetActive(true);
            }
            else
            {
                _buttonSaveRenameDeck.gameObject.SetActive(true);
                _buttonContinue.gameObject.SetActive(false);
            }

            SetName(_deckName);
        }

        public void Show(object data)
        {
            if(data is object[] param)
            {
                _deckName = (string) param[0];
                _isCreatingNewDeck = (bool) param[1];
            }
            Show();
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void Update()
        {

        }



        public void Dispose()
        {

        }

        public void SetMainPriority()
        {

        }

        private void OnInputFieldRenameEndedEdit(string value)
        {

        }

        public void SetName(string name)
        {
            _inputFieldRenameDeckName.text = name;
        }

        private void ButtonSaveRenameDeckHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSaveRenameDeck.name))
                return;

            DataUtilities.PlayClickSound();
            string newName = _inputFieldRenameDeckName.text;
            _myDeckPage.HordeEditTab.RenameDeck(newName);

            Hide();
        }

        private void ButtonContinueHandler()
        {
            DataUtilities.PlayClickSound();

            string deckName = _inputFieldRenameDeckName.text;
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            if (!deckGeneratorController.VerifyDeckName(deckName))
                return;

            OnSelectDeckName?.Invoke(deckName);

            Hide();

        }

        private void ButtonCancelHandler()
        {
            DataUtilities.PlayClickSound();
            Hide();
        }
    }
}

