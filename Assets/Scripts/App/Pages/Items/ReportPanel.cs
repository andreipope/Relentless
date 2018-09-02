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
                case Enumerators.ActionType.ATTACK_PLAYER_BY_CREATURE:
                    reportView = new ReportViewBaseAttackPlayerByCreature(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_CREATURE:
                    reportView = new ReportViewBaseAttackCreatureByCreature(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL:
                    reportView = new GameplayActionReportAttackCreatureBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL:
                    reportView = new GameplayActionReportAttackPlayerBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_SKILL:
                    reportView = new GameplayActionReportHealPlayerBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_SKILL:
                    reportView = new GameplayActionReportHealCreatureBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY:
                    reportView = new GameplayActionReportAttackCreatureByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_ABILITY:
                    reportView = new GameplayActionReportAttackPlayerByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_ABILITY:
                    reportView = new GameplayActionReportHealPlayerByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_ABILITY:
                    reportView = new GameplayActionReportHealCreatureByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PLAY_UNIT_CARD:
                    reportView = new GameplayActionReportPlayUnitCard(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PLAY_SPELL_CARD:
                    reportView = new GameplayActionReportPlaySpellCard(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.STUN_CREATURE_BY_ABILITY:
                    reportView = new GameplayActionReportStunCreatureByAbility(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.STUN_UNIT_BY_SKILL:
                    reportView = new GameplayActionReportStunCreatureBySkill(PlayedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.SUMMON_UNIT_CARD:
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
