using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class CustomGameModeCustomUiPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CustomGameModeCustomUiPage));

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private IDataManager _dataManager;

        private IPvPManager _pvpManager;

        private BackendFacade _backendFacade;

        private GameObject _selfPage;

        private Button _backButton;

        private Button _joinButton;

        private List<GameObject> _uiElements = new List<GameObject>();

        private Transform _customUiRoot;

        public GameMode GameMode { get; private set; }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _pvpManager = GameClient.Get<IPvPManager>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            throw new NotSupportedException();
        }

        public void Show(GameMode gameMode, ICollection<CustomGameModeCustomUiElement> customUiElements)
        {
            GameMode = gameMode;
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CustomGameModeCustomUiPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _joinButton = _selfPage.transform.Find("Button_Join").GetComponent<Button>();

            _customUiRoot = _selfPage.transform.Find("CustomUiContainer");
            _backButton.onClick.AddListener(BackButtonOnClickHandler);
            _joinButton.onClick.AddListener(ChooseButtonOnClickHandler);

            RefreshCustomUi(customUiElements);
        }

        private void RefreshCustomUi(ICollection<CustomGameModeCustomUiElement> customUiElements)
        {
            foreach (GameObject go in _uiElements)
            {
                Object.Destroy(go);
            }

            _uiElements.Clear();

            GameObject labelPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CustomGameModeCustomUiLabel");
            GameObject buttonPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Buttons/Button_Normal");

            foreach (CustomGameModeCustomUiElement customUiElement in customUiElements)
            {
                GameObject elementGameObject;
                switch (customUiElement.UiElementCase)
                {
                    case CustomGameModeCustomUiElement.UiElementOneofCase.None:
                        continue;
                    case CustomGameModeCustomUiElement.UiElementOneofCase.Label:
                    {
                        elementGameObject = Object.Instantiate(labelPrefab);
                        elementGameObject.GetComponentInChildren<TextMeshProUGUI>().text = customUiElement.Label.Text;
                        break;
                    }
                    case CustomGameModeCustomUiElement.UiElementOneofCase.Button:
                        elementGameObject = Object.Instantiate(buttonPrefab);
                        elementGameObject.GetComponentInChildren<TextMeshProUGUI>().text = customUiElement.Button.Title;
                        elementGameObject.GetComponent<Button>().onClick.AddListener(async () =>
                        {
                            try
                            {
                                await _backendFacade.CallCustomGameModeFunction(
                                    Address.FromProtobufAddress(GameMode.Address),
                                    customUiElement.Button.CallData.ToByteArray()
                                    );
                                GetCustomGameModeCustomUiResponse customUiResponse =
                                    await _backendFacade.GetGameModeCustomUi(Address.FromProtobufAddress(GameMode.Address));
                                RefreshCustomUi(customUiResponse.UiElements.ToArray());
                            }
                            catch (Exception e)
                            {
                                Helpers.ExceptionReporter.SilentReportException(e);
                                Log.Warn($"got exception: {e.Message} ->> {e.StackTrace}");
                            }
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RectTransform rectTransform = elementGameObject.GetComponent<RectTransform>();
                rectTransform.SetParent(_customUiRoot, false);

                // left top corner
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0, 1);

                rectTransform.anchoredPosition =
                    new Vector2(
                        customUiElement.Rect.Position.X,
                        -customUiElement.Rect.Position.Y
                    );

                rectTransform.sizeDelta =
                    new Vector2(
                        customUiElement.Rect.Size.X,
                        customUiElement.Rect.Size.Y
                    );

                _uiElements.Add(elementGameObject);
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
            foreach (GameObject go in _uiElements)
            {
                Object.Destroy(go);
            }

            _uiElements.Clear();
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.BackAppState();
            Hide();
        }

        private void ChooseButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _pvpManager.CustomGameModeAddress = Address.FromProtobufAddress(GameMode.Address);
            _stateManager.ChangeAppState(Enumerators.AppState.HordeSelection);
        }
    }
}
