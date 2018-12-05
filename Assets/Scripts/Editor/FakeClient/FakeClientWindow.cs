using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class FakeClientWindow : EditorWindow
{
    private Queue<Func<Task>> _asyncTaskQueue = new Queue<Func<Task>>();
    private BackendFacade _backendFacade;
    private UserDataModel _userDataModel;
    private MatchMakingFlowController _matchMakingFlowController;
    private SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

    private GUIStyle _richLabel;

    [MenuItem("Window/ZombieBattleground/Open new FakeClient")]
    private static void OpenWindow()
    {
        FakeClientWindow window = CreateInstance<FakeClientWindow>();
        window.Show();
    }

    private void Awake()
    {
        _richLabel = new GUIStyle(EditorStyles.label);
        _richLabel.richText = true;
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
        GUILayout.Label("<b>Connection</b>", _richLabel);
        {
            if (_backendFacade == null)
            {
                if (GUILayout.Button("Create client"))
                {
                    QueueAsyncTask(async () =>
                    {
                        _userDataModel = new UserDataModel("Fake_" + Random.Range(int.MinValue, int.MaxValue), CryptoUtils.GeneratePrivateKey());
                        Debug.Log("User ID: " + _userDataModel.UserId);
                        _backendFacade = new BackendFacade(GameClient.GetDefaultBackendEndpoint())
                        {
                            Logger = Debug.unityLogger
                        };
                        _backendFacade.Init();
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
                    });
                }
            }
            else
            {
                if (_userDataModel != null)
                {
                    GUILayout.Label("User ID: " + _userDataModel.UserId);
                }

                if (GUILayout.Button("Kill client"))
                {
                    QueueAsyncTask(async () =>
                    {
                        _backendFacade.Contract.Client.Dispose();
                        _backendFacade = null;
                        await _matchMakingFlowController.Stop();
                        _matchMakingFlowController = null;
                    });
                }
            }

            if (_backendFacade == null ||
                _matchMakingFlowController == null)
                return;

            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }
        GUILayout.Label("<b>Matchmaking</b>", _richLabel);
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
                    $"  UseBackendGameLogic: {_matchMakingFlowController.MatchMetadata.UseBackendGameLogic}\n"
                );
            }
        }

        if (_matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed)
        {
            GUILayout.Label("<b>Game Actions</b>", _richLabel);
            {
                if (GUILayout.Button("End Turn"))
                {
                    QueueAsyncTask(async () =>
                    {
                        //await _backendFacade.SendPlayerAction()
                    });
                }
            }
        }
    }

    private void QueueAsyncTask(Func<Task> task)
    {
        _asyncTaskQueue.Enqueue(task);
    }
}
