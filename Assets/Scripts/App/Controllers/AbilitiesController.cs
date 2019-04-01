using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class AbilitiesController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(AbilitiesController));

        private IGameplayManager _gameplayManager;

        private BattlegroundController _battlegroundController;

        private Dictionary<Enumerators.AbilityTrigger, List<ICardAbility>> _activeAbilities;

        public void Dispose()
        {
        }

        public void ResetAll()
        {
            UnsubscribeEvents();
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            _gameplayManager.GameInitialized += GameInitializedHandler;
            _gameplayManager.GameEnded += GameEndedHandler;
        }

        public void Update()
        {
        }

        private void GameInitializedHandler()
        {
            _activeAbilities = new Dictionary<Enumerators.AbilityTrigger, List<ICardAbility>>();

            for (int i = 1; i < Enum.GetNames(typeof(Enumerators.AbilityTrigger)).Length; i++)
            {
                _activeAbilities.Add((Enumerators.AbilityTrigger)i, new List<ICardAbility>());
            }

            SubscribeEvents();
        }

        private void GameEndedHandler(Enumerators.EndGameType obj)
        {
            foreach(KeyValuePair<Enumerators.AbilityTrigger, List<ICardAbility>> element in _activeAbilities)
            {
                foreach (ICardAbility ability in element.Value)
                {
                    ability.Dispose();
                }
            }

            _activeAbilities.Clear();

            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _battlegroundController.TurnStarted += TurnStartedHandler;
            _battlegroundController.TurnEnded += TurnEndedHandler;
            _battlegroundController.UnitDied += UnitDiedHandler;

            _gameplayManager.CurrentPlayer.CardPlayed += CardPlayedHandler;
            _gameplayManager.CurrentPlayer.PlayerCardsController.HandChanged += (count) =>
            {
                HandChangedHandler(_gameplayManager.CurrentPlayer);
            };
            _gameplayManager.CurrentPlayer.PlayerCardsController.BoardChanged += (count) =>
            {
                BoardChangedHandler(_gameplayManager.CurrentPlayer);
            };

            _gameplayManager.OpponentPlayer.CardPlayed += CardPlayedHandler;
            _gameplayManager.OpponentPlayer.PlayerCardsController.HandChanged += (count) =>
            {
                HandChangedHandler(_gameplayManager.OpponentPlayer);
            };
            _gameplayManager.OpponentPlayer.PlayerCardsController.BoardChanged += (count) =>
            {
                BoardChangedHandler(_gameplayManager.OpponentPlayer);
            };
        }

        private void UnsubscribeEvents()
        {
            _battlegroundController.TurnStarted -= TurnStartedHandler;
            _battlegroundController.TurnEnded -= TurnEndedHandler;
            _battlegroundController.UnitDied -= UnitDiedHandler;

            _gameplayManager.CurrentPlayer.CardPlayed -= CardPlayedHandler;
            _gameplayManager.CurrentPlayer.PlayerCardsController.HandChanged -= (count) =>
            {
                HandChangedHandler(_gameplayManager.CurrentPlayer);
            };
            _gameplayManager.CurrentPlayer.PlayerCardsController.BoardChanged -= (count) =>
            {
                BoardChangedHandler(_gameplayManager.CurrentPlayer);
            };

            _gameplayManager.OpponentPlayer.CardPlayed -= CardPlayedHandler;
            _gameplayManager.OpponentPlayer.PlayerCardsController.HandChanged -= (count) =>
            {
                HandChangedHandler(_gameplayManager.OpponentPlayer);
            };
            _gameplayManager.OpponentPlayer.PlayerCardsController.BoardChanged -= (count) =>
            {
                BoardChangedHandler(_gameplayManager.OpponentPlayer);
            };
        }

        public void InitializeAbilities(BoardUnitModel boardUnitModel, List<BoardObject> targets = null)
        {
            List<BoardObject> selectedTargets;
            foreach (CardAbilitiesCombination combination in boardUnitModel.Card.Prototype.Abilities.Combinations)
            {
                selectedTargets = targets;

                foreach (CardAbilityData cardAbilityData in combination.CardAbilities)
                {
                    if (cardAbilityData.Triggers.Contains(Enumerators.AbilityTrigger.Entry))
                    {
                        selectedTargets = FilterTargets(boardUnitModel, cardAbilityData.GenericParameters, GetTargets(boardUnitModel, cardAbilityData, targets));
                    }
                    else if (selectedTargets == null)
                    {
                        selectedTargets = new List<BoardObject>();
                    }

                    InitializeAbility(boardUnitModel, combination, cardAbilityData, selectedTargets);
                }
            }
        }

        public void EndAbility(ICardAbility cardAbility)
        {
            KeyValuePair<Enumerators.AbilityTrigger, List<ICardAbility>> keyValuePair =
                _activeAbilities.FirstOrDefault(element => element.Value.Contains(cardAbility));

            if(!keyValuePair.IsDefault())
            {
                keyValuePair.Value.Remove(cardAbility);
            }
        }

        public void TakeAbilityToUnit(BoardUnitModel boardUnitModel, CardAbilityData cardAbilityData)
        {
            InitializeAbility(boardUnitModel, null, cardAbilityData, new List<BoardObject>() { boardUnitModel });
        }

        public void ReactivateAbilitiesOnUnit(BoardUnitModel boardUnitModel)
        {
            foreach (CardAbilitiesCombination combination in boardUnitModel.Card.Prototype.Abilities.Combinations)
            {
                foreach (CardAbilityData cardAbilityData in combination.CardAbilities)
                {
                    InitializeAbility(boardUnitModel, combination, cardAbilityData, new List<BoardObject>(), true);
                }
            }
        }

        private void InitializeAbility(BoardUnitModel boardUnitModel,
                                       CardAbilitiesCombination combination,
                                       CardAbilityData cardAbilityData,
                                       List<BoardObject> targets,
                                       bool ignoreEntry = false)
        {
            if (cardAbilityData.Ability == Enumerators.AbilityType.Undefined)
                return;

            ICardAbility ability;
            ICardAbilityView abilityView = null;

            string abilityViewClassName, abilityClassName;

            foreach (Enumerators.AbilityTrigger trigger in cardAbilityData.Triggers)
            {
                if (ignoreEntry &&
                    (trigger == Enumerators.AbilityTrigger.Entry ||
                    trigger == Enumerators.AbilityTrigger.EntryWithSelection))
                    continue;

                abilityClassName = $"{cardAbilityData.Ability.ToString()}Ability";

                if (!InternalTools.IsTypeExists<ICardAbility>(abilityClassName))
                    continue;

                abilityViewClassName = $"{abilityClassName}View";

                if (InternalTools.IsTypeExists<ICardAbilityView>(abilityViewClassName))
                {
                    abilityView = InternalTools.GetInstance<ICardAbilityView>(abilityViewClassName);
                }

                ability = InternalTools.GetInstance<ICardAbility>(abilityClassName);
                ability.Init(boardUnitModel, combination, cardAbilityData, targets, abilityView);

                _activeAbilities[trigger].Add(ability);

                CheckOnEntryAbility(ability, cardAbilityData);
            }
        }

        #region event callers

        public void UnitStatChanged(Enumerators.Stat stat, BoardUnitModel boardUnit, int from, int to)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.StatChanged])
            {
                if (ability.UnitModelOwner == boardUnit)
                {
                    ability.InsertTargets(FilterTargets(ability.UnitModelOwner, ability.CardAbilityData.GenericParameters,
                                                        GetTargets(ability.UnitModelOwner, ability.CardAbilityData, new List<BoardObject>()
                    {
                        boardUnit
                    }, true)));
                    ability.DoAction();
                }
            }
        }

        public void UnitBeganAttack(BoardUnitModel attacker)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.Attack])
            {
                if (ability.UnitModelOwner == attacker)
                {
                    ability.InsertTargets(new List<BoardObject>()
                    {
                        attacker
                    });
                    ability.DoAction();
                }
            }
        }

        public void UnitAttacked(BoardUnitModel attacker, BoardObject targetAttacked, int damage)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.Attack])
            {
                if (ability.UnitModelOwner == attacker)
                {
                    ability.InsertTargets(FilterTargets(ability.UnitModelOwner, ability.CardAbilityData.GenericParameters,
                                                        GetTargets(ability.UnitModelOwner, ability.CardAbilityData, new List<BoardObject>()
                    {
                        targetAttacked
                    }, true)));
                    ability.DoAction(new List<GenericParameter>() { new GenericParameter(Enumerators.AbilityParameter.Damage, damage) });
                }
            }
        }

        public void UnitKilled(BoardUnitModel attacker, BoardObject targetKilled)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.KillUnit])
            {
                if (ability.UnitModelOwner == attacker)
                {
                    ability.InsertTargets(FilterTargets(ability.UnitModelOwner, ability.CardAbilityData.GenericParameters,
                                                         GetTargets(ability.UnitModelOwner, ability.CardAbilityData, new List<BoardObject>()
                     {
                        targetKilled
                     }, true)));
                    ability.DoAction();
                }
            }
        }

        public void UnitBeingBeAttacked(BoardObject unitAttacked)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.AtDefense])
            {
                if (ability.UnitModelOwner == unitAttacked)
                {
                    ability.InsertTargets(new List<BoardObject>()
                    {
                        unitAttacked
                    });
                    ability.DoAction();
                }
            }
        }

        public void UnitWasAttacked(BoardUnitModel attacker, BoardObject targetAttacked, int damage)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.AtDefense])
            {
                if (ability.UnitModelOwner == attacker)
                {
                    ability.InsertTargets(FilterTargets(ability.UnitModelOwner, ability.CardAbilityData.GenericParameters,
                                                        GetTargets(ability.UnitModelOwner, ability.CardAbilityData, new List<BoardObject>()
                    {
                        attacker
                    }, true)));
                    ability.DoAction(new List<GenericParameter>() { new GenericParameter(Enumerators.AbilityParameter.Damage, damage) });
                }
            }
        }

        public void CheckOnEntryAbility(ICardAbility cardAbility, CardAbilityData abilityData)
        {
            if(abilityData.Triggers.Contains(Enumerators.AbilityTrigger.Entry) ||
               abilityData.Triggers.Contains(Enumerators.AbilityTrigger.EntryWithSelection))
            {
                cardAbility.DoAction();
            }
        }

        #endregion


        #region event handlers

        private void UnitDiedHandler(BoardUnitModel boardUnit)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.Death])
            {
                if (ability.UnitModelOwner == boardUnit)
                {
                    ability.DoAction();
                }
            }

            foreach(ICardAbility ability in GetAbilitiesOnUnit(boardUnit))
            {
                EndAbility(ability);
            }
        }

        private void TurnEndedHandler()
        {
            foreach(ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.End])
            {
                ability.DoAction();
            }
        }

        private void TurnStartedHandler()
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.Turn])
            {
                ability.DoAction();
            }
        }

        private void HandChangedHandler(Player handOwner)
        {

        }

        private void BoardChangedHandler(Player boardOwner)
        {

        }

        private void CardPlayedHandler(BoardUnitModel boardUnit, int position)
        {

        }

        #endregion

        #region tools

        public List<ICardAbility> GetAbilitiesOnUnit(BoardUnitModel unitModel)
        {
            IEnumerable<List<ICardAbility>> abilities = _activeAbilities.Values.Select(item => item.FindAll(x => x.UnitModelOwner == unitModel));

            List<ICardAbility> filteredAbilities = new List<ICardAbility>();

            foreach (List<ICardAbility> list in abilities)
            {
                filteredAbilities.AddRange(list);
            }

            return filteredAbilities;
        }

        public bool HasEntryWithSelection(BoardUnitModel unitModel)
        {
            foreach (CardAbilitiesCombination combination in unitModel.Card.Prototype.Abilities.Combinations)
            {
                foreach (CardAbilityData data in combination.CardAbilities)
                {
                    foreach (Enumerators.AbilityTrigger trigger in data.Triggers)
                    {
                        if (trigger == Enumerators.AbilityTrigger.EntryWithSelection)
                            return true;
                    }
                }
            }

            return false;
        }

        #region targets filtering

        public List<BoardObject> GetTargets(BoardUnitModel modelCaller, CardAbilityData cardAbilityData, List<BoardObject> targets, bool insert = false)
        {
            if (targets != null && !insert)
                return targets;

            targets = targets ?? new List<BoardObject>();

            foreach(CardAbilityData.TargetInfo targetInfo in cardAbilityData.Targets)
            {
                switch (targetInfo.Target)
                {
                    case Enumerators.Target.ItSelf:
                        targets.Add(modelCaller);
                        break;
                    case Enumerators.Target.Opponent:
                        targets.Add(GetOpponentPlayer(modelCaller));
                        break;
                    case Enumerators.Target.Player:
                        targets.Add(modelCaller.OwnerPlayer);
                        break;
                    case Enumerators.Target.PlayerCard:
                        switch(targetInfo.TargetFilter)
                        {
                            case Enumerators.TargetFilter.Target:
                                break;
                            case Enumerators.TargetFilter.TargetAdjustments:
                                if (targets.Count > 0 && targets[0] is BoardUnitModel unitModel)
                                {
                                    targets.AddRange(_battlegroundController.GetAdjacentUnitsToUnit(unitModel));
                                }
                                break;
                            case Enumerators.TargetFilter.Undefined:
                                break;
                        }
                        break;
                    case Enumerators.Target.OpponentCard:
                        switch (targetInfo.TargetFilter)
                        {
                            case Enumerators.TargetFilter.Target:
                                break;
                            case Enumerators.TargetFilter.TargetAdjustments:
                                if (targets.Count > 0 && targets[0] is BoardUnitModel unitModel)
                                {
                                    targets.AddRange(_battlegroundController.GetAdjacentUnitsToUnit(unitModel));
                                }
                                break;
                            case Enumerators.TargetFilter.Undefined:
                                break;
                        }
                        break;
                    case Enumerators.Target.All:
                        targets.Add(_gameplayManager.CurrentPlayer);
                        targets.Add(_gameplayManager.OpponentPlayer);
                        targets.AddRange(_gameplayManager.CurrentPlayer.PlayerCardsController.CardsOnBoard);
                        targets.AddRange(_gameplayManager.OpponentPlayer.PlayerCardsController.CardsOnBoard);
                        break;
                }
            }

            return targets;
        }

        public List<BoardObject> FilterTargets(
            BoardUnitModel boardUnitModel,
            List<GenericParameter> genericParameters,
            List<BoardObject> targets)
        {
            List<BoardObject> filteredTargets = new List<BoardObject>();

            if (TryGetParameterValue(genericParameters,
                                     Enumerators.AbilityParameter.TargetFaction,
                                     out Enumerators.Faction targetFaction))
            {
                foreach (BoardObject target in targets)
                {
                    switch (target)
                    {
                        case BoardUnitModel unitModel:
                            if (unitModel.Faction != targetFaction)
                            {
                                filteredTargets.Add(target);
                            }
                            break;
                        case Player player:
                            if (player.SelfOverlord.Faction != targetFaction)
                            {
                                filteredTargets.Add(target);
                            }
                            break;
                    }
                }
            }


            if (TryGetParameterValue(genericParameters,
                                     Enumerators.AbilityParameter.TargetStatus,
                                     out Enumerators.UnitSpecialStatus targetStatus))
            {
                foreach (BoardObject target in targets)
                {
                    switch (target)
                    {
                        case BoardUnitModel unitModel:
                            if (unitModel.UnitSpecialStatus != targetStatus)
                            {
                                filteredTargets.Add(target);
                            }
                            break;
                    }
                }
            }

            filteredTargets.ForEach((target) => { targets.Remove(target); });
            filteredTargets.Clear();

            return targets;
        }

        #endregion

        public Player GetOpponentPlayer(BoardUnitModel model)
        {
            if (model.OwnerPlayer.IsLocalPlayer)
                return _gameplayManager.OpponentPlayer;
            else
                return _gameplayManager.CurrentPlayer;
        }

        #region parameter tools

        public T GetParameterValue<T>(IReadOnlyList<GenericParameter> genericParameters, Enumerators.AbilityParameter abilityParameter)
        {
            GenericParameter parameter = genericParameters.FirstOrDefault(param => param.AbilityParameter == abilityParameter);

            if(typeof(T).IsEnum && parameter.Value != null && parameter.Value is string value)
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }

            return (T)Convert.ChangeType(parameter.Value, typeof(T));
        }

        public bool HasParameter(IReadOnlyList<GenericParameter> genericParameters, Enumerators.AbilityParameter abilityParameter)
        {
            return genericParameters.FindAll(param => param.AbilityParameter == abilityParameter).Count > 0;
        }

        public bool TryGetParameterValue<T>(IReadOnlyList<GenericParameter> genericParameters, Enumerators.AbilityParameter abilityParameter, out T result)
        {
            if (!HasParameter(genericParameters, abilityParameter))
            {
                result = default(T);
                return false;
            }

            result = GetParameterValue<T>(genericParameters, abilityParameter);

            return true;
        }

        #endregion

        #endregion

    }
}
