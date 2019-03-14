using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.Client;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class CustomGameModeListPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CustomGameModeListPage));

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IAppStateManager _appStateManager;

        private ISoundManager _soundManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private GameObject _selfPage;

        private Button _backButton;

        private Transform _parentOfModesList;

        private List<CustomModeItem> _customModesList;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
            UpdateLocalization();

            _customModesList = new List<CustomModeItem>();
        }

        public void Update()
        {

        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CustomModesPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _parentOfModesList = _selfPage.transform.Find("Panel_ModesList/Viewport/Content");

            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _backButton.onClick.AddListener(BackButtonOnClickHandler);

            FillCustomModes();
        }

        public void Hide()
        {
            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
        }


        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _appStateManager.BackAppState();
        }

        private async void FillCustomModes()
        {
            ResetCustomModes();

            try
            {
                GameModeList gameModeList = await _backendFacade.GetCustomGameModeList();

                if (gameModeList != null)
                {
                    foreach (GameMode gameMode in gameModeList.GameModes)
                    {
                        _customModesList.Add(new CustomModeItem(_parentOfModesList, gameMode));
                    }
                }
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Info("save deck exception === " + e);
            }
        }

        private void ResetCustomModes()
        {
            foreach (CustomModeItem gameModeListItem in _customModesList)
            {
                gameModeListItem.Dispose();
            }

            _customModesList.Clear();
        }

        public class CustomModeItem
        {
            private GameObject _selfObject;

            private ILoadObjectsManager _loadObjectsManager;

            private IAppStateManager _stateManager;

            private ISoundManager _soundManager;

            private IPvPManager _pvpManager;

            private TextMeshProUGUI _titleText,
                                    _descriptionText;

            private Image _modePictureImage;

            private Button _playButton;

            public GameMode Mode;

            public CustomModeItem(Transform parent, GameMode mode)
            {
                Mode = mode;

                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _stateManager = GameClient.Get<IAppStateManager>();
                _soundManager = GameClient.Get<ISoundManager>();
                _pvpManager = GameClient.Get<IPvPManager>();

                _selfObject = Object.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Item_CustomMode"), parent, false);


                _titleText = _selfObject.transform.Find("Text_ModeTitle").GetComponent<TextMeshProUGUI>();
                _descriptionText = _selfObject.transform.Find("Text_ModeDescription").GetComponent<TextMeshProUGUI>();

                _modePictureImage = _selfObject.transform.Find("Image_ModePicture").GetComponent<Image>();

                _playButton = _selfObject.transform.Find("Button_Play").GetComponent<Button>();

                _playButton.onClick.AddListener(PlayButtonOnClickHandler);

                _titleText.text = $"{Mode.Name} v.{Mode.Version}";
                _descriptionText.text = Mode.Description;

                //_modePictureImage.sprite = null; // TODO: implement pictures logic
            }


            public void Dispose()
            {
                Object.Destroy(_selfObject);
            }

            private async void PlayButtonOnClickHandler()
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

                try
                {
                    GetCustomGameModeCustomUiResponse customUiResponse =
                        await GameClient.Get<BackendFacade>()
                            .GetGameModeCustomUi(Address.FromProtobufAddress(Mode.Address));

                    RepeatedField<CustomGameModeCustomUiElement> customUiElements = customUiResponse?.UiElements;
                    if (customUiElements?.Count > 0)
                    {
                        GameClient.Get<IUIManager>().GetPage<CustomGameModeListPage>().Hide();
                        GameClient.Get<IUIManager>().GetPage<CustomGameModeCustomUiPage>().Show(Mode, customUiElements);
                    }
                    else
                    {
                        _pvpManager.CustomGameModeAddress = Address.FromProtobufAddress(Mode.Address);
                        _stateManager.ChangeAppState(Enumerators.AppState.HordeSelection);
                    }
                }
                catch (Exception e)
                {
                    Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);
                }
            }
        }
    }
}
