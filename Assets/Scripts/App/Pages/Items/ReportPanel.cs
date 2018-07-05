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
        private CardsController cardsController;
        private ActionsQueueController _actionsQueueController;

        private GameObject selfPanel;
        private VerticalLayoutGroup _reportGroup;

        public GameObject playedCardPrefab;

        private List<ReportViewBase> _allReports;

        private float _graveYardTopOffset;

        public ReportPanelItem() { }

        public ReportPanelItem(GameObject gameObject)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            cardsController = _gameplayManager.GetController<CardsController>();

            _allReports = new List<ReportViewBase>();

            selfPanel = gameObject.transform.Find("Viewport/CardGraveyard").gameObject;
            _reportGroup = selfPanel.GetComponent<VerticalLayoutGroup>();

            playedCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");

            _actionsQueueController.GotNewActionReportEvent += GotNewActionReportEventHandler;
        }

        public void Clear()
        {
            foreach (var item in _allReports)
                item.Dispose();
            _allReports.Clear();
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
                case Enumerators.ActionType.ATTACK_CREATURE_BY_SPELL:
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_SPELL:
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL:
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL:
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_SKILL:
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_SKILL:
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY:
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_ABILITY:
                    break;
                case Enumerators.ActionType.HEAL_PLAYER_BY_ABILITY:
                    break;
                case Enumerators.ActionType.HEAL_CREATURE_BY_ABILITY:
                    break;
                case Enumerators.ActionType.ATTACK_PLAYER_BY_WEAPON:
                    break;
                case Enumerators.ActionType.ATTACK_CREATURE_BY_WEAPON:
                    break;
                default:
                    break;
            }
            if (reportView != null)
                _allReports.Add(reportView);


            //if (_allReports.Count > 4)
            //{
            //    _graveYardTopOffset = -66 - 120 * (_allReports.Count - 5);
            //}

            //if (_reportGroup.padding.top > _graveYardTopOffset)
            //{
            //    float offset = Mathf.Lerp((float)_reportGroup.padding.top, (float)_graveYardTopOffset, Time.deltaTime * 2);
            //    _reportGroup.padding = new RectOffset(0, 0, Mathf.FloorToInt(offset), 0);
            //}
            for (int j = _allReports.Count - 1; j >= 0; j--)
                _allReports[j].selfObject.transform.SetAsLastSibling();        }
    }
}
