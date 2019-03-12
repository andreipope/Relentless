using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Loom.ZombieBattleground
{
    public class AIController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(AIController));

        public BoardCardView CurrentSpellCard;

        public bool IsBrainWorking = false;

        public bool AIPaused = false;

        private const int MinTurnForAttack = 0;

        private readonly Random _random = new Random();

        private IGameplayManager _gameplayManager;

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private ILoadObjectsManager _loadObjectsManager;

        private BattlegroundController _battlegroundController;

        private BoardController _boardController;

        private CardsController _cardsController;

        private ActionsQueueController _actionsQueueController;

        private AbilitiesController _abilitiesController;

        private SkillsController _skillsController;

        private BoardArrowController _boardArrowController;

        private Enumerators.AIType _aiType;

        private List<BoardUnitModel> _attackedUnitTargets;

        private List<BoardUnitModel> _unitsToIgnoreThisTurn;

        private List<WorkingCard> _normalUnitCardInHand, _normalSpellCardInHand;

        private GameObject _fightTargetingArrowPrefab; // rewrite

        private CancellationTokenSource _aiBrainCancellationTokenSource;

        private Enumerators.AiBrainType _aiBrainType;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _boardController = _gameplayManager.GetController<BoardController>();

            _gameplayManager.GameEnded += GameEndedHandler;
            _gameplayManager.GameStarted += GameStartedHandler;

            _normalUnitCardInHand = new List<WorkingCard>();
            _normalSpellCardInHand = new List<WorkingCard>();

        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
            _aiBrainCancellationTokenSource?.Cancel();
        }

        public void InitializePlayer(InstanceId instanceId)
        {
            _gameplayManager.OpponentPlayer = new Player(instanceId, GameObject.Find("Opponent"), true);

            _fightTargetingArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _attackedUnitTargets = new List<BoardUnitModel>();
            _unitsToIgnoreThisTurn = new List<BoardUnitModel>();

            if (!_gameplayManager.IsSpecificGameplayBattleground ||
                (_gameplayManager.IsTutorial &&
                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                SpecificBattlegroundInfo.DisabledInitialization))
            {
                List<WorkingCard> workingDeck = new List<WorkingCard>();

                foreach (DeckCardData card in _gameplayManager.OpponentPlayerDeck.Cards)
                {
                    for (int i = 0; i < card.Amount; i++)
                    {
                        workingDeck.Add(_cardsController.GetWorkingCardFromCardName(card.CardName, _gameplayManager.OpponentPlayer));
                    }
                }

                _gameplayManager.OpponentPlayer.SetDeck(workingDeck, true);

                _battlegroundController.UpdatePositionOfCardsInOpponentHand();
            }

            if (_gameplayManager.IsTutorial &&
                _tutorialManager.CurrentTutorial != null &&
                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.AISpecificOrderEnabled &&
                !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)
            {
                SetAiBrainType(Enumerators.AiBrainType.Tutorial);
            }
            else
            {
                SetAiBrainType(Enumerators.AiBrainType.Normal);
            }

            _gameplayManager.OpponentPlayer.TurnStarted += TurnStartedHandler;
            _gameplayManager.OpponentPlayer.TurnEnded += TurnEndedHandler;
        }

        public void SetAiBrainType(Enumerators.AiBrainType aiBrainType)
        {
            _aiBrainType = aiBrainType;
        }

        public async Task LaunchAIBrain()
        {
            _aiBrainCancellationTokenSource = new CancellationTokenSource();
            Log.Info("brain started");

            IsBrainWorking = true;

            try
            {
                switch (_aiBrainType)
                {
                    case Enumerators.AiBrainType.DoNothing:
                        await DoNothingAiBrain(_aiBrainCancellationTokenSource.Token);
                        break;
                    case Enumerators.AiBrainType.Normal:
                        await DoAiBrain(_aiBrainCancellationTokenSource.Token);
                        break;
                    case Enumerators.AiBrainType.Tutorial:
                        await DoAiBrainForTutorial(_aiBrainCancellationTokenSource.Token);
                        break;
                    case Enumerators.AiBrainType.DontAttack:
                        await DontAttackAiBrain(_aiBrainCancellationTokenSource.Token);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (OperationCanceledException e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Info("brain canceled!");
            }

            Log.Info("brain finished");

            if (!_tutorialManager.IsTutorial ||
                (_aiBrainType == Enumerators.AiBrainType.Tutorial &&
                _tutorialManager.CurrentTutorialStep.ActionToEndThisStep == Enumerators.TutorialActivityAction.EndTurn))
            {
                IsBrainWorking = false;
            }
        }

        public async Task SetTutorialStep()
        {
            try
            {
                switch (_aiBrainType)
                {
                    case Enumerators.AiBrainType.Tutorial:
                        await DoAiBrainForTutorial(_aiBrainCancellationTokenSource.Token);
                        break;
                    default:
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("brain canceled!");
            }
        }

        private void SetAiTypeByDeck()
        {
            AIDeck aiDeck =
                _dataManager.CachedAiDecksData.Decks.Find(d => d.Deck.Id == _gameplayManager.OpponentDeckId);

            if (aiDeck != null)
            {
                SetAiType(aiDeck.Type != Enumerators.AIType.UNDEFINED ? aiDeck.Type : Enumerators.AIType.MIXED_AI);
            }
            else
            {
                throw new NullReferenceException($"OpponentDeck with id {_gameplayManager.OpponentDeckId} is null!");
            }
        }

        public void SetAiType(Enumerators.AIType aiType)
        {
            _aiType = aiType;
        }


        private void GameEndedHandler(Enumerators.EndGameType obj)
        {
            _aiBrainCancellationTokenSource?.Cancel();
        }

        private void GameStartedHandler()
        {
            if (!_gameplayManager.IsTutorial && GameClient.Get<IMatchManager>().MatchType != Enumerators.MatchType.PVP)
            {
                SetAiTypeByDeck();
            }
        }

        private async void TurnStartedHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.OpponentPlayer) ||
                !_gameplayManager.IsGameStarted)
            {
                _aiBrainCancellationTokenSource?.Cancel();
                return;
            }

            if (_tutorialManager.IsTutorial && _gameplayManager.IsSpecificGameplayBattleground &&
               !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)
                return;

            await LaunchAIBrain();
        }

        private void TurnEndedHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.OpponentPlayer))
                return;

            _aiBrainCancellationTokenSource.Cancel();

            _attackedUnitTargets.Clear();
            _unitsToIgnoreThisTurn.Clear();
        }


        private async Task DoAiBrain(CancellationToken cancellationToken)
        {
            //  await Task.Delay(TimeSpan.FromSeconds(1f));
            await LetsThink(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await LetsWaitForQueue(cancellationToken);

            await PlayCardsFromHand(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (!_tutorialManager.IsTutorial)
            {
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);

                await LetsWaitForQueue(cancellationToken);
            }

            await UseUnitsOnBoard(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await UsePlayerSkills(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await LetsWaitForQueue(cancellationToken);

            if (_gameplayManager.OpponentPlayer.SelfHero.HeroElement == Enumerators.SetType.FIRE)
            {
                await UseUnitsOnBoard(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            if (!_tutorialManager.IsTutorial)
            {
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);
            }
            _battlegroundController.StopTurn();
        }

        private async Task DoAiBrainForTutorial(CancellationToken cancellationToken)
        {
            TutorialStep step = _tutorialManager.CurrentTutorialStep;

            await LetsThink(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await LetsWaitForQueue(cancellationToken);

            List<Enumerators.TutorialActivityAction> requiredActivitiesToDoneDuringTurn = step.ToGameplayStep().RequiredActivitiesToDoneDuringStep;
            if(requiredActivitiesToDoneDuringTurn == null)
            {
                requiredActivitiesToDoneDuringTurn = _tutorialManager.GetCurrentTurnInfo().RequiredActivitiesToDoneDuringTurn;
            }

            foreach (Enumerators.TutorialActivityAction activityAction in requiredActivitiesToDoneDuringTurn)
            {
                switch (activityAction)
                {
                    case Enumerators.TutorialActivityAction.EnemyOverlordCardPlayed:
                        await PlayCardsFromHand(cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();

                        await LetsThink(cancellationToken);
                        await LetsThink(cancellationToken);
                        await LetsThink(cancellationToken);

                        await LetsWaitForQueue(cancellationToken);
                        break;
                    case Enumerators.TutorialActivityAction.BattleframeAttacked:
                        await UseUnitsOnBoard(cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        await UsePlayerSkills(cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();

                        await LetsWaitForQueue(cancellationToken);
                        break;
                    default:
                        break;
                }

                if (CheckTutorialAIStepPaused())
                    return;
            }

            if (_gameplayManager.OpponentPlayer.SelfHero.HeroElement == Enumerators.SetType.FIRE)
            {
                await UseUnitsOnBoard(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (step.ActionToEndThisStep == Enumerators.TutorialActivityAction.EndTurn)
            {
                _battlegroundController.StopTurn();
            }
        }

        private bool CheckTutorialAIStepPaused()
        {
            if (!_tutorialManager.IsTutorial ||
                _tutorialManager.CurrentTutorialStep == null ||
                _tutorialManager.CurrentTutorialStep.ToGameplayStep() == null)
            {
                return false;
            }
            AIPaused = _tutorialManager.CurrentTutorialStep.ToGameplayStep().AIShouldBePaused;
            return AIPaused;
        }

        private async Task DoNothingAiBrain(CancellationToken cancellationToken)
        {
            await LetsThink(cancellationToken);
            await LetsThink(cancellationToken);
            _battlegroundController.StopTurn();
        }

        private async Task DontAttackAiBrain(CancellationToken cancellationToken)
        {
            await LetsThink(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await PlayCardsFromHand(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await LetsThink(cancellationToken);
            await LetsThink(cancellationToken);
            await LetsThink(cancellationToken);

            _battlegroundController.StopTurn();
        }

        // ai step 1
        private async Task PlayCardsFromHand(CancellationToken cancellationToken)
        {
            if (_tutorialManager.IsTutorial && !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().DisabledSpecificTurnInfos)
            {
                foreach (PlayCardActionInfo playCardActionInfo in _tutorialManager.GetCurrentTurnInfo().PlayCardsSequence)
                {
                    if (CheckTutorialAIStepPaused())
                        return;

                    WorkingCard card = _gameplayManager.OpponentPlayer.CardsInHand.FirstOrDefault(
                        cardInHand => cardInHand.TutorialObjectId == playCardActionInfo.TutorialObjectId);

                    if (card != null)
                    {

                        PlayCardOnBoard(card, playCardActionInfo: playCardActionInfo);

                        await LetsThink(cancellationToken);
                        await LetsThink(cancellationToken);

                        if (_aiBrainType == Enumerators.AiBrainType.Tutorial)
                        {
                            break;
                        }
                    }
                }

                return;
            }

            // CHECK about CardsInHand in modification collection!
            await CheckGooCard(cancellationToken);

            List<WorkingCard> cardsInHand = new List<WorkingCard>();
            cardsInHand.AddRange(_normalUnitCardInHand);

            bool wasAction = false;
            foreach (WorkingCard card in cardsInHand)
            {
                if (_gameplayManager.OpponentPlayer.BoardCards.Count >= _gameplayManager.OpponentPlayer.MaxCardsInPlay)
                {
                    break;
                }

                if (CardCanBePlayable(card) && CheckSpecialCardRules(card))
                {
                    PlayCardOnBoard(card);
                    wasAction = true;
                    await LetsThink(cancellationToken);
                    await LetsThink(cancellationToken);
                }

                await LetsWaitForQueue(cancellationToken);
            }

            foreach (WorkingCard card in _normalSpellCardInHand)
            {
                if (CardCanBePlayable(card) && CheckSpecialCardRules(card))
                {
                    PlayCardOnBoard(card);
                    wasAction = true;
                    await LetsThink(cancellationToken);
                    await LetsThink(cancellationToken);
                }

                await LetsWaitForQueue(cancellationToken);
            }

            if (wasAction)
            {
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);
            }

            await CheckGooCard(cancellationToken);
        }

        // ai step 2
        private async Task UseUnitsOnBoard(CancellationToken cancellationToken)
        {
            if (_tutorialManager.IsTutorial && !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().DisabledSpecificTurnInfos)
            {
                foreach (UseBattleframeActionInfo frame in _tutorialManager.GetCurrentTurnInfo().UseBattleframesSequence)
                {
                    if (CheckTutorialAIStepPaused())
                        return;

                    BoardUnitModel unit = GetUnitsOnBoard().Find(unitOnBoard => !unitOnBoard.AttackedThisTurn &&
                                                                       unitOnBoard.Card.Prototype.Name.ToLowerInvariant() ==
                                                                       _tutorialManager.GetCardNameByTutorialObjectId(frame.TutorialObjectId).
                                                                       ToLowerInvariant() &&
                                                                       UnitCanBeUsable(unitOnBoard));
                    BoardObject target = null;

                    if (frame.TargetType == Enumerators.SkillTargetType.OPPONENT_CARD)
                    {
                        target = GetOpponentUnitsOnBoard().Find(targetUnit => targetUnit.Card.Prototype.Name.ToLowerInvariant() ==
                                                                     _tutorialManager.GetCardNameByTutorialObjectId(frame.TargetTutorialObjectId).
                                                                     ToLowerInvariant() &&
                                                                     targetUnit.CurrentHp > 0);
                    }
                    else
                    {
                        target = _gameplayManager.CurrentPlayer;
                    }

                    if (target != null && unit != null)
                    {
                        unit.DoCombat(target);
                        if (_aiBrainType == Enumerators.AiBrainType.Tutorial)
                            break;
                    }

                    await LetsWaitForQueue(cancellationToken);

                    await LetsThink(cancellationToken);
                }

                return;
            }

            List<BoardUnitModel> unitsOnBoard = new List<BoardUnitModel>();
            List<BoardUnitModel> alreadyUsedUnits = new List<BoardUnitModel>();

            unitsOnBoard.AddRange(GetUnitsOnBoard());

            if (OpponentHasHeavyUnits())
            {
                foreach (BoardUnitModel unit in unitsOnBoard)
                {
                    if (unit.AttackTargetsAvailability.Count == 0 ||
                        !unit.AttackTargetsAvailability.Contains(Enumerators.SkillTargetType.OPPONENT_CARD))
                        continue;

                    await LetsWaitForQueue(cancellationToken);

                    while (UnitCanBeUsable(unit))
                    {
                        BoardUnitModel attackedUnit = GetTargetOpponentUnit();
                        if (attackedUnit != null)
                        {
                            unit.DoCombat(attackedUnit);

                            alreadyUsedUnits.Add(unit);

                            await LetsThink(cancellationToken);
                            if (!OpponentHasHeavyUnits())
                            {
                                break;
                            }
                        }
                        else break;
                    }
                }
            }

            foreach (BoardUnitModel creature in alreadyUsedUnits)
            {
                unitsOnBoard.Remove(creature);
            }

            int totalValue = GetPlayerAttackingValue();
            if ((totalValue >= _gameplayManager.OpponentPlayer.Defense || _aiType == Enumerators.AIType.BLITZ_AI ||
                _aiType == Enumerators.AIType.TIME_BLITZ_AI))
            {
                foreach (BoardUnitModel unit in unitsOnBoard)
                {
                    if (unit.HasBuffRush || unit.AttackTargetsAvailability.Count == 0)
                        continue;

                    if (!unit.AttackTargetsAvailability.Contains(Enumerators.SkillTargetType.OPPONENT))
                        continue;

                    if (!_tutorialManager.IsTutorial)
                    {
                        await LetsWaitForQueue(cancellationToken);
                    }

                    while (UnitCanBeUsable(unit))
                    {
                        unit.DoCombat(_gameplayManager.CurrentPlayer);
                        await LetsThink(cancellationToken);
                    }
                }
            }
            else
            {
                foreach (BoardUnitModel unit in unitsOnBoard)
                {
                    if (unit.AttackTargetsAvailability.Count == 0)
                        continue;

                    if (!_tutorialManager.IsTutorial)
                    {
                        await LetsWaitForQueue(cancellationToken);
                    }

                    while (UnitCanBeUsable(unit))
                    {
                        if (GetPlayerAttackingValue() > GetOpponentAttackingValue() &&
                            !unit.HasBuffRush &&
                            unit.AttackTargetsAvailability.Contains(Enumerators.SkillTargetType.OPPONENT))
                        { 
                            unit.DoCombat(_gameplayManager.CurrentPlayer);
                            await LetsThink(cancellationToken);
                        }
                        else
                        {
                            BoardUnitModel attackedCreature = GetRandomOpponentUnit();

                            if (attackedCreature != null && unit.AttackTargetsAvailability.Contains(Enumerators.SkillTargetType.OPPONENT_CARD))
                            {
                                unit.DoCombat(attackedCreature);
                                await LetsThink(cancellationToken);
                            }
                            else
                            {
                                if (unit.HasBuffRush)
                                    break;

                                if (!unit.AttackTargetsAvailability.Contains(Enumerators.SkillTargetType.OPPONENT))
                                    break;

                                unit.DoCombat(_gameplayManager.CurrentPlayer);
                                await LetsThink(cancellationToken);
                            }
                        }
                    }
                }
            }
        }

        // ai step 3
        private async Task UsePlayerSkills(CancellationToken cancellationToken)
        {
            bool wasAction = false;

            if (_gameplayManager.IsTutorial && !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().DisabledSpecificTurnInfos)
                return;

            if(_gameplayManager.OpponentPlayer.IsStunned)
                return;

            if (_skillsController.OpponentPrimarySkill != null)
            {
                if (_skillsController.OpponentPrimarySkill.IsSkillReady)
                {
                    await LetsWaitForQueue(cancellationToken);

                    DoBoardSkill(_skillsController.OpponentPrimarySkill);
                    wasAction = true;
                }
            }

            if (wasAction)
            {
                await LetsThink(cancellationToken);
            }

            if (_skillsController.OpponentSecondarySkill != null)
            {
                if (_skillsController.OpponentSecondarySkill.IsSkillReady)
                {
                    await LetsWaitForQueue(cancellationToken);

                    DoBoardSkill(_skillsController.OpponentSecondarySkill);
                    wasAction = true;
                }
            }

            if (wasAction)
            {
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);
            }
        }

        private async Task LetsWaitForQueue(CancellationToken cancellationToken, int delay = 100)
        {
            while (_actionsQueueController.ActionsInQueue > 0)
            {
                await Task.Delay(delay, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        // some thinking - delay between general actions
        private async Task LetsThink(CancellationToken cancellationToken)
        {
            await Task.Delay(Constants.DelayBetweenAiActions, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task CheckGooCard(CancellationToken cancellationToken)
        {
            int benefit = 0;
            int boardCount = 0;
            int gooAmount = _gameplayManager.OpponentPlayer.CurrentGoo;
            List<WorkingCard> overflowGooCards = new List<WorkingCard>();
            List<WorkingCard> cards = new List<WorkingCard>();
            cards.AddRange(GetUnitCardsInHand());
            cards.AddRange(GetSpellCardsInHand());
            cards.RemoveAll(x => x == null || x.Prototype == null);
            cards  = cards.FindAll(x => CardBePlayableForOverflowGoo(x.Prototype.Cost, gooAmount));
            AbilityData overflowGooAbility;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Prototype.Abilities != null)
                {
                    AbilityData attackOverlordAbility = cards[i].Prototype.Abilities
                        .FirstOrDefault(x => x.AbilityType == Enumerators.AbilityType.ATTACK_OVERLORD);
                    if (attackOverlordAbility != null)
                    {
                        if (attackOverlordAbility.Value * 2 >= _gameplayManager.OpponentPlayer.Defense)
                            break;
                    }

                    overflowGooAbility = cards[i].Prototype.Abilities
                        .FirstOrDefault(x => x.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO);
                    if (overflowGooAbility != null)
                    {
                        if (_gameplayManager.OpponentPlayer.BoardCards.Count + boardCount < _gameplayManager.OpponentPlayer.MaxCardsInPlay - 1)
                        {
                            boardCount++;
                            gooAmount -= cards[i].Prototype.Cost;
                            benefit += overflowGooAbility.Value - cards[i].Prototype.Cost;
                            overflowGooCards.Add(cards[i]);
                            cards = cards.FindAll(x => CardBePlayableForOverflowGoo(x.Prototype.Cost, gooAmount));
                        }
                    }
                }
            }

            WorkingCard expensiveCard =
                GetUnitCardsInHand()
                    .Find(
                        x => x.InstanceCard.Cost > _gameplayManager.OpponentPlayer.CurrentGoo &&
                            x.InstanceCard.Cost <= _gameplayManager.OpponentPlayer.CurrentGoo + benefit);
            if (expensiveCard != null)
            {
                bool wasAction = false;
                foreach (WorkingCard card in overflowGooCards)
                {
                    if (_gameplayManager.OpponentPlayer.BoardCards.Count >= _gameplayManager.OpponentPlayer.MaxCardsInPlay)
                        break;
                    if (CardCanBePlayable(card))
                    {
                        PlayCardOnBoard(card);
                        wasAction = true;
                        await LetsThink(cancellationToken);
                        await LetsThink(cancellationToken);
                    }
                }

                PlayCardOnBoard(expensiveCard);

                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);
                if (wasAction)
                {
                    await LetsThink(cancellationToken);
                    await LetsThink(cancellationToken);
                }
            }
            else
            {
                _normalUnitCardInHand.Clear();
                _normalUnitCardInHand.AddRange(GetUnitCardsInHand());
                _normalUnitCardInHand.RemoveAll(x =>
                    x.Prototype.Abilities.Any(z => z.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO));
                _normalSpellCardInHand.Clear();
                _normalSpellCardInHand.AddRange(GetSpellCardsInHand());
                _normalSpellCardInHand.RemoveAll(x =>
                    x.Prototype.Abilities.Any(z => z.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO));
            }

            await LetsThink(cancellationToken);
        }

        private bool CardBePlayableForOverflowGoo(int cost, int goo)
        {
            if (_gameplayManager.OpponentPlayer == null)
                return false;
            if (!Constants.DevModeEnabled)
            {
                return cost <= goo && _gameplayManager.OpponentPlayer.Turn > MinTurnForAttack;
            }
            else
            {
                return true;
            }
        }

        private bool CardCanBePlayable(WorkingCard card)
        {
            if (!Constants.DevModeEnabled)
            {
                return card.InstanceCard.Cost <= _gameplayManager.OpponentPlayer.CurrentGoo &&
                _gameplayManager.OpponentPlayer.Turn > MinTurnForAttack;
            }
            else
            {
                return true;
            }
        }

        private bool UnitCanBeUsable(BoardUnitModel unit)
        {
            return unit.UnitCanBeUsable();
        }

        private bool CheckSpecialCardRules(WorkingCard card)
        {
            if (card.Prototype.Abilities != null)
            {
                foreach (AbilityData ability in card.Prototype.Abilities)
                {
                    if (ability.AbilityType == Enumerators.AbilityType.ATTACK_OVERLORD)
                    {
                        // smart enough HP to use goo carriers
                        if (ability.Value * 2 >= _gameplayManager.OpponentPlayer.Defense)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void PlayCardOnBoard(WorkingCard card, bool ignorePlayAbility = false, PlayCardActionInfo playCardActionInfo = null)
        {
            _actionsQueueController.AddNewActionInToQueue((parameter, completeCallback) =>
            {
                if(!CardCanBePlayable(card) && !ignorePlayAbility)
                {
                    completeCallback?.Invoke();
                    return;
                }

                bool needTargetForAbility = false;

                if (card.Prototype.Abilities != null && card.Prototype.Abilities.Count > 0)
                {
                    List<AbilityData> abilitiesWithTargets = card.Prototype.Abilities.FindAll(x => x.AbilityTargetTypes.Count > 0);

                    if (abilitiesWithTargets.Count > 0)
                    {
                        foreach(AbilityData data in abilitiesWithTargets)
                        {
                            if (data.CallType == Enumerators.AbilityCallType.ENTRY &&
                                data.ActivityType == Enumerators.AbilityActivityType.ACTIVE)
                            {
                                needTargetForAbility = true;
                            }
                        }
                    }                    
                }

                BoardObject target = null;

                if (needTargetForAbility)
                {
                    if (_gameplayManager.IsTutorial && playCardActionInfo != null)
                    {
                        if (!string.IsNullOrEmpty(_tutorialManager.GetCardNameByTutorialObjectId(playCardActionInfo.TargetTutorialObjectId)))
                        {
                            switch (playCardActionInfo.TargetType)
                            {
                                case Enumerators.SkillTargetType.OPPONENT:
                                    target = _gameplayManager.CurrentPlayer;
                                    break;
                                case Enumerators.SkillTargetType.OPPONENT_CARD:
                                    target = GetOpponentUnitsOnBoard().Find(x => x.Card.Prototype.Name.ToLowerInvariant() ==
                                                                            _tutorialManager.GetCardNameByTutorialObjectId(playCardActionInfo.TargetTutorialObjectId)
                                                                            .ToLowerInvariant());
                                    break;
                            }
                        }
                    }
                    else
                    {
                        target = GetAbilityTarget(card);
                    }
                }
                switch (card.Prototype.CardKind)
                {
                    case Enumerators.CardKind.CREATURE when _battlegroundController.OpponentBoardCards.Count < _gameplayManager.OpponentPlayer.MaxCardsInPlay:
                        _gameplayManager.OpponentPlayer.RemoveCardFromHand(card);
                        _gameplayManager.OpponentPlayer.AddCardToBoard(card, ItemPosition.End);

                        _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card.InstanceId, target, null, (x, y) =>
                        {
                            PlayCardCompleteHandler(x, y, completeCallback);
                        });

                        _cardsController.DrawCardInfo(card);
                        break;
                    case Enumerators.CardKind.SPELL:
                        {
                            if ((target != null && needTargetForAbility) || !needTargetForAbility)
                            {
                                _gameplayManager.OpponentPlayer.RemoveCardFromHand(card);
                                _gameplayManager.OpponentPlayer.AddCardToBoard(card, ItemPosition.End);

                                _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card.InstanceId, target, null, (x, y) =>
                                {
                                    PlayCardCompleteHandler(x, y, completeCallback);
                                });
                                _cardsController.DrawCardInfo(card);
                            }
                            else
                            {
                                completeCallback?.Invoke();
                            }

                            break;
                        }
                    default:
                        completeCallback?.Invoke();
                        break;
                }
            }, Enumerators.QueueActionType.CardPlay);
        }

        private void PlayCardCompleteHandler(WorkingCard card, BoardObject target, Action completeCallback)
        {
            completeCallback?.Invoke();

            if ( card == null)
                return;

            GameplayQueueAction<object> callAbilityAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsage, blockQueue: true);
            GameplayQueueAction<object> ranksBuffAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.RankBuff);

            _gameplayManager.OpponentPlayer.CurrentGoo -= card.InstanceCard.Cost;

            switch (card.Prototype.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    {
                        BoardUnitView boardUnitViewElement = new BoardUnitView(new BoardUnitModel(card), GameObject.Find("OpponentBoard").transform);
                        GameObject boardUnit = boardUnitViewElement.GameObject;
                        boardUnit.tag = SRTags.OpponentOwned;
                        boardUnit.transform.position = Vector3.up * 2f; // Start pos before moving cards to the opponents board

                        _battlegroundController.OpponentBoardCards.Insert(ItemPosition.End, boardUnitViewElement);
                        _gameplayManager.OpponentPlayer.BoardCards.Insert(ItemPosition.End, boardUnitViewElement);

                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = boardUnitViewElement.Model,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });

                        _gameplayManager.GetController<RanksController>().UpdateRanksByElements(card.Owner.BoardCards, card, ranksBuffAction);

                        _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitViewElement.Model, false);

                        _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.OpponentPlayer,
                            () =>
                            {
                                bool createTargetArrow = false;

                                if (card.Prototype.Abilities != null && card.Prototype.Abilities.Count > 0)
                                {
                                    createTargetArrow =
                                        _abilitiesController.IsAbilityCanActivateTargetAtStart(
                                            card.Prototype.Abilities[0]);
                                }

                                if (target != null)
                                {
                                    Action callback = () =>
                                    {
                                        _abilitiesController.CallAbility(card.Prototype, null, card, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model,
                                        null, false, (status) =>
                                        {
                                            if (!status)
                                            {
                                                ranksBuffAction.Action = null;
                                                ranksBuffAction.ForceActionDone();
                                            }

                                        }, callAbilityAction, target);

                                        _actionsQueueController.ForceContinueAction(callAbilityAction);
                                    };

                                    _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(boardUnit.transform, target, action: callback);
                                }
                                else
                                {
                                    _abilitiesController.CallAbility(card.Prototype, null, card,
                                        Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, callAbilityAction);

                                    _actionsQueueController.ForceContinueAction(callAbilityAction);
                                }
                            });
                        boardUnitViewElement.PlayArrivalAnimation(playUniqueAnimation: true);
                    }
                    break;

                case Enumerators.CardKind.SPELL:
                    {
                        GameObject spellCard = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                        spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;

                        CurrentSpellCard = new SpellBoardCard(spellCard);

                        CurrentSpellCard.Init(new BoardUnitModel(card));
                        CurrentSpellCard.SetHighlightingEnabled(false);

                        BoardSpell boardSpell = new BoardSpell(spellCard, card);

                        spellCard.gameObject.SetActive(false);

                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = boardSpell,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });

                        bool createTargetArrow = false;

                        if (card.Prototype.Abilities != null && card.Prototype.Abilities.Count > 0)
                        {
                            createTargetArrow =
                                _abilitiesController.IsAbilityCanActivateTargetAtStart(card.Prototype.Abilities[0]);
                        }

                        if (target != null)
                        {
                            Action callback = () =>
                            {
                                _abilitiesController.CallAbility(card.Prototype, null, card, Enumerators.CardKind.SPELL, boardSpell, null, false, null, callAbilityAction, target);
                                _actionsQueueController.ForceContinueAction(callAbilityAction);
                            };

                            _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(_gameplayManager.OpponentPlayer.AvatarObject.transform, target, action: callback);
                        }
                        else
                        {
                            _abilitiesController.CallAbility(card.Prototype, null, card, Enumerators.CardKind.SPELL, boardSpell, null, false, null, callAbilityAction);

                            _actionsQueueController.ForceContinueAction(callAbilityAction);
                            ranksBuffAction.ForceActionDone();
                        }
                    }
                    break;
            }
        }

        private BoardObject GetAbilityTarget(WorkingCard card)
        {
            IReadOnlyCard prototype = card.Prototype;

            BoardObject target = null;

            List<AbilityData> abilitiesWithTarget = new List<AbilityData>();

            bool needsToSelectTarget = false;
            foreach (AbilityData ability in prototype.Abilities)
            {
                foreach (Enumerators.AbilityTargetType item in ability.AbilityTargetTypes)
                {
                    switch (item)
                    {
                        case Enumerators.AbilityTargetType.OPPONENT_CARD:
                            if (_gameplayManager.CurrentPlayer.BoardCards.Count > 1 ||
                                ability.AbilityType == Enumerators.AbilityType.CARD_RETURN &&
                                _gameplayManager.CurrentPlayer.BoardCards.Count > 0)
                            {
                                needsToSelectTarget = true;
                                abilitiesWithTarget.Add(ability);
                            }

                            break;
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            if (_gameplayManager.OpponentPlayer.BoardCards.Count > 1 ||
                                prototype.CardKind == Enumerators.CardKind.SPELL ||
                                ability.AbilityType == Enumerators.AbilityType.CARD_RETURN &&
                                _gameplayManager.OpponentPlayer.BoardCards.Count > 0)
                            {
                                needsToSelectTarget = true;
                                abilitiesWithTarget.Add(ability);
                            }

                            break;
                        case Enumerators.AbilityTargetType.PLAYER:
                        case Enumerators.AbilityTargetType.OPPONENT:
                        case Enumerators.AbilityTargetType.ALL:
                            needsToSelectTarget = true;
                            abilitiesWithTarget.Add(ability);
                            break;
                    }
                }
            }

            if (!needsToSelectTarget)
                return null;

            foreach (AbilityData ability in abilitiesWithTarget)
            {
                switch (ability.AbilityType)
                {
                    case Enumerators.AbilityType.ADD_GOO_VIAL:
                        target = _gameplayManager.OpponentPlayer;
                        break;
                    case Enumerators.AbilityType.CARD_RETURN:
                        if (!AddRandomTargetUnit(true, ref target, false, true))
                        {
                            AddRandomTargetUnit(false, ref target, true, true);
                        }

                        break;
                    case Enumerators.AbilityType.DAMAGE_TARGET:
                        CheckAndAddTargets(ability, ref target);
                        break;
                    case Enumerators.AbilityType.DAMAGE_TARGET_ADJUSTMENTS:
                        AddRandomTargetUnit(true, ref target);
                        break;
                    case Enumerators.AbilityType.MASSIVE_DAMAGE:
                        AddRandomTargetUnit(true, ref target);
                        break;
                    case Enumerators.AbilityType.MODIFICATOR_STATS:
                        if (ability.Value > 0)
                        {
                            AddRandomTargetUnit(false, ref target);
                        }
                        else
                        {
                            AddRandomTargetUnit(true, ref target);
                        }

                        break;
                    case Enumerators.AbilityType.STUN:
                        CheckAndAddTargets(ability, ref target);
                        break;
                    case Enumerators.AbilityType.STUN_OR_DAMAGE_ADJUSTMENTS:
                        CheckAndAddTargets(ability, ref target);
                        break;
                    case Enumerators.AbilityType.CHANGE_STAT:
                        if (ability.Value > 0)
                        {
                            AddRandomTargetUnit(false, ref target);
                        }
                        else
                        {
                            AddRandomTargetUnit(true, ref target);
                        }

                        break;
                    case Enumerators.AbilityType.SUMMON:
                        break;
                    case Enumerators.AbilityType.WEAPON:
                        target = _gameplayManager.CurrentPlayer;
                        break;
                    case Enumerators.AbilityType.SPURT:
                        AddRandomTargetUnit(true, ref target);
                        break;
                    case Enumerators.AbilityType.SPELL_ATTACK:
                        CheckAndAddTargets(ability, ref target);
                        break;
                    case Enumerators.AbilityType.HEAL:
                        List<BoardUnitModel> units = GetUnitsWithLowHp();

                        if (units.Count > 0)
                        {
                            target = units[_random.Next(0, units.Count)];
                        }
                        else
                        {
                            target = _gameplayManager.OpponentPlayer;
                        }

                        break;
                    case Enumerators.AbilityType.DOT:
                        CheckAndAddTargets(ability, ref target);
                        break;
                    case Enumerators.AbilityType.DESTROY_UNIT_BY_TYPE:
                        GetTargetByType(ability, ref target, false);
                        break;
                }

                return target; // hack to handle only one ability
            }

            return null;
        }

        private void CheckAndAddTargets(AbilityData ability, ref BoardObject target)
        {
            if (ability.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
            {
                AddRandomTargetUnit(true, ref target);
            }
            else if (ability.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT))
            {
                target = _gameplayManager.CurrentPlayer;
            }
        }

        private void GetTargetByType(AbilityData ability, ref BoardObject target, bool checkPlayerAlso)
        {
            if (ability.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
            {
                IReadOnlyList<BoardUnitView> targets = GetHeavyUnitsOnBoard(_gameplayManager.CurrentPlayer);

                if (targets.Count > 0)
                {
                    target = targets[UnityEngine.Random.Range(0, targets.Count)].Model;
                }

                if (checkPlayerAlso && target == null &&
                    ability.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER_CARD))
                {
                    target = _gameplayManager.CurrentPlayer;

                    targets = GetHeavyUnitsOnBoard(_gameplayManager.OpponentPlayer);

                    if (targets.Count > 0)
                    {
                        target = targets[UnityEngine.Random.Range(0, targets.Count)].Model;
                    }
                }
            }
        }

        private IReadOnlyList<BoardUnitView> GetHeavyUnitsOnBoard(Player player)
        {
            return player.BoardCards.FindAll(x => x.Model.HasHeavy || x.Model.HasBuffHeavy);
        }

        private bool AddRandomTargetUnit(
            bool opponent, ref BoardObject target, bool lowHp = false, bool addAttackIgnore = false)
        {
            BoardUnitModel boardUnit = opponent ? GetRandomOpponentUnit() : GetRandomUnit(lowHp);
            if (boardUnit == null)
                return false;

            target = boardUnit;

            if (addAttackIgnore)
            {
                _attackedUnitTargets.Add(boardUnit);
            }

            return true;
        }

        private int GetPlayerAttackingValue()
        {
            int power = 0;
            foreach (BoardUnitView creature in _gameplayManager.OpponentPlayer.BoardCards)
            {
                if (creature.Model.CurrentHp > 0 && (creature.Model.NumTurnsOnBoard >= 1 || creature.Model.HasFeral))
                {
                    power += creature.Model.CurrentDamage;
                }
            }

            return power;
        }

        private int GetOpponentAttackingValue()
        {
            int power = 0;
            foreach (BoardUnitView card in _gameplayManager.CurrentPlayer.BoardCards)
            {
                power += card.Model.CurrentDamage;
            }

            return power;
        }

        private List<BoardUnitModel> GetUnitsWithLowHp(List<BoardUnitModel> unitsToIgnore = null)
        {
            List<BoardUnitModel> finalList = new List<BoardUnitModel>();

            List<BoardUnitModel> list = GetUnitsOnBoard();

            foreach (BoardUnitModel item in list)
            {
                if (item.CurrentHp < item.MaxCurrentHp && item.CurrentHp > 0 && !item.IsDead)
                {
                    finalList.Add(item);
                }
            }

            if (unitsToIgnore != null)
            {
                finalList = finalList.FindAll(x => !unitsToIgnore.Contains(x));
            }

            finalList = finalList.OrderBy(x => x.CurrentHp).ThenBy(y => y.CurrentHp.ToString().Length).ToList();

            return finalList;
        }

        private List<WorkingCard> GetUnitCardsInHand()
        {
            IReadOnlyList<WorkingCard> list =
                _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x =>
                    x.Prototype.CardKind == Enumerators.CardKind.CREATURE);

            List<Card> cards = new List<Card>();

            foreach (WorkingCard item in list)
            {
                cards.Add(_dataManager.CachedCardsLibraryData.GetCardFromName(item.Prototype.Name));
            }

            cards = cards.OrderBy(x => x.Cost).ThenBy(y => y.Cost.ToString().Length).ToList();

            List<WorkingCard> sortedList = new List<WorkingCard>();

            cards.Reverse();

            foreach (Card item in cards)
            {
                sortedList.Add(list.First(x => x.Prototype.MouldId == item.MouldId && !sortedList.Contains(x)));
            }

            return sortedList;
        }

        private IReadOnlyList<WorkingCard> GetSpellCardsInHand()
        {
            return _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x =>
                x.Prototype.CardKind == Enumerators.CardKind.SPELL);
        }

        private List<BoardUnitModel> GetUnitsOnBoard()
        {
            return
                _gameplayManager.OpponentPlayer.BoardCards
                    .FindAll(x => x.Model.CurrentHp > 0 && !x.Model.IsDead)
                    .Select(x => x.Model)
                    .ToList();
        }

        private BoardUnitModel GetRandomUnit(bool lowHp = false, List<BoardUnitModel> unitsToIgnore = null)
        {
            List<BoardUnitModel> eligibleUnits;

            if (!lowHp)
            {
                eligibleUnits =
                    _gameplayManager.OpponentPlayer.BoardCards
                        .FindAll(x => x.Model.CurrentHp > 0 && !_attackedUnitTargets.Contains(x.Model) && !x.Model.IsDead)
                        .Select(x => x.Model)
                        .ToList();
            }
            else
            {
                eligibleUnits =
                    _gameplayManager.OpponentPlayer.BoardCards
                        .FindAll(x => x.Model.CurrentHp < x.Model.MaxCurrentHp && x.Model.CurrentHp > 0 && !_attackedUnitTargets.Contains(x.Model) && !x.Model.IsDead)
                        .Select(x => x.Model)
                        .ToList();
            }

            if (unitsToIgnore != null)
            {
                eligibleUnits = eligibleUnits.FindAll(x => !unitsToIgnore.Contains(x));
            }

            if (eligibleUnits.Count > 0)
            {
                return eligibleUnits[_random.Next(0, eligibleUnits.Count)];
            }

            return null;
        }

        private BoardUnitModel GetTargetOpponentUnit()
        {
            List<BoardUnitModel> eligibleUnits =
                _gameplayManager.CurrentPlayer.BoardCards
                    .FindAll(x => x.Model.CurrentHp > 0 && !x.Model.IsDead)
                    .Select(x => x.Model)
                    .ToList();

            if (eligibleUnits.Count > 0)
            {
                List<BoardUnitModel> heavyUnits = eligibleUnits.FindAll(x => x.IsHeavyUnit);
                if (heavyUnits.Count >= 1)
                {
                    return heavyUnits[_random.Next(0, heavyUnits.Count)];
                }

                return eligibleUnits[_random.Next(0, eligibleUnits.Count)];
            }

            return null;
        }

        private List<BoardUnitModel> GetOpponentUnitsOnBoard()
        {
        return _gameplayManager.CurrentPlayer.BoardCards
                    .FindAll(x => x.Model.CurrentHp > 0 && !x.Model.IsDead)
                    .Select(x => x.Model)
                    .ToList();
        }

        private BoardUnitModel GetRandomOpponentUnit(List<BoardUnitModel> unitsToIgnore = null)
        {
            List<BoardUnitModel> eligibleCreatures =
                _gameplayManager.CurrentPlayer.BoardCards
                    .Select(x => x.Model)
                    .Where(x => x.CurrentHp > 0 && !_attackedUnitTargets.Contains(x) && !x.IsDead)
                    .ToList();

            if (unitsToIgnore != null)
            {
                eligibleCreatures = eligibleCreatures.FindAll(x => !unitsToIgnore.Contains(x));
            }

            if (eligibleCreatures.Count > 0)
            {
                return eligibleCreatures[_random.Next(0, eligibleCreatures.Count)];
            }

            return null;
        }

        private bool OpponentHasHeavyUnits()
        {
            List<BoardUnitModel> board =
                _gameplayManager.CurrentPlayer.BoardCards
                    .Select(x => x.Model)
                    .ToList();
            List<BoardUnitModel> eligibleCreatures = board.FindAll(x => x.CurrentHp > 0 && !x.IsDead);
            if (eligibleCreatures.Count > 0)
            {
                List<BoardUnitModel> provokeCreatures = eligibleCreatures.FindAll(x => x.IsHeavyUnit);
                return provokeCreatures != null && provokeCreatures.Count >= 1;
            }

            return false;
        }

        private void DoBoardSkill(BoardSkill skill)
        {
            BoardObject target = null;

            Enumerators.AffectObjectType selectedObjectType = Enumerators.AffectObjectType.None;

            switch (skill.Skill.OverlordSkill)
            {
                case Enumerators.OverlordSkill.HARDEN:
                case Enumerators.OverlordSkill.DRAW:
                    target = _gameplayManager.OpponentPlayer;
                    break;
                case Enumerators.OverlordSkill.STONE_SKIN:
                case Enumerators.OverlordSkill.FORTIFY:
                case Enumerators.OverlordSkill.WIND_SHIELD:
                case Enumerators.OverlordSkill.INFECT:
                case Enumerators.OverlordSkill.TOXIC_POWER:
                    {
                        List<BoardUnitModel> units = GetUnitsOnBoard().FindAll(x => x.Card.Prototype.CardSetType ==
                                                                        _gameplayManager.OpponentPlayer.SelfHero.HeroElement);
                        if (units.Count > 0)
                        {
                            target = units[UnityEngine.Random.Range(0, units.Count)];
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                    {
                        List<BoardUnitModel> units = GetUnitsWithLowHp();

                        if (units.Count > 0)
                        {
                            target = units[0];
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                    break;
                case Enumerators.OverlordSkill.ICE_WALL:
                case Enumerators.OverlordSkill.ENHANCE:
                    {
                        List<BoardUnitModel> units = GetUnitsOnBoard().FindAll(x => x.Card.Prototype.CardSetType ==
                                                                        _gameplayManager.OpponentPlayer.SelfHero.HeroElement);

                        if (units.Count > 0)
                        {
                            target = units[0];
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                        {
                            target = _gameplayManager.OpponentPlayer;
                            selectedObjectType = Enumerators.AffectObjectType.Player;
                        }
                    }
                    break;
                case Enumerators.OverlordSkill.MEND:
                    {
                        target = _gameplayManager.OpponentPlayer;
                        selectedObjectType = Enumerators.AffectObjectType.Player;

                        if (_gameplayManager.OpponentPlayer.Defense > 13)
                        {
                            if (skill.Skill.ElementTargetTypes.Count > 0)
                            {
                                _unitsToIgnoreThisTurn =
                                    _gameplayManager.OpponentPlayer.BoardCards
                                    .FindAll(x => !skill.Skill.ElementTargetTypes.Contains(x.Model.Card.Prototype.CardSetType) && !x.Model.IsDead)
                                    .Select(x => x.Model)
                                    .ToList();
                            }

                            List<BoardUnitModel> units = GetUnitsWithLowHp(_unitsToIgnoreThisTurn);

                            if (units.Count > 0)
                            {
                                target = units[0];
                                selectedObjectType = Enumerators.AffectObjectType.Character;
                            }
                            else
                                return;
                        }
                        else
                            return;
                    }

                    break;
                case Enumerators.OverlordSkill.RABIES:
                    {
                        _unitsToIgnoreThisTurn =
                            _gameplayManager.OpponentPlayer.BoardCards.FindAll(x =>
                            skill.Skill.ElementTargetTypes.Count > 0 && !x.Model.IsDead &&
                            !skill.Skill.ElementTargetTypes.Contains(x.Model.Card.Prototype.CardSetType) ||
                            x.Model.NumTurnsOnBoard > 0 || x.Model.HasFeral)
                                .Select(x => x.Model)
                                .ToList();
                        BoardUnitModel unit = GetRandomUnit(false, _unitsToIgnoreThisTurn);

                        if (unit != null)
                        {
                            target = unit;
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }

                    break;
                case Enumerators.OverlordSkill.POISON_DART:
                case Enumerators.OverlordSkill.ICE_BOLT:
                case Enumerators.OverlordSkill.FREEZE:
                case Enumerators.OverlordSkill.FIRE_BOLT:
                case Enumerators.OverlordSkill.FIREBALL:
                    {
                        target = _gameplayManager.CurrentPlayer;
                        selectedObjectType = Enumerators.AffectObjectType.Player;

                        BoardUnitModel unit = GetRandomOpponentUnit(GetOpponentUnitsOnBoard().FindAll(model => skill.BlockedUnitStatusTypes.Contains(model.UnitStatus)
                        && !model.IsDead));

                        if (unit != null)
                        {
                            target = unit;
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }

                    break;
                case Enumerators.OverlordSkill.PUSH:
                    {
                        if (skill.Skill.ElementTargetTypes.Count > 0)
                        {
                            _unitsToIgnoreThisTurn =
                                _gameplayManager.OpponentPlayer.BoardCards
                                    .FindAll(x => !skill.Skill.ElementTargetTypes.Contains(x.Model.Card.Prototype.CardSetType) && !x.Model.IsDead)
                                    .Select(x => x.Model)
                                    .ToList();
                        }

                        List<BoardUnitModel> units = GetUnitsWithLowHp(_unitsToIgnoreThisTurn);

                        if (units.Count > 0)
                        {
                            target = units[0];

                            _unitsToIgnoreThisTurn.Add((BoardUnitModel)target);

                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                        {
                            BoardUnitModel unit = GetRandomOpponentUnit(_unitsToIgnoreThisTurn);

                            if (unit != null)
                            {
                                target = unit;

                                _unitsToIgnoreThisTurn.Add((BoardUnitModel)target);

                                selectedObjectType = Enumerators.AffectObjectType.Character;
                            }
                            else
                                return;
                        }
                    }

                    break;

                case Enumerators.OverlordSkill.SHATTER:
                    {
                        List<BoardUnitModel> units = _gameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.Model.IsStun && !x.Model.IsDead).Select(x => x.Model).ToList();

                        if (units.Count > 0)
                        {
                            target = units[0];
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                    break;
                case Enumerators.OverlordSkill.PHALANX:
                case Enumerators.OverlordSkill.FORTRESS:
                case Enumerators.OverlordSkill.MASS_RABIES:
                case Enumerators.OverlordSkill.METEOR_SHOWER:
                case Enumerators.OverlordSkill.BLIZZARD:
                case Enumerators.OverlordSkill.LEVITATE:
                case Enumerators.OverlordSkill.RETREAT:
                case Enumerators.OverlordSkill.BREAKOUT:
                case Enumerators.OverlordSkill.EPIDEMIC:
                case Enumerators.OverlordSkill.RESSURECT:
                case Enumerators.OverlordSkill.REANIMATE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skill.Skill.OverlordSkill), skill.Skill.OverlordSkill, null);
            }

            skill.StartDoSkill();

            Action callback = () =>
            {
                switch (selectedObjectType)
                {
                    case Enumerators.AffectObjectType.Player:
                        skill.FightTargetingArrow.SelectedPlayer = (Player) target;
                        break;
                    case Enumerators.AffectObjectType.Character:
                        BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel((BoardUnitModel) target);
                        skill.FightTargetingArrow.SelectedCard = selectedCardView;
                        break;
                    case Enumerators.AffectObjectType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(selectedObjectType), selectedObjectType, null);
                }

                skill.EndDoSkill(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(target)
                }, true);
            };

            if ((skill.Skill.CanSelectTarget && target != null) ||
                selectedObjectType == Enumerators.AffectObjectType.Player ||
                selectedObjectType == Enumerators.AffectObjectType.Character)
            {
                skill.FightTargetingArrow = _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(skill.SelfObject.transform, target, action: callback);
            }
            else
            {
                callback.Invoke();
            }
        }
    }
}
