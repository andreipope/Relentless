using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class ReportPanelItem
    {
        private readonly GameObject _playedCardPrefab;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly GameObject _selfPanel;

        private readonly VerticalLayoutGroup _reportGroup;

        private readonly List<ReportViewBase> _allReports;

        public ReportPanelItem()
        {
        }

        public ReportPanelItem(GameObject gameObject)
        {
            ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();

            _actionsQueueController = gameplayManager.GetController<ActionsQueueController>();

            _allReports = new List<ReportViewBase>();

            _selfPanel = gameObject.transform.Find("Viewport/CardGraveyard").gameObject;
            _reportGroup = _selfPanel.GetComponent<VerticalLayoutGroup>();

            _playedCardPrefab =
                loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");

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
                    reportView =
                        new ReportViewBaseAttackPlayerByCreature(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_CREATURE:
                    reportView =
                        new ReportViewBaseAttackCreatureByCreature(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL:
                    reportView =
                        new GameplayActionReportAttackCreatureBySkill(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL:
                    reportView =
                        new GameplayActionReportAttackPlayerBySkill(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_SKILL:
                    reportView =
                        new GameplayActionReportHealPlayerBySkill(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_SKILL:
                    reportView =
                        new GameplayActionReportHealCreatureBySkill(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY:
                    reportView =
                        new GameplayActionReportAttackCreatureByAbility(_playedCardPrefab, _selfPanel.transform,
                            report);
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_ABILITY:
                    reportView =
                        new GameplayActionReportAttackPlayerByAbility(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_ABILITY:
                    reportView =
                        new GameplayActionReportHealPlayerByAbility(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_ABILITY:
                    reportView =
                        new GameplayActionReportHealCreatureByAbility(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PLAY_UNIT_CARD:
                    reportView = new GameplayActionReportPlayUnitCard(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PLAY_SPELL_CARD:
                    reportView = new GameplayActionReportPlaySpellCard(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.STUN_CREATURE_BY_ABILITY:
                    reportView =
                        new GameplayActionReportStunCreatureByAbility(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.STUN_UNIT_BY_SKILL:
                    reportView =
                        new GameplayActionReportStunCreatureBySkill(_playedCardPrefab, _selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.SUMMON_UNIT_CARD:
                    reportView = new GameplayActionReportPlayUnitCard(_playedCardPrefab, _selfPanel.transform, report);
                    break;
            }

            if (reportView != null)
            {
                _allReports.Add(reportView);
            }
        }
    }
}
