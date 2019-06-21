using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.Iap;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class IapProcessPurchaseDebugWindow : EditorWindow
    {
        private string _receiptJson = "";
        private string _transactionResponseJson = "";

        private readonly Queue<Func<Task>> _asyncTaskQueue = new Queue<Func<Task>>();
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);
        private Tab _currentTab;

        private async void Update()
        {
            await _updateSemaphore.WaitAsync();
            try
            {
                Repaint();
                await Task.Delay((int) (Time.deltaTime * 1000));
                await AsyncUpdate();
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private async Task AsyncUpdate()
        {
            while (_asyncTaskQueue.Count > 0)
            {
                try
                {
                    Func<Task> taskFunc = _asyncTaskQueue.Peek();
                    await taskFunc();
                }
                finally
                {
                    _asyncTaskQueue.Dequeue();
                }
            }
        }

        private void EnqueueAsyncTask(Func<Task> task)
        {
            _asyncTaskQueue.Enqueue(task);
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying || GameClient.Instance == null)
            {
                GUILayout.Label("Only available during gameplay");
                return;
            }

            _currentTab = (Tab) GUILayout.Toolbar(
                (int) _currentTab,
                Enum.GetNames(typeof(Tab)),
                "LargeButton"
            );
            switch (_currentTab)
            {
                case Tab.ProcessReceipt:
                    DrawProcessReceipt();
                    break;
                case Tab.RequestPack:
                    DrawRequestPack();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawRequestPack()
        {
            PlasmaChainBackendFacade plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();

            GUIStyle guiStyle = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true
            };
            EditorGUILayout.LabelField("Transaction response JSON");
            _transactionResponseJson = EditorGUILayout.TextArea(_transactionResponseJson, guiStyle, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Claim"))
            {
                AuthFiatApiFacade.TransactionReceipt transactionReceipt = JsonConvert.DeserializeObject<AuthFiatApiFacade.TransactionReceipt>(_transactionResponseJson);
                EnqueueAsyncTask(async () =>
                {
                    DAppChainClient client = await plasmaChainBackendFacade.GetConnectedClient();
                    using (client)
                    {
                        await plasmaChainBackendFacade.ClaimPacks(client, transactionReceipt);
                    }
                });
            }
        }

        private void DrawProcessReceipt()
        {
            IapMediator iapMediator = GameClient.Get<IapMediator>();
            AuthFiatApiFacade authFiatApiFacade = GameClient.Get<AuthFiatApiFacade>();
            PlasmaChainBackendFacade plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();

            GUIStyle guiStyle = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true
            };
            EditorGUILayout.LabelField("Receipt JSON");
            _receiptJson = EditorGUILayout.TextArea(_receiptJson, guiStyle, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Process"))
            {
                EnqueueAsyncTask(async () =>
                {
                    await iapMediator.ExecutePostPurchaseProcessing(_receiptJson);
                });
            }
        }

        [MenuItem("Window/ZombieBattleground/Open IapProcessPurchaseDebugWindow")]
        private static void OpenWindow()
        {
            IapProcessPurchaseDebugWindow window = CreateInstance<IapProcessPurchaseDebugWindow>();
            window.titleContent = new GUIContent("IAP Purchase Processing Debug");
            window.Show();
        }

        private enum Tab
        {
            ProcessReceipt,
            RequestPack
        }
    }
}
