

using log4net;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class HordeRenameTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(HordeRenameTab));

        private Button _buttonSaveRenameDeck;
        private Button _buttonBack;

        private TMP_InputField _inputFieldRenameDeckName;

        private HordeSelectionWithNavigationPage _myDeckPage;

        public void Init()
        {

        }

        public void Show(GameObject renameTabObj)
        {
            _inputFieldRenameDeckName = renameTabObj.transform.Find("Panel_Content/InputText_DeckName").GetComponent<TMP_InputField>();
            _inputFieldRenameDeckName.onEndEdit.AddListener(OnInputFieldRenameEndedEdit);
            _inputFieldRenameDeckName.text = "Deck Name";

            _buttonSaveRenameDeck = renameTabObj.transform.Find("Panel_FrameComponents/Lower_Items/Button_Save").GetComponent<Button>();
            _buttonSaveRenameDeck.onClick.AddListener(ButtonSaveRenameDeckHandler);

            _buttonBack = renameTabObj.transform.Find("Image_ButtonBackTray/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _myDeckPage = GameClient.Get<IUIManager>().GetPage<HordeSelectionWithNavigationPage>();
        }

        public void Dispose()
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

            PlayClickSound();
            string newName = _inputFieldRenameDeckName.text;
            _myDeckPage.HordeEditTab.RenameDeck(newName);
        }

        private void ButtonBackHandler()
        {
            _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectDeck);
        }

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}

