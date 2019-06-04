using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Iap;
using OneOf;
using OneOf.Types;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class IapProcessPurchaseDebugWindow : EditorWindow
    {
        private string _receiptJson = "";

        private readonly Queue<Func<Task>> _asyncTaskQueue = new Queue<Func<Task>>();
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

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

            IapMediator iapMediator = GameClient.Get<IapMediator>();
            AuthFiatApiFacade authFiatApiFacade = GameClient.Get<AuthFiatApiFacade>();
            FiatPlasmaManager fiatPlasmaManager = GameClient.Get<FiatPlasmaManager>();

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
    }
}
