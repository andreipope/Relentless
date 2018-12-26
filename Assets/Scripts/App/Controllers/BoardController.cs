using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardController : IController
    {
        private IPlayerManager _playerManager;

        private ISoundManager _soundManager;

        private ITimerManager _timerManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private IPvPManager _pvpManager;

        private IMatchManager _matchManager;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private BattlegroundController _battlegroundController;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _pvpManager = GameClient.Get<IPvPManager>();
            _matchManager = GameClient.Get<IMatchManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
        }

        public void ResetAll()
        {
        }

        public void Update()
        {
        }

        public void UpdateWholeBoard(Action boardUpdated)
        {
            int incrementValue = 0;
            int maxValue = 2;

            UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer, () =>
            {
                CheckIfBoardWasUpdated(ref incrementValue, maxValue, boardUpdated);
            });

            UpdateCurrentBoardOfPlayer(_gameplayManager.OpponentPlayer, () =>
            {
                CheckIfBoardWasUpdated(ref incrementValue, maxValue, boardUpdated);
            });
        }

        public void UpdateCurrentBoardOfPlayer(Player player, Action boardUpdated)
        {
            UpdateBoard(player.BoardCards, player.IsLocalPlayer, boardUpdated);
        }

        public void UpdateBoard(List<BoardUnitView> units, bool isBottom, Action boardUpdated)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            float boardWidth = 0.0f;
            float spacing = 0.2f;
            float cardWidth = 0.0f;

            for (int i = 0; i < units.Count; i++)
            {
                cardWidth = 2.5f;
                boardWidth += cardWidth;
                boardWidth += spacing;
            }

            boardWidth -= spacing;

            List<Vector2> newPositions = new List<Vector2>(units.Count);

            Vector3 pivot = isBottom ? _battlegroundController.PlayerBoardObject.transform.position :
                                       _battlegroundController.OpponentBoardObject.transform.position;

            for (int i = 0; i < units.Count; i++)
            {
                newPositions.Add(new Vector2(pivot.x - boardWidth / 2 + cardWidth / 2, pivot.y + (isBottom ? -1.7f : 0f)));
                pivot.x += boardWidth / units.Count;
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < units.Count; i++)
            {
                BoardUnitView card = units[i];

                if (card.Model.IsDead)
                    continue;

                card.PositionOfBoard = newPositions[i];
                sequence.Insert(0, card.Transform.DOMove(newPositions[i], 0.4f).SetEase(Ease.OutSine));
            }

            sequence.AppendCallback(() => boardUpdated?.Invoke());
        }

        private void CheckIfBoardWasUpdated(ref int value, int maxValue, Action endCallback)
        {
            value++;

            if (value == maxValue)
            {
                endCallback?.Invoke();
            }
        }
    }
}
