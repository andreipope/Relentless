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

        public void InitializeAbilities(BoardUnitModel boardUnitModel, List<BoardObject> targets = null, bool ignoreEntry = false)
        {
            ICardAbility ability;
            foreach (CardAbilityData cardAbilityData in boardUnitModel.Card.Prototype.Abilities.CardAbilityData)
            {
                if (cardAbilityData.Ability == Enumerators.AbilityType.Undefined)
                    continue;

                foreach (Enumerators.AbilityTrigger trigger in cardAbilityData.Triggers)
                {
                    ability = InternalTools.GetInstance<CardAbility>($"{cardAbilityData.Ability.ToString()}Ability");
                    ability.Init(boardUnitModel, cardAbilityData, FilterTargets(boardUnitModel, cardAbilityData,
                                                                            GetTargets(boardUnitModel, cardAbilityData, targets)));

                    _activeAbilities[trigger].Add(ability);

                    CheckOnEntryAbility(ability, cardAbilityData);
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
            ICardAbility ability;

            foreach (Enumerators.AbilityTrigger trigger in cardAbilityData.Triggers)
            {
                ability = InternalTools.GetInstance<CardAbility>($"{cardAbilityData.Ability.ToString()}Ability");
                ability.Init(boardUnitModel, cardAbilityData, new List<BoardObject>() { boardUnitModel });

                _activeAbilities[trigger].Add(ability);

                CheckOnEntryAbility(ability, cardAbilityData);
            }
        }

        #region event callers

        public void UnitStatChanged(Enumerators.Stat stat, BoardUnitModel boardUnit, int from, int to)
        {

        }

        public void UnitAttacked(BoardUnitModel attacker, BoardObject targetAttacked)
        {
            foreach (ICardAbility ability in _activeAbilities[Enumerators.AbilityTrigger.Attack])
            {
                if (ability.UnitModelOwner == attacker)
                {
                    ability.InsertTargets(FilterTargets(ability.UnitModelOwner, ability.CardAbilityData,
                                                        GetTargets(ability.UnitModelOwner, ability.CardAbilityData, new List<BoardObject>()
                    {
                        targetAttacked
                    }, true)));
                    ability.DoAction();
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
            foreach (CardAbilityData data in unitModel.Card.Prototype.Abilities.CardAbilityData)
            {
                foreach (Enumerators.AbilityTrigger trigger in data.Triggers)
                {
                    if (trigger == Enumerators.AbilityTrigger.EntryWithSelection)
                        return true;
                }
            }

            return false;
        }

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
                                break;
                            case Enumerators.TargetFilter.Undefined:
                                break;
                        }
                        break;
                    case Enumerators.Target.All:
                        targets.Add(GetOpponentPlayer(modelCaller));
                        targets.Add(modelCaller.OwnerPlayer);
                        targets.AddRange(_gameplayManager.CurrentPlayer.PlayerCardsController.CardsOnBoard);
                        targets.AddRange(_gameplayManager.OpponentPlayer.PlayerCardsController.CardsOnBoard);
                        break;
                }
            }

            return targets;
        }
        public List<BoardObject> FilterTargets(BoardUnitModel modelCaller, CardAbilityData cardAbilityData, List<BoardObject> targets)
        {
            return targets;
        }

        public Player GetOpponentPlayer(BoardUnitModel model)
        {
            if (model.OwnerPlayer.IsLocalPlayer)
                return _gameplayManager.OpponentPlayer;
            else
                return _gameplayManager.CurrentPlayer;
        }

        public T GetParameterValue<T>(IReadOnlyList<GenericParameter> genericParameters, Enumerators.AbilityParameter abilityParameter)
        {
            return (T)genericParameters.FirstOrDefault(param => param.AbilityParameter == abilityParameter).Value;
        }

        public bool HasParameter(IReadOnlyList<GenericParameter> genericParameters, Enumerators.AbilityParameter abilityParameter)
        {
            return genericParameters.FindAll(param => param.AbilityParameter == abilityParameter).Count > 0;
        }

        #endregion

    }
}
