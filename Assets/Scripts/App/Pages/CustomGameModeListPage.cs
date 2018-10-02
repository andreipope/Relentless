using System.Collections.Generic;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NotImplementedException = System.NotImplementedException;

namespace Loom.ZombieBattleground
{
    public class CustomGameModeListPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private IDataManager _dataManager;

        private BackendFacade _backendFacade;

        private GameObject _selfPage;

        private Button _backButton;


        private List<CustomGameModeListItem> _listItems = new List<CustomGameModeListItem>();

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
        }

        public void Update()
        {
        }

        public async void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CustomGameModeListPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            Transform customGameModeItemsRoot = _selfPage.transform.Find("ContentPanel/ScrollView_List/Viewport/Content");
            _backButton.onClick.AddListener(BackButtonOnClickHandler);


            GameModeList gameModeList = await _backendFacade.GetCustomGameModeList();
            if (gameModeList != null)
            {
                foreach (GameMode gameMode in gameModeList.GameModes)
                {
                    _listItems.Add(new CustomGameModeListItem(customGameModeItemsRoot, gameMode));
                }
            }
        }

        public void Hide()
        {
            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
            Dispose();
        }

        public void Dispose()
        {
            foreach (CustomGameModeListItem gameModeListItem in _listItems)
            {
                gameModeListItem.Dispose();
            }

            _listItems.Clear();
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.BackAppState();
        }

        private class CustomGameModeListItem
        {
            private ILoadObjectsManager _loadObjectsManager;
            private IAppStateManager _stateManager;

            private ISoundManager _soundManager;

            private TextMeshProUGUI _nameText;
            private TextMeshProUGUI _descriptionText;
            private Button _chooseButton;

            public CustomGameModeListItem(Transform parent, GameMode gameMode)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _stateManager = GameClient.Get<IAppStateManager>();
                _soundManager = GameClient.Get<ISoundManager>();

                GameMode = gameMode;
                GameObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CustomGameModeListItem"));
                GameObject.GetComponent<RectTransform>().SetParent(parent, false);

                _nameText = GameObject.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                _descriptionText = GameObject.transform.Find("Description").GetComponent<TextMeshProUGUI>();
                _chooseButton = GameObject.transform.Find("Button_Choose").GetComponent<Button>();

                _chooseButton.onClick.AddListener(ChooseButtonOnClickHandler);

                _nameText.text = $"{gameMode.Name} v.{gameMode.Version}";
                _descriptionText.text = gameMode.Description;
            }

            public GameObject GameObject { get; }
            public GameMode GameMode { get; }

            public void Dispose()
            {
                Object.Destroy(GameObject);
            }

            private void ChooseButtonOnClickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

                GameClient.Get<IMatchManager>().CustomGameAddress =
                    Address.FromString(
                        CryptoUtils.BytesToHexString(GameMode.Address.Local.ToByteArray())
                        );
                _stateManager.ChangeAppState(Enumerators.AppState.HordeSelection);
            }
        }
    }
}
