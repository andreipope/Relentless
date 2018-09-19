using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GeneralAbility : IDisposable
    {
        private List<NewAbilityData> _abilityDatas;

        private List<IAbility> _entryTriggerAbility;
        private List<IAbility> _endTurnTriggerAbility;
        private List<IAbility> _startTurnTriggerAbility;
        private List<IAbility> _deathTriggerAbility;
        private List<IAbility> _delayTriggerAbility;
        private List<IAbility> _unitAttackedTriggerAbility;
        private List<IAbility> _unitDamagedTriggerAbility;
        private List<IAbility> _unitHealthChangedTriggerAbility;

        public int Id;

        public ILoadObjectsManager LoadObjectsManager;
        public IGameplayManager GameplayManager;
        public IDataManager DataManager;
        public ITimerManager TimerManager;
        public ISoundManager SoundManager;

        public BattlegroundController BattlegroundController;
        public BoardArrowController BoardArrowController;

        public Player PlayerOwner;
        public BoardObject BoardObjectOwner;

        public BoardObject SelectedTarget { get; private set; }

        public GeneralAbility(BoardObject boardObjectOwner, List<NewAbilityData> abilityDatas)
        {
            BoardObjectOwner = boardObjectOwner;
            PlayerOwner = boardObjectOwner.OwnerPlayer;

            _abilityDatas = abilityDatas;

            GameplayManager = GameClient.Get<IGameplayManager>();
            DataManager = GameClient.Get<IDataManager>();
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            TimerManager = GameClient.Get<ITimerManager>();
            SoundManager = GameClient.Get<ISoundManager>();

            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            BoardArrowController = GameplayManager.GetController<BoardArrowController>();

            PlayerOwner.TurnEnded += TurnEndedHandler;
            PlayerOwner.TurnStarted += TurnStartedHandler;

            if (BoardObjectOwner is BoardUnit unit)
            {
                unit.UnitDied += UnitDeadHandler;
                unit.UnitHpChanged += UnitHealthChangedHandler;
            }
            else if (BoardObjectOwner is BoardSpell spell)
            {
                spell.Used += SpellUsedHandler;
            }

            SetupBunchOfAbilities();

            CheckEntryAbilities();
        }

        public void Dispose()
        {
            PlayerOwner.TurnEnded -= TurnEndedHandler;
            PlayerOwner.TurnStarted -= TurnStartedHandler;

            if (BoardObjectOwner is BoardUnit unit)
            {
                unit.UnitDied -= UnitDeadHandler;
                unit.UnitHpChanged -= UnitHealthChangedHandler;
            }
            else if (BoardObjectOwner is BoardSpell spell)
            {
                spell.Used -= SpellUsedHandler;
            }
        }

        public void Update()
        {

        }

        public void SetupBunchOfAbilities()
        {
            _entryTriggerAbility = new List<IAbility>();
            _endTurnTriggerAbility = new List<IAbility>();
            _startTurnTriggerAbility = new List<IAbility>();
            _deathTriggerAbility = new List<IAbility>();
            _delayTriggerAbility = new List<IAbility>();
            _unitAttackedTriggerAbility = new List<IAbility>();
            _unitDamagedTriggerAbility = new List<IAbility>();
            _unitHealthChangedTriggerAbility = new List<IAbility>();

            IAbility ability = null;
            foreach (NewAbilityData abilityData in _abilityDatas)
            {
                switch (abilityData.Type)
                {
                    default: break;
                }

                if (ability == null)
                    continue;

                ability.Init(abilityData, BoardObjectOwner);
            }
        }

        public void CheckEntryAbilities()
        {
            bool foundTargeting = false;

            foreach(IAbility ability in _entryTriggerAbility)
            {
                if (ability.AbilityData.PossibleTargets != AbilityEnumerator.AbilityPossibleTargets.NONE)
                {
                    DoSelectTarget(ability.AbilityData);

                    foundTargeting = true;

                    break;
                }
            }

            if (!foundTargeting)
                EntryHandler();
        }

        public void DoSelectTarget(NewAbilityData abilityData)
        {
            Transform from = PlayerOwner.AvatarObject.transform;

            if (BoardObjectOwner is BoardUnit unit)
                from = unit.Transform;                                          
        }

        #region actions handlers
        private void TurnEndedHandler()
        {
            foreach (IAbility ability in _endTurnTriggerAbility)
                ability.CallAction();
        }

        private void TurnStartedHandler()
        {
            foreach (IAbility ability in _startTurnTriggerAbility)
                ability.CallAction();
        }

        private void EntryHandler(BoardObject target = null)
        {
            foreach (IAbility ability in _entryTriggerAbility)
                ability.CallAction(target);
        }

        private void DelayDoneHandler()
        {
            foreach (IAbility ability in _delayTriggerAbility)
                ability.CallAction();
        }

        private void UnitDamagedHandler(BoardObject attacker)
        {
            foreach (IAbility ability in _unitDamagedTriggerAbility)
                ability.CallAction();
        }

        private void UnitAttackedHandler(BoardObject target, int damage, bool isAttacker)
        {
            foreach (IAbility ability in _unitAttackedTriggerAbility)
                ability.CallAction();
        }

        private void UnitDeadHandler()
        {
            foreach (IAbility ability in _deathTriggerAbility)
                ability.CallAction();

            Dispose();
        }

        private void UnitHealthChangedHandler()
        {
            foreach (IAbility ability in _unitHealthChangedTriggerAbility)
                ability.CallAction();
        }

        private void SpellUsedHandler()
        {
            Dispose();
        }

        #endregion

        #region targetting handlers

        private void UnitSelectedHandler(BoardUnit unit)
        {
            SelectedTarget = unit;
        }

        private void UnitUnselectedHandler(BoardUnit unit)
        {
            if (SelectedTarget == unit)
            {
                SelectedTarget = unit;
            }
        }

        private void PlayerUnselectedHandler(Player player)
        {
            if (SelectedTarget == player)
            {
                SelectedTarget = null;
            }
        }

        private void PlayerSelectedHandler(Player player)
        {
            SelectedTarget = player;
        }

        private void InputEndedHandler()
        {
            if (SelectedTarget != null)
                EntryHandler(SelectedTarget);

            BoardArrowController.CurrentBoardArrow.Dispose();
        }

        private void InputCanceledHandler()
        {
            SelectedTarget = null;

            BoardArrowController.CurrentBoardArrow.Dispose();
        }

        #endregion
    }
}
