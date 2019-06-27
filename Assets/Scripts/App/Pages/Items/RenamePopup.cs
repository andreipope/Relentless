

using System;
using log4net;
using Loom.ZombieBattleground.Data;
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

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;

        public GameObject Self { get; private set;  }

        public static Action<string> OnSelectDeckName;
        public static Action<string> OnSaveNewDeckName;

        private Deck _deck;

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

            if (_deck.Id.Id == -1)
            {
                _buttonSaveRenameDeck.gameObject.SetActive(false);
                _buttonContinue.gameObject.SetActive(true);
            }
            else
            {
                _buttonSaveRenameDeck.gameObject.SetActive(true);
                _buttonContinue.gameObject.SetActive(false);
            }

            SetName(_deck.Name);

            // set the input field not intractable, if there is tutorial
            _inputFieldRenameDeckName.interactable = !GameClient.Get<ITutorialManager>().IsTutorial;
        }

        public void Show(object data)
        {
            _deck = (Deck) data;
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

        private void SetName(string name)
        {
            _inputFieldRenameDeckName.text = name;
        }

        private void ButtonSaveRenameDeckHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSaveRenameDeck.name))
                return;

            DataUtilities.PlayClickSound();
            string newDeckName = _inputFieldRenameDeckName.text;
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            if (!deckGeneratorController.VerifyDeckName(newDeckName))
                return;

            _deck.Name = newDeckName;
            deckGeneratorController.FinishEditDeck += FinishEditDeckName;
            deckGeneratorController.ProcessEditDeck(_deck);

            Hide();
        }

        private void FinishEditDeckName(bool success, Deck deck)
        {
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            deckGeneratorController.FinishEditDeck -= FinishEditDeckName;

            OnSaveNewDeckName?.Invoke(deck.Name);
        }

        private void ButtonContinueHandler()
        {
            DataUtilities.PlayClickSound();

            string newDeckName = _inputFieldRenameDeckName.text;
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            if (!deckGeneratorController.VerifyDeckName(newDeckName))
                return;

            OnSelectDeckName?.Invoke(newDeckName);

            Hide();

        }

        private void ButtonCancelHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonCancel.name))
                return;

            DataUtilities.PlayClickSound();
            Hide();
        }
    }
}

