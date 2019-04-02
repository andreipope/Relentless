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

        public void ShowBattleBoardArrow(BoardUnitModel owner, Transform from, List<Enumerators.SkillTarget> TargetsType, IReadOnlyList<BoardUnitModel> CardsOnBoard)
        {
            BattleBoardArrow battleArrow = BeginTargetingArrowFrom<BattleBoardArrow>(from);
            battleArrow.TargetsType = TargetsType;
            battleArrow.BoardCards = CardsOnBoard;
            battleArrow.Owner = owner;

            CurrentBoardArrow = battleArrow;
        }

        public void ShowAbilityBoardArrow(BoardUnitModel owner, Transform from, Enumerators.CardKind cardKind,
            List<Enumerators.SkillTarget> targetsType = null, Action callback = null, Action failedCallback = null)
        {
            AbilityBoardArrow abilityArrow = BeginTargetingArrowFrom<AbilityBoardArrow>(from);

            //abilityArrow.PossibleTargets = AbilityTargets;
            //abilityArrow.TargetUnitType = TargetCardType;
            //abilityArrow.TargetUnitSpecialStatusType = TargetUnitSpecialStatus;
            //abilityArrow.UnitDefense = AbilityData.Defense2;
            //abilityArrow.UnitCost = AbilityData.Cost;

            switch (cardKind)
            {
                case Enumerators.CardKind.CREATURE:

                    //BoardUnitView abilityUnitOwnerView = GetAbilityUnitOwnerView();
                    //abilityArrow.SelfBoardCreature = abilityUnitOwnerView;
                    //abilityArrow.Begin(abilityUnitOwnerView.Transform.position);
                    break;
                case Enumerators.CardKind.ITEM:
                    //abilityArrow.Begin(SelectedPlayer.AvatarObject.transform.position);
                    break;
                default:
                    //abilityArrow.Begin(PlayerCallerOfAbility.AvatarObject.transform.position);
                    break;
            }

            //abilityArrow.CardSelected += CardSelectedHandler;
            //abilityArrow.CardUnselected += CardUnselectedHandler;
            //abilityArrow.PlayerSelected += PlayerSelectedHandler;
            //abilityArrow.PlayerUnselected += PlayerUnselectedHandler;
            abilityArrow.InputEnded += InputEndedHandler;
            abilityArrow.InputCanceled += InputCanceledHandler;

            ///AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

        }


        public void DeactivateSelectTarget()
        {
            //TargettingArrow.CardSelected -= CardSelectedHandler;
            //TargettingArrow.CardUnselected -= CardUnselectedHandler;
            //TargettingArrow.PlayerSelected -= PlayerSelectedHandler;
            //TargettingArrow.PlayerUnselected -= PlayerUnselectedHandler;
            //TargettingArrow.InputEnded -= InputEndedHandler;
            //TargettingArrow.InputCanceled -= InputCanceledHandler;

            ResetCurrentBoardArrow();
        }


        public virtual void SelectedTargetAction(bool callInputEndBefore = false)
        {
            //if (callInputEndBefore)
            //{
            //    PermanentInputEndEvent?.Invoke();
            //    return;
            //}

            //if (TargetUnit != null)
            //{
            //    AffectObjectType = Enumerators.AffectObjectType.Character;
            //}
            //else if (TargetPlayer != null)
            //{
            //    AffectObjectType = Enumerators.AffectObjectType.Player;
            //}
            //else
            //{
            //    AffectObjectType = Enumerators.AffectObjectType.None;
            //}

            //if (AffectObjectType != Enumerators.AffectObjectType.None)
            //{
            //    IsAbilityResolved = true;

            //    OnObjectSelectedByTargettingArrowCallback?.Invoke();
            //    OnObjectSelectedByTargettingArrowCallback = null;
            //}
            //else
            //{
            //    OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
            //    OnObjectSelectFailedByTargettingArrowCallback = null;
            //}
        }

        protected virtual void InputEndedHandler()
        {
            SelectedTargetAction();
            DeactivateSelectTarget();
        }

        protected virtual void InputCanceledHandler()
        {
            //OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
            //OnObjectSelectFailedByTargettingArrowCallback = null;

            DeactivateSelectTarget();
        }
    }
}
