using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class BoardArrowController : IController
    {
        private ITimerManager _timerManager;
        private ILoadObjectsManager _loadObjectsManager;

        private GameObject _boardArrowPrefab;

        public BoardArrow CurrentBoardArrow { get; set; }

        public bool IsBoardArrowNowInTheBattle { get; set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            _timerManager = GameClient.Get<ITimerManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _boardArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void SetStatusOfBoardArrowOnBoard(bool status)
        {
            IsBoardArrowNowInTheBattle = status;
        }

        public void ResetCurrentBoardArrow()
        {
            if (CurrentBoardArrow != null)
            {
                CurrentBoardArrow.Dispose();
                CurrentBoardArrow = null;
            }
        }


        public T DoAutoTargetingArrowFromTo<T>(Transform from, object to, float delayTillDestroyArrow = 1f,
                                               Action action = null, bool isManuallyDoAction = false) where T : BoardArrow
        {
            if (isManuallyDoAction)
            {
                action?.Invoke();
                return null;
            }

            T arrow = UnityEngine.Object.Instantiate(_boardArrowPrefab).AddComponent<T>();
            arrow.Begin(from.position);
            arrow.SetTarget(to);

            _timerManager.AddTimer((x) =>
            {
                arrow.Dispose();
                UnityEngine.Object.Destroy(arrow.gameObject);
                action?.Invoke();
            }, null, delayTillDestroyArrow);

            return arrow;
        }

        public T BeginTargetingArrowFrom<T>(Transform from) where T : BoardArrow
        {
            T arrow = UnityEngine.Object.Instantiate(_boardArrowPrefab).AddComponent<T>();
            arrow.Begin(from.position);

            return arrow;
        }
    }
}
