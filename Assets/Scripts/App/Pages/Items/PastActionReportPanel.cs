using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class PastActionReportPanel
    {
        private readonly GameObject _playedCardPrefab;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly ActionsReportController _actionsReportController;

        private readonly GameObject _selfPanel;

        private readonly VerticalLayoutGroup _reportGroup;

        private List<PastReportActionSmall> _pastReportActionsSmall;

        public bool IsDrawing { get; set; }

        public PastActionReportPanel(GameObject gameObject)
        {
            ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();

            _actionsQueueController = gameplayManager.GetController<ActionsQueueController>();
            _actionsReportController = gameplayManager.GetController<ActionsReportController>();

            _selfPanel = gameObject.transform.Find("Viewport/CardGraveyard").gameObject;
            _reportGroup = _selfPanel.GetComponent<VerticalLayoutGroup>();

            _playedCardPrefab = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");

            _actionsReportController.GotNewActionReportEvent += GotNewActionReportEventHandler;

            _reportGroup.padding.top = 0;

            _pastReportActionsSmall = new List<PastReportActionSmall>();
        }

        public void Update()
        {
            if (_pastReportActionsSmall != null)
            {
                foreach (PastReportActionSmall action in _pastReportActionsSmall)
                {
                    action.Update();
                }
            }
        }

        public void Dispose()
        {
            _actionsReportController.GotNewActionReportEvent -= GotNewActionReportEventHandler;
        }

        public void Clear()
        {
            foreach(PastReportActionSmall action in _pastReportActionsSmall)
            {
                action.Dispose();
            }
            _pastReportActionsSmall.Clear();

            _reportGroup.padding.top = 0;
        }

        private void GotNewActionReportEventHandler(PastActionsPopup.PastActionParam report)
        {
            _pastReportActionsSmall.Add(new PastReportActionSmall(this, _playedCardPrefab, _selfPanel.transform, report));
        }
    }
}
