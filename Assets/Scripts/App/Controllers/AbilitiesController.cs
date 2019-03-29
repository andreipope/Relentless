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
            ICardAbility ability;
            foreach (CardAbilityData abilityData in boardUnitModel.Card.Prototype.Abilities.CardAbilityData)
            {
                foreach (Enumerators.AbilityTrigger trigger in abilityData.Triggers)
                {
                    ability = GetInstance<CardAbility>($"{abilityData.Ability.ToString()}Ability");
                    ability.Init(boardUnitModel, abilityData.GenericParameters, targets);

                    CheckOnEntryAbility(ability, abilityData);

                    _activeAbilities[trigger].Add(ability);
                }
            }
        }


        #region event callers

        public void UnitStatChanged(Enumerators.Stat stat, BoardUnitModel boardUnit, int from, int to)
        {

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

        private void UnitAttackedHandler(BoardUnitModel attacker, BoardObject targetAttacked)
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

        public T GetParameterValue<T>(IReadOnlyList<GenericParameter> genericParameters, Enumerators.AbilityParameter abilityParameter)
        {
            return (T)genericParameters.FirstOrDefault(param => param.AbilityParameter == abilityParameter).Value;
        }

        public bool HasParameter(IReadOnlyList<GenericParameter> genericParameters, Enumerators.AbilityParameter abilityParameter)
        {
            return genericParameters.FindAll(param => param.AbilityParameter == abilityParameter).Count > 0;
        }

        #endregion

        private T GetInstance<T>(string className)
        {
            return (T)Activator.CreateInstance(Type.GetType($"{typeof(T).Namespace}.{className}"));
        }
    }

    public class GenericParameter
    {
        public readonly Enumerators.AbilityParameter AbilityParameter;
        public readonly object Value;

        public GenericParameter(Enumerators.AbilityParameter abilityParameter, object value)
        {
            AbilityParameter = abilityParameter;
            Value = value;
        }

        public GenericParameter(GenericParameter source)
        {
            AbilityParameter = source.AbilityParameter;
            Value = source.Value;
        }
    }

    public class CardAbilities
    {
        public IReadOnlyList<GenericParameter> DefaultParameters { get; set; }
        public IReadOnlyList<CardAbilityData> CardAbilityData { get; set; }

        public CardAbilities(
            IReadOnlyList<GenericParameter> defaultParameters,
            IReadOnlyList<CardAbilityData> cardAbilityData)
        {
            DefaultParameters = defaultParameters;
            CardAbilityData = cardAbilityData;
        }

        public CardAbilities(CardAbilities source)
        {
            DefaultParameters = source.DefaultParameters;
            CardAbilityData = source.CardAbilityData;
        }
    }

    public class CardAbilityData
    {
        public Enumerators.AbilityType Ability;
        public Enumerators.GameMechanicDescription GameMechanicDescription;

        public IReadOnlyList<Enumerators.AbilityTrigger> Triggers;
        public IReadOnlyList<Enumerators.Target> Targets;
        public IReadOnlyList<GenericParameter> GenericParameters;

        public CardAbilityData(
            Enumerators.AbilityType ability,
            Enumerators.GameMechanicDescription gameMechanicDescription,
            IReadOnlyList<Enumerators.AbilityTrigger> triggers,
            IReadOnlyList<Enumerators.Target> targets,
            IReadOnlyList<GenericParameter> genericParameters)
        {
            Ability = ability;
            GameMechanicDescription = gameMechanicDescription;
            Triggers = triggers;
            Targets = targets;
            GenericParameters = genericParameters;
        }

        public CardAbilityData(CardAbilityData source)
        {
            Ability = source.Ability;
            GameMechanicDescription = source.GameMechanicDescription;
            Triggers = source.Triggers;
            Targets = source.Targets;
            GenericParameters = source.GenericParameters;
        }
    }
}
