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

        private long _sequenceUniqueId = 0;

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
            _sequenceUniqueId = 0;
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
            UpdateBoard(_battlegroundController.GetCardViewsByModels<BoardUnitView>(player.PlayerCardsController.CardsOnBoard), player.IsLocalPlayer, boardUpdated);
        }

        public void UpdateBoard(IReadOnlyList<BoardUnitView> units, bool isBottom, Action boardUpdated, int skipIndex = -1)
        {
            const float Duration = 0.4f;

            if (_gameplayManager.IsGameEnded || units == null)
                return;

            List<UnitPositionOnBoard> newPositions = GetPositionsForUnits(units, isBottom);

            _sequenceUniqueId++;

            BoardUpdateSequence updateSequence = new BoardUpdateSequence();
            updateSequence.Id = _sequenceUniqueId;

            Tween tween;

            for (int i = 0; i < newPositions.Count; i++)
            {
                updateSequence.AddTween(null, Duration);
            }

            for (int i = 0; i < updateSequence.Tweens.Count; i++)
            {
                if (i != skipIndex)
                {
                    BoardUnitView card = newPositions[i].BoardUnitView;

                    card.PositionOfBoard = newPositions[i].Position;

                    tween = card.Transform.DOMove(newPositions[i].Position, Duration).SetEase(Ease.OutSine);
                    tween.OnComplete(() =>
                    {
                        updateSequence.TweenEnded(tween);
                    });

                    updateSequence.Tweens[i].Tween = tween;
                    updateSequence.StartTween(updateSequence.Tweens[i]);
                }
            }

            if (boardUpdated != null)
            {
                updateSequence.TweensEnded += boardUpdated;

                // update board timeout
                InternalTools.DoActionDelayed(() =>
                {
                    if (!updateSequence.SequenceDone)
                    {
                        updateSequence.SequenceDone = true;
                        boardUpdated?.Invoke();
                    }
                }, Duration * 2f);
            }
        }

        public Vector2 GetCorrectPositionOfUnitOnBoard(Player player, BoardUnitView boardUnitView)
        {
            UnitPositionOnBoard unitPositionOnBoard =
                GetPositionsForUnits(_battlegroundController.GetCardViewsByModels<BoardUnitView>(player.PlayerCardsController.CardsOnBoard),
                                     player.IsLocalPlayer).Find(item => item.BoardUnitView == boardUnitView);

            if (unitPositionOnBoard != null)
            {
                if(!player.IsLocalPlayer)
                {
                    unitPositionOnBoard.Position.x *= -1;
                }

                return unitPositionOnBoard.Position;
            }

            return Vector2.zero;
        }

        private List<UnitPositionOnBoard> GetPositionsForUnits(IReadOnlyList<BoardUnitView> units, bool isBottom)
        {
            // FIXME HARD: in tutorial, arrows should NEVER use XYZ coordinates, and use references to actual things instead.
            if (!isBottom)
            {
                units = units.Reverse().ToList();
            }

            float boardWidth = 0.0f;
            const float spacing = 0.2f;
            const float cardWidth = 2.5f;

            for (int i = 0; i < units.Count; i++)
            {
                boardWidth += cardWidth;
                boardWidth += spacing;
            }

            boardWidth -= spacing;

            List<UnitPositionOnBoard> newPositions = new List<UnitPositionOnBoard>(units.Count);

            Vector3 pivot = isBottom ? _battlegroundController.PlayerBoardObject.transform.position :
                                       _battlegroundController.OpponentBoardObject.transform.position;

            UnitPositionOnBoard unitPositionOnBoard;
            for (int i = 0; i < units.Count; i++)
            {
                unitPositionOnBoard = new UnitPositionOnBoard()
                {
                    BoardUnitView = units[i],
                    Position = new Vector2(pivot.x - boardWidth / 2 + cardWidth / 2, pivot.y + (isBottom ? -1.7f : 0f))
                };
                pivot.x += boardWidth / units.Count;
                newPositions.Add(unitPositionOnBoard);
            }

            return newPositions;
        }

        private void CheckIfBoardWasUpdated(ref int value, int maxValue, Action endCallback)
        {
            value++;

            if (value == maxValue)
            {
                endCallback?.Invoke();
            }
        }

        public class BoardUpdateSequence
        {
            public event Action TweensEnded;

            public long Id;

            public List<TweenObject> Tweens;

            public bool SequenceDone;

            public BoardUpdateSequence()
            {
                Tweens = new List<TweenObject>();
                SequenceDone = false;
            }

            public void AddTween(Tween tween, float duration)
            {
                TweenObject tweenObject = new TweenObject();
                tweenObject.Tween = tween;
                tweenObject.IsDone = false;
                tweenObject.timeout = duration * 1.25f;

                Tweens.Add(tweenObject);
            }

            public void StartTween(TweenObject tweenObject)
            {
                InternalTools.DoActionDelayed(() =>
                {
                    TweenEnded(tweenObject.Tween);
                }, tweenObject.timeout);
            }

            public void TweenEnded(Tween tween)
            {
                if (SequenceDone)
                    return;

                TweenObject tweenObject = Tweens.Find(tweenElement => tweenElement.Tween == tween);
                tweenObject.IsDone = true;

                if (Tweens.FindAll(tweenElement => tweenElement.IsDone).Count >= Tweens.Count)
                {
                    SequenceDone = true;
                    TweensEnded?.Invoke();
                }
            }

            public class TweenObject
            {
                public Tween Tween;
                public bool IsDone;
                public float timeout;
            }
        }

        public class UnitPositionOnBoard
        {
            public BoardUnitView BoardUnitView;
            public Vector2 Position;
        }
    }
}
