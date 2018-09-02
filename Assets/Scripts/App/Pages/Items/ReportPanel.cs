using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class ReportPanelItem
    {
        public GameObject PlayedCardPrefab;

        private readonly ILoadObjectsManager _loadObjectsManager;

        private readonly IGameplayManager _gameplayManager;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly GameObject _selfPanel;

        private readonly VerticalLayoutGroup _reportGroup;

        private readonly List<ReportViewBase> _allReports;

        private ITimerManager _timerManager;

        private CardsController _cardsController;

        public ReportPanelItem()
        {
        }

        public ReportPanelItem(GameObject gameObject)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _cardsController = _gameplayManager.GetController<CardsController>();

            _allReports = new List<ReportViewBase>();

            _selfPanel = gameObject.transform.Find("Viewport/CardGraveyard").gameObject;
            _reportGroup = _selfPanel.GetComponent<VerticalLayoutGroup>();

            PlayedCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");

            _actionsQueueController.GotNewActionReportEvent += GotNewActionReportEventHandler;

            _reportGroup.padding.top = 0;
        }

        public void Dispose()
        {
            _actionsQueueController.GotNewActionReportEvent -= GotNewActionReportEventHandler;
        }

        public void Clear()
        {
            foreach (ReportViewBase item in _allReports)
            {
                item.Dispose();
            }

            _allReports.Clear();

            _reportGroup.padding.top = 0;
        }

        private void GotNewActionReportEventHandler(GameActionReport report)
        {
            ReportViewBase reportView = null;
            switch (report.ActionType)
            {
                case Enumerators.ActionType.AttackPlayerByCreature:
                    reportView = new ReportViewBaseAttackPlayerByCreature(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.AttackCreatureByCreature:
                    reportView = new ReportViewBaseAttackCreatureByCreature(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.AttackCreatureBySkill:
                    reportView = new GameplayActionReportAttackCreatureBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.AttackPlayerBySkill:
                    reportView = new GameplayActionReportAttackPlayerBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HealPlayerBySkill:
                    reportView = new GameplayActionReportHealPlayerBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HealCreatureBySkill:
                    reportView = new GameplayActionReportHealCreatureBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.AttackCreatureByAbility:
                    reportView = new GameplayActionReportAttackCreatureByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.AttackPlayerByAbility:
                    reportView = new GameplayActionReportAttackPlayerByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HealPlayerByAbility:
                    reportView = new GameplayActionReportHealPlayerByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HealCreatureByAbility:
                    reportView = new GameplayActionReportHealCreatureByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PlayUnitCard:
                    reportView = new GameplayActionReportPlayUnitCard(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PlaySpellCard:
                    reportView = new GameplayActionReportPlaySpellCard(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.StunCreatureByAbility:
                    reportView = new GameplayActionReportStunCreatureByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.StunUnitBySkill:
                    reportView = new GameplayActionReportStunCreatureBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.SummonUnitCard:
                    reportView = new GameplayActionReportPlayUnitCard(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
            }

            if (reportView != null)
            {
                _allReports.Add(reportView);
            }
        }
    }
}
