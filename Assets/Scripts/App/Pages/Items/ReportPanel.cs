// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace LoomNetwork.CZB
{
    public class ReportPanelItem
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private ITimerManager _timerManager;

        private CardsController cardsController;
        private ActionsQueueController _actionsQueueController;

        private GameObject selfPanel;
        private VerticalLayoutGroup _reportGroup;

        public GameObject playedCardPrefab;

        private List<ReportViewBase> _allReports;

        public ReportPanelItem() { }

        public ReportPanelItem(GameObject gameObject)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            cardsController = _gameplayManager.GetController<CardsController>();

            _allReports = new List<ReportViewBase>();

            selfPanel = gameObject.transform.Find("Viewport/CardGraveyard").gameObject;
            _reportGroup = selfPanel.GetComponent<VerticalLayoutGroup>();

            playedCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");

            _actionsQueueController.GotNewActionReportEvent += GotNewActionReportEventHandler;

            _reportGroup.padding.top = 0;
        }

        public void Dispose()
        {
            _actionsQueueController.GotNewActionReportEvent -= GotNewActionReportEventHandler;
        }

        public void Clear()
        {
            foreach (var item in _allReports)
                item.Dispose();
            _allReports.Clear();

            _reportGroup.padding.top = 0;
        }

        private void GotNewActionReportEventHandler(GameActionReport report)
        {
            ReportViewBase reportView = null;
            switch (report.actionType)
            {
                case Enumerators.ActionType.ATTACK_PLAYER_BY_CREATURE:
                    reportView = new ReportViewBaseAttackPlayerByCreature(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_CREATURE:
                    reportView = new ReportViewBaseAttackCreatureByCreature(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL:
                    reportView = new GameplayActionReport_AttackCreatureBySkill(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL:
                    reportView = new GameplayActionReport_AttackPlayerBySkill(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_SKILL:
                    reportView = new GameplayActionReport_HealPlayerBySkill(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_SKILL:
                    reportView = new GameplayActionReport_HealCreatureBySkill(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY:
                    reportView = new GameplayActionReport_AttackCreatureByAbility(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_ABILITY:
                    reportView = new GameplayActionReport_AttackPlayerByAbility(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_ABILITY:
                    reportView = new GameplayActionReport_HealPlayerByAbility(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_ABILITY:
                    reportView = new GameplayActionReport_HealCreatureByAbility(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PLAY_UNIT_CARD:
                    reportView = new GameplayActionReport_PlayUnitCard(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.PLAY_SPELL_CARD:
                    reportView = new GameplayActionReport_PlaySpellCard(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.STUN_CREATURE_BY_ABILITY:
                    reportView = new GameplayActionReport_StunCreatureByAbility(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.STUN_UNIT_BY_SKILL:
                    reportView = new GameplayActionReport_StunCreatureBySkill(playedCardPrefab, selfPanel.transform, report);
                    break;
                case Enumerators.ActionType.SUMMON_UNIT_CARD:
                    reportView = new GameplayActionReport_PlayUnitCard(playedCardPrefab, selfPanel.transform, report);
                    break;
                default:
                    break;
            }
            if (reportView != null)
                _allReports.Add(reportView);
        }
    }
}