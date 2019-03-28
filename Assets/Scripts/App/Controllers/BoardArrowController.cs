using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
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
            ResetCurrentBoardArrow();
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
                IsBoardArrowNowInTheBattle = false;
            }
        }


        public T DoAutoTargetingArrowFromTo<T>(Transform from, BoardObject to, float delayTillDestroyArrow = 1f,
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

            InternalTools.DoActionDelayed(() =>
            {
                arrow.Dispose();
                if (arrow?.SelfObject != null)
                {
                    UnityEngine.Object.Destroy(arrow.gameObject);
                }
                action?.Invoke();
            }, delayTillDestroyArrow);

            return arrow;
        }

        public T BeginTargetingArrowFrom<T>(Transform from) where T : BoardArrow
        {
            T arrow = UnityEngine.Object.Instantiate(_boardArrowPrefab).AddComponent<T>();
            arrow.Begin(from.position);

            return arrow;
        }



        public void ActivateBoardArrow(Transform from, BoardUnitModel Owner, List<Enumerators.SkillTarget> TargetsType, IReadOnlyList<BoardUnitModel> boardCards)
        {
            BattleBoardArrow fightTargetingArrow = BeginTargetingArrowFrom<BattleBoardArrow>(from);
            fightTargetingArrow.TargetsType = TargetsType;
            fightTargetingArrow.BoardCards = boardCards;
            fightTargetingArrow.Owner = Owner;
        }


        #region AbilityArrow

        protected Action OnObjectSelectedByTargettingArrowCallback;

        protected Action OnObjectSelectFailedByTargettingArrowCallback;

        public void ActivateAbilityArrow(Transform ownerTransform,
            List<Enumerators.SkillTarget> targetsType = null, Action callback = null, Action failedCallback = null)
        {
            OnObjectSelectedByTargettingArrowCallback = callback;
            OnObjectSelectFailedByTargettingArrowCallback = failedCallback;

            AbilityBoardArrow abilityTargettingArrow = BeginTargetingArrowFrom<AbilityBoardArrow>(ownerTransform);
            //_fightTargetingArrow.TargetsType = Model.AttackTargetsAvailability;
            //_fightTargetingArrow.BoardCards = _gameplayManager.OpponentPlayer.CardsOnBoard;
            //_fightTargetingArrow.Owner = this.Model;


            //TargettingArrow.PossibleTargets = AbilityTargets;
            //TargettingArrow.TargetUnitType = TargetCardType;
            //TargettingArrow.TargetUnitSpecialStatusType = TargetUnitSpecialStatus;
            //TargettingArrow.UnitDefense = AbilityData.Defense2;
            //TargettingArrow.UnitCost = AbilityData.Cost;

            // switch (CardKind)
            // {
            //     case Enumerators.CardKind.CREATURE:

            //         BoardUnitView abilityUnitOwnerView = GetAbilityUnitOwnerView();
            //         TargettingArrow.SelfBoardCreature = abilityUnitOwnerView;
            //         TargettingArrow.Begin(abilityUnitOwnerView.Transform.position);
            //         break;
            //     case Enumerators.CardKind.ITEM:
            //         TargettingArrow.Begin(SelectedPlayer.AvatarObject.transform.position);
            //         break;
            //     default:
            //         TargettingArrow.Begin(PlayerCallerOfAbility.AvatarObject.transform.position);
            //         break;
            // }

            //TargettingArrow.CardSelected += CardSelectedHandler;
            //TargettingArrow.CardUnselected += CardUnselectedHandler;
            //TargettingArrow.PlayerSelected += PlayerSelectedHandler;
            //TargettingArrow.PlayerUnselected += PlayerUnselectedHandler;
            abilityTargettingArrow.InputEnded += InputEndedHandler;
            abilityTargettingArrow.InputCanceled += InputCanceledHandler;

             //AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);*/
        }

        protected virtual void InputEndedHandler()
        {
            //SelectedTargetAction();
            DeactivateSelectTarget();
        }

        protected virtual void InputCanceledHandler()
        {
            OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
            OnObjectSelectFailedByTargettingArrowCallback = null;

            DeactivateSelectTarget();
        }

        public void DeactivateSelectTarget()
        {
            if (CurrentBoardArrow != null)
            {
                //TargettingArrow.CardSelected -= CardSelectedHandler;
                //TargettingArrow.CardUnselected -= CardUnselectedHandler;
                //TargettingArrow.PlayerSelected -= PlayerSelectedHandler;
                //TargettingArrow.PlayerUnselected -= PlayerUnselectedHandler;
                (CurrentBoardArrow as AbilityBoardArrow).InputEnded -= InputEndedHandler;
                (CurrentBoardArrow as AbilityBoardArrow).InputCanceled -= InputCanceledHandler;

                ResetCurrentBoardArrow();
            }
        }
        #endregion
    }
}
