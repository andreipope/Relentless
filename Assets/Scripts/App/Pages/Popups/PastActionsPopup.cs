using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PastActionsPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;

        public GameObject Self { get; private set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Hide()
        {
            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
              _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PastActionPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);
        }

        public void Show(object data)
        {
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                _uiManager.DrawPopup<PastActionsPopup>(new GameActionReport(Enumerators.ActionType.SUMMON_UNIT_CARD, new object[] { }));
            }
        }
    }
}
