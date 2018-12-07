using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Random = UnityEngine.Random;
using Rect = UnityEngine.Rect;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public class FakeClientWindow : EditorWindow
    {
        [SerializeField]
        private PlayerActionLogView _playerActionLogView = new PlayerActionLogView();

        private readonly Queue<Func<Task>> _asyncTaskQueue = new Queue<Func<Task>>();
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        private BackendFacade _backendFacade;
        private UserDataModel _userDataModel;
        private MatchMakingFlowController _matchMakingFlowController;
        private MatchRequestFactory _matchRequestFactory;
        private PlayerActionFactory _playerActionFactory;
        private Vector2 _scrollPosition;
        private bool _useBackendLogic;
        private GameState _initialGameState;

        [MenuItem("Window/ZombieBattleground/Open new FakeClient")]
        private static void OpenWindow()
        {
            FakeClientWindow window = CreateInstance<FakeClientWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Fake Client");
        }

        private async void Update()
        {
            await _updateSemaphore.WaitAsync();
            try
            {
                Repaint();
                await Task.Delay((int) (Time.deltaTime * 1000));
                await AsyncUpdate();
                Repaint();
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private async Task AsyncUpdate()
        {
            if (_matchMakingFlowController != null)
            {
                await _matchMakingFlowController.Update();
            }

            while (_asyncTaskQueue.Count > 0)
            {
                Func<Task> taskFunc = _asyncTaskQueue.Dequeue();
                await taskFunc();
            }
        }

        private void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.BeginVertical();
                {
                    DrawMainGui();
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMainGui()
        {
            GUILayout.Label("<b>Connection</b>", Styles.RichLabel);
            {
                if (_backendFacade == null)
                {
                    if (GUILayout.Button("Create client"))
                    {
                        QueueAsyncTask(async () =>
                        {
                            await ResetClient();

                            _userDataModel = new UserDataModel(
                                "TestFakeUser_" + Random.Range(int.MinValue, int.MaxValue).ToString().Replace("-", "0"),
                                CryptoUtils.GeneratePrivateKey()
                            );

                            _backendFacade = new BackendFacade(GameClient.GetDefaultBackendEndpoint())
                            {
                                Logger = Debug.unityLogger
                            };
                            _backendFacade.Init();
                            _backendFacade.PlayerActionDataReceived += OnPlayerActionDataReceived;
                            await _backendFacade.CreateContract(_userDataModel.PrivateKey);
                            try
                            {
                                await _backendFacade.SignUp(_userDataModel.UserId);
                            }
                            catch (TxCommitException e) when (e.Message.Contains("user already exists"))
                            {
                                // Ignore
                            }

                            _matchMakingFlowController = new MatchMakingFlowController(_backendFacade, _userDataModel);
                            _matchMakingFlowController.ActionWaitingTime = 0.3f;
                            _matchMakingFlowController.MatchConfirmed += OnMatchConfirmed;
                        });
                    }
                }
                else
                {
                    if (_userDataModel != null)
                    {
                        GUILayout.Label("User ID: " + _userDataModel.UserId);
                    }

                    if (_backendFacade.Contract != null)
                    {
                        GUILayout.Label("State: " + _backendFacade.Contract.Client.ReadClient.ConnectionState);
                    }

                    if (GUILayout.Button("Kill client"))
                    {
                        QueueAsyncTask(async () =>
                        {
                            await ResetClient();
                        });
                    }
                }
            }

            if (_backendFacade != null && _matchMakingFlowController != null)
            {
                DrawSeparator();

                GUILayout.Label("<b>Matchmaking</b>", Styles.RichLabel);
                {
                    GUILayout.Label("State: " + _matchMakingFlowController.State);
                    if (!_matchMakingFlowController.IsMatchmakingInProcess)
                    {
                        if (GUILayout.Button("Start matchmaking"))
                        {
                            QueueAsyncTask(async () =>
                            {
                                await _matchMakingFlowController.Start(1, null);
                            });
                        }

                        GUI.enabled = _matchMakingFlowController.State != MatchMakingFlowController.MatchMakingState.Confirmed;
                        _useBackendLogic = EditorGUILayout.ToggleLeft("Use Backend Logic", _useBackendLogic);
                        GUI.enabled = true;
                    }
                    else
                    {
                        if (GUILayout.Button("Stop matchmaking"))
                        {
                            QueueAsyncTask(async () =>
                            {
                                await _matchMakingFlowController.Stop();
                            });
                        }
                    }

                    if (_matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed)
                    {
                        GUILayout.Label(
                            "Match Metadata:\n" +
                            $"  Id: {_matchMakingFlowController.MatchMetadata.Id}\n" +
                            $"  Topics: {String.Join("", "", _matchMakingFlowController.MatchMetadata.Topics)}\n" +
                            $"  UseBackendGameLogic: {_matchMakingFlowController.MatchMetadata.UseBackendGameLogic}"
                        );
                    }
                }

                if (_matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed)
                {
                    DrawSeparator();

                    GUILayout.Label("<b>Game Actions</b>", Styles.RichLabel);
                    {
                        if (GUILayout.Button("End Turn"))
                        {
                            QueueAsyncTask(async () =>
                            {
                                await _backendFacade.SendPlayerAction(
                                    _matchRequestFactory.CreateAction(_playerActionFactory.EndTurn())
                                );
                            });
                        }

                        if (GUILayout.Button("Leave Match"))
                        {
                            QueueAsyncTask(async () =>
                            {
                                await _backendFacade.SendPlayerAction(
                                    _matchRequestFactory.CreateAction(_playerActionFactory.LeaveMatch())
                                );
                            });
                        }
                    }
                }
            }

            if ((_matchMakingFlowController != null && _matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed) ||
                _playerActionLogView.PlayerActions.Count > 0)
            {
                DrawSeparator();

                GUILayout.Label("<b>Action Log</b>", Styles.RichLabel);
                {
                    _playerActionLogView.Draw();
                }
            }
        }

        private async Task ResetClient()
        {
            _playerActionLogView = new PlayerActionLogView();

            if (_backendFacade != null)
            {
                _backendFacade.PlayerActionDataReceived -= OnPlayerActionDataReceived;
                _backendFacade.Contract?.Client.Dispose();

                _backendFacade = null;

                await _matchMakingFlowController.Stop();
                _matchMakingFlowController = null;
            }
        }

        private async void OnMatchConfirmed(MatchMetadata metadata)
        {
            _matchRequestFactory = new MatchRequestFactory(metadata.Id);
            _playerActionFactory = new PlayerActionFactory(_userDataModel.UserId);
            GetGameStateResponse getGameStateResponse = await _backendFacade.GetGameState(metadata.Id);

            _playerActionLogView.Add(
                new PlayerActionLogView.PlayerActionEventViewModel(
                    _playerActionLogView.PlayerActions.Count,
                    "",
                    "Initial State",
                    getGameStateResponse.GameState.ToString(),
                    null
                    )
                );
            _initialGameState = getGameStateResponse.GameState;
        }

        private static void DrawSeparator()
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 1;
            EditorGUI.DrawRect(rect, Color.black);
            EditorGUILayout.Space();
        }

        private void OnPlayerActionDataReceived(byte[] data)
        {
            PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(data);
            bool? isLocalPlayer =
                playerActionEvent.PlayerAction != null ?
                    playerActionEvent.PlayerAction.PlayerId == _userDataModel.UserId :
                    (bool?) null;

            _playerActionLogView.Add(playerActionEvent, isLocalPlayer);
        }

        private void QueueAsyncTask(Func<Task> task)
        {
            _asyncTaskQueue.Enqueue(task);
        }

        private static class Styles
        {
            public static GUIStyle RichLabel;

            public static GUIStyle BoxLeftAlign;

            static Styles()
            {
                RichLabel = new GUIStyle(EditorStyles.label);
                RichLabel.richText = true;

                BoxLeftAlign = new GUIStyle(GUI.skin.box);
                BoxLeftAlign.alignment = TextAnchor.UpperLeft;
            }
        }

        [Serializable]
        private class PlayerActionLogView
        {
            private const int IdColumnIndex = 0;
            private const int MatchColumnIndex = 1;
            private const int ActionTypeColumnIndex = 2;
            private const int IsLocalPlayerColumnIndex = 3;
            private const int ActionColumnIndex = 4;

            [NonSerialized]
            private bool _initialized;

            [SerializeField]
            private List<PlayerActionEventViewModel> _playerActions = new List<PlayerActionEventViewModel>();

            [SerializeField]
            private MultiColumnHeaderState _multiColumnHeaderState;

            [SerializeField]
            private MultiColumnHeader _header;

            public List<PlayerActionEventViewModel> PlayerActions => _playerActions;

            public void Draw()
            {
                InitIfNeeded();
                Rect headerRect = EditorGUILayout.GetControlRect(false, 20);
                _header.OnGUI(headerRect, 0);

                for (int i = 0; i < _playerActions.Count; i++)
                {
                    PlayerActionEventViewModel playerAction = _playerActions[i];

                    Rect rowRect = GUILayoutUtility.GetRect(0, 1000000000, 20, 20, Styles.BoxLeftAlign);
                    PostprocessCellRect(ref rowRect);
                    GUI.Label(rowRect, "", Styles.BoxLeftAlign);

                    Rect idRect = _header.GetCellRect(IdColumnIndex, rowRect);
                    PostprocessCellRect(ref idRect);
                    GUI.Label(idRect, playerAction.Id, Styles.BoxLeftAlign);

                    Rect matchRect = _header.GetCellRect(MatchColumnIndex, rowRect);
                    PostprocessCellRect(ref matchRect);
                    if (GUI.Button(matchRect, playerAction.MatchPreview, Styles.BoxLeftAlign))
                    {
                        OpenInDataPreviewWindow(playerAction.Match);
                    }

                    Rect actionTypeRect = _header.GetCellRect(ActionTypeColumnIndex, rowRect);
                    PostprocessCellRect(ref actionTypeRect);
                    GUI.Label(actionTypeRect, playerAction.ActionType, Styles.BoxLeftAlign);

                    if (playerAction.HasLocalPlayer)
                    {
                        Rect isLocalPlayerRect = _header.GetCellRect(IsLocalPlayerColumnIndex, rowRect);
                        isLocalPlayerRect.xMin += 10;
                        isLocalPlayerRect.yMin += 2;
                        EditorGUI.Toggle(isLocalPlayerRect, playerAction.IsLocalPlayer);
                    }

                    Rect actionRect = _header.GetCellRect(ActionColumnIndex, rowRect);
                    PostprocessCellRect(ref actionRect);
                    if (GUI.Button(actionRect, playerAction.ActionPreview, Styles.BoxLeftAlign))
                    {
                        OpenInDataPreviewWindow(playerAction.Action);
                    }
                }
            }

            private void OpenInDataPreviewWindow(string json)
            {
                try
                {
                    json = JToken.Parse(json).ToString(Formatting.Indented);
                }
                catch
                {
                    // Ignore
                }

                FakeClientDataPreviewWindow dataPreviewWindow = GetWindow<FakeClientDataPreviewWindow>(GetType());
                dataPreviewWindow.SetText(json);
            }

            public void Add(PlayerActionEvent actionEvent, bool? isLocalPlayer)
            {
                Add(new PlayerActionEventViewModel(actionEvent, _playerActions.Count, isLocalPlayer));
            }

            public void Add(PlayerActionEventViewModel playerActionEventViewModel)
            {
                _playerActions.Add(playerActionEventViewModel);
            }

            private void InitIfNeeded()
            {
                if (_initialized)
                    return;

                MultiColumnHeaderState headerState =
                    new MultiColumnHeaderState(
                        new[]
                        {
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("#"),
                                headerTextAlignment = TextAlignment.Center,
                                width = 32,
                                minWidth = 32,
                                maxWidth = 32,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Match"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 200,
                                minWidth = 200,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Action Type"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 100,
                                minWidth = 100,
                                maxWidth = 100,
                                autoResize = true,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Me"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 30,
                                minWidth = 30,
                                maxWidth = 30,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Value"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 300,
                                minWidth = 300,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            }
                        });
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);
                _multiColumnHeaderState = headerState;
                _header = new MultiColumnHeader(_multiColumnHeaderState);

                _initialized = true;
            }

            private static void PostprocessCellRect(ref Rect rect)
            {
                rect.xMin += 2;
                rect.xMax += 3;
            }

            [Serializable]
            internal class PlayerActionEventViewModel
            {
                private const int MaxPreviewTextLength = 256;

                public GUIContent Id;
                public GUIContent ActionType;
                public string Match;
                public string Action;
                public GUIContent MatchPreview;
                public GUIContent ActionPreview;
                public bool HasLocalPlayer;
                public bool IsLocalPlayer;

                public PlayerActionEventViewModel(PlayerActionEvent playerAction, long id, bool? isLocalPlayer)
                    : this(
                        id,
                        playerAction.Match.ToString(),
                        playerAction.PlayerAction != null ? playerAction.PlayerAction.ActionType.ToString() : "Matchmaking",
                        playerAction.PlayerAction != null ? playerAction.PlayerAction.ToString() : "Match Status: " + playerAction.Match.Status,
                        isLocalPlayer
                        )
                {
                }

                public PlayerActionEventViewModel(long id, string match, string actionType, string action, bool? isLocalPlayer)
                {
                    Action = action;
                    Match = match;
                    Id = new GUIContent(id.ToString());

                    string matchPreview = Utilites.LimitStringLength(match, MaxPreviewTextLength);
                    string actionPreview = Utilites.LimitStringLength(action, MaxPreviewTextLength);
                    MatchPreview = new GUIContent(matchPreview, matchPreview);
                    ActionType = new GUIContent(actionType);
                    ActionPreview = new GUIContent(actionPreview, actionPreview);
                    HasLocalPlayer = isLocalPlayer.HasValue;
                    IsLocalPlayer = isLocalPlayer ?? false;
                }
            }
        }
    }
}
