using System;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class AnimationsController : IController
    {
        private IGameplayManager _gameplayManager;

        private ITimerManager _timerManager;

        private BattlegroundController _battlegroundController;

        private BoardController _boardController;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _boardController = _gameplayManager.GetController<BoardController>();
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void DoFightAnimation(
            BoardUnitView boardUnitView,
            GameObject source,
            GameObject target,
            float shakeStrength,
            Action onHitCallback,
            Action onCompleteCallback = null,
            bool isCreatureAttacker = true,
            float duration = 0.5f)
        {
            Vector3 originalPos = source.transform.position;

            SortingGroup sortingGroup = source.GetComponent<SortingGroup>();
            int oldSortingOrder = sortingGroup.sortingOrder;
            string oldsortingLayerName = sortingGroup.sortingLayerName;

            sortingGroup.sortingLayerID = SRSortingLayers.BoardCards;
            sortingGroup.sortingOrder = 1000;

            Vector3 partWay;

            if (isCreatureAttacker)
            {
                partWay = Vector3.Lerp(originalPos + Vector3.back * 5f, target.transform.position + Vector3.back * 5f, Constants.DurationUnitAttacking);
            }
            else
            {
                partWay = Vector3.Lerp(originalPos + Vector3.back * 5f, target.transform.position + Vector3.back * 5f, Constants.DurationUnitAttacking);
            }

            source.transform.DOMove(partWay, Constants.DurationEndUnitAttacking).SetEase(Ease.InSine).OnComplete(
                () =>
                {
                    target.transform.DOShakePosition(1, new Vector3(shakeStrength, shakeStrength, 0));

                    Vector2 correctEndPosition = _boardController.GetCorrectPositionOfUnitOnBoard(boardUnitView.Model.Owner, boardUnitView);

                    if(correctEndPosition == Vector2.zero)
                    {
                        correctEndPosition = originalPos;
                    }

                    source.transform.DOMove(correctEndPosition, duration).SetEase(Ease.OutSine).OnComplete(
                        () =>
                        {
                            onCompleteCallback?.Invoke();

                            sortingGroup.sortingOrder = oldSortingOrder;
                            sortingGroup.sortingLayerName = oldsortingLayerName;
                        });

                    onHitCallback?.Invoke();
                });
        }

        public void MoveCardFromPlayerDeckToPlayerHandAnimation(Player fromDeck, Player toHand, BoardCardView boardCardView)
        {
            boardCardView.DrawCardFromOpponentDeckToPlayer();
        }

        public void MoveCardFromPlayerDeckToOpponentHandAnimation(Player fromDeck, Player toHand, OpponentHandCard boardCardView)
        {
            Animator animator = boardCardView.GameObject.GetComponent<Animator>();

            boardCardView.Transform.localScale = Vector3.zero;
            boardCardView.Transform.DOScale(new Vector3(0.9f, 0.9f, 0.9f), 0.15f);

            animator.enabled = true;
            animator.StopPlayback();
            animator.Play("MoveCardFromPlayerDeckToOpponentHand");

            _timerManager.AddTimer(
                x =>
                {
                    animator.enabled = false;

                    _battlegroundController.OpponentHandCards.Insert(ItemPosition.End, boardCardView);

                    _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
                },
                null,
                1.1f);
        }
    }
}
