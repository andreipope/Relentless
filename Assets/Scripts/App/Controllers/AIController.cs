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

        public BoardCardView CurrentItemCard;

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

        private List<BoardUnitModel> _normalUnitCardInHand, _normalItemCardInHand;

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

            _normalUnitCardInHand = new List<BoardUnitModel>();
            _normalItemCardInHand = new List<BoardUnitModel>();

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
                        workingDeck.Add(_cardsController.CreateWorkingCardFromCardName(card.CardName, _gameplayManager.OpponentPlayer));
                    }
                }

                _gameplayManager.OpponentPlayer.PlayerCardsController.SetCardsInDeck(workingDeck.Select(x => new BoardUnitModel(x)).ToArray());
                _battlegroundController.UpdatePositionOfCardsInOpponentHand();
                Resources.UnloadUnusedAssets();
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

            if (_gameplayManager.OpponentPlayer.SelfOverlord.Faction == Enumerators.Faction.FIRE)
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

            if (_gameplayManager.OpponentPlayer.SelfOverlord.Faction == Enumerators.Faction.FIRE)
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

                    BoardUnitModel card = _gameplayManager.OpponentPlayer.CardsInHand.FirstOrDefault(
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

            List<BoardUnitModel> cardsInHand = new List<BoardUnitModel>();
            cardsInHand.AddRange(_normalUnitCardInHand);

            bool wasAction = false;
            foreach (BoardUnitModel card in cardsInHand)
            {
                if (_gameplayManager.OpponentPlayer.CardsOnBoard.Count >= _gameplayManager.OpponentPlayer.MaxCardsInPlay)
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

            foreach (BoardUnitModel card in _normalItemCardInHand)
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
                    BoardObject target;

                    if (frame.Target == Enumerators.SkillTarget.OPPONENT_CARD)
                    {
                        target = GetOpponentUnitsOnBoard().Find(targetUnit => targetUnit.Card.Prototype.Name.ToLowerInvariant() ==
                                                                     _tutorialManager.GetCardNameByTutorialObjectId(frame.TargetTutorialObjectId).
                                                                     ToLowerInvariant() &&
                                                                     targetUnit.CurrentDefense > 0);
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
                        !unit.AttackTargetsAvailability.Contains(Enumerators.SkillTarget.OPPONENT_CARD))
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

                    if (!unit.AttackTargetsAvailability.Contains(Enumerators.SkillTarget.OPPONENT))
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
                            unit.AttackTargetsAvailability.Contains(Enumerators.SkillTarget.OPPONENT))
                        { 
                            unit.DoCombat(_gameplayManager.CurrentPlayer);
                            await LetsThink(cancellationToken);
                        }
                        else
                        {
                            BoardUnitModel attackedCreature = GetRandomOpponentUnit();

                            if (attackedCreature != null && unit.AttackTargetsAvailability.Contains(Enumerators.SkillTarget.OPPONENT_CARD))
                            {
                                unit.DoCombat(attackedCreature);
                                await LetsThink(cancellationToken);
                            }
                            else
                            {
                                if (unit.HasBuffRush)
                                    break;

                                if (!unit.AttackTargetsAvailability.Contains(Enumerators.SkillTarget.OPPONENT))
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
            List<BoardUnitModel> overflowGooCards = new List<BoardUnitModel>();
            List<BoardUnitModel> cards = new List<BoardUnitModel>();
            cards.AddRange(GetUnitCardsInHand());
            cards.AddRange(GetItemCardsInHand());
            cards.RemoveAll(x => x?.Prototype == null);
            cards  = cards.FindAll(x => CardBePlayableForOverflowGoo(x.Prototype.Cost, gooAmount));
            AbilityData overflowGooAbility;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].InstanceCard.Abilities != null)
                {
                    AbilityData attackOverlordAbility = cards[i].InstanceCard.Abilities
                        .FirstOrDefault(x => x.Ability == Enumerators.AbilityType.ATTACK_OVERLORD);
                    if (attackOverlordAbility != null)
                    {
                        if (attackOverlordAbility.Value * 2 >= _gameplayManager.OpponentPlayer.Defense)
                            break;
                    }

                    overflowGooAbility = cards[i].InstanceCard.Abilities
                        .FirstOrDefault(x => x.Ability == Enumerators.AbilityType.OVERFLOW_GOO);
                    if (overflowGooAbility != null)
                    {
                        if (_gameplayManager.OpponentPlayer.CardsOnBoard.Count + boardCount < _gameplayManager.OpponentPlayer.MaxCardsInPlay - 1)
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

            BoardUnitModel expensiveCard =
                GetUnitCardsInHand()
                    .Find(
                        x => x.CurrentCost > _gameplayManager.OpponentPlayer.CurrentGoo &&
                            x.CurrentCost <= _gameplayManager.OpponentPlayer.CurrentGoo + benefit);
            if (expensiveCard != null)
            {
                bool wasAction = false;
                foreach (BoardUnitModel card in overflowGooCards)
                {
                    if (_gameplayManager.OpponentPlayer.CardsOnBoard.Count >= _gameplayManager.OpponentPlayer.MaxCardsInPlay)
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
                    x.Prototype.Abilities.Any(z => z.Ability == Enumerators.AbilityType.OVERFLOW_GOO));
                _normalItemCardInHand.Clear();
                _normalItemCardInHand.AddRange(GetItemCardsInHand());
                _normalItemCardInHand.RemoveAll(x =>
                    x.InstanceCard.Abilities.Any(z => z.Ability == Enumerators.AbilityType.OVERFLOW_GOO));
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

        private bool CardCanBePlayable(BoardUnitModel boardUnitModel)
        {
            if (!Constants.DevModeEnabled)
            {
                return boardUnitModel.CurrentCost <= _gameplayManager.OpponentPlayer.CurrentGoo &&
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

        private bool CheckSpecialCardRules(BoardUnitModel boardUnitModel)
        {
            if (boardUnitModel.InstanceCard.Abilities != null)
            {
                foreach (AbilityData ability in boardUnitModel.InstanceCard.Abilities)
                {
                    if (ability.Ability == Enumerators.AbilityType.ATTACK_OVERLORD)
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

        public void PlayCardOnBoard(BoardUnitModel boardUnitModel, bool ignorePlayAbility = false, PlayCardActionInfo playCardActionInfo = null)
        {
            _actionsQueueController.AddNewActionInToQueue((parameter, completeCallback) =>
            {
                if(!CardCanBePlayable(boardUnitModel) && !ignorePlayAbility)
                {
                    completeCallback?.Invoke();
                    return;
                }

                bool needTargetForAbility = false;

                if (boardUnitModel.Card.InstanceCard.Abilities != null && boardUnitModel.Card.InstanceCard.Abilities.Count > 0)
                {
                    List<AbilityData> abilitiesWithTargets = boardUnitModel.Card.InstanceCard.Abilities.FindAll(x => x.Targets.Count > 0);

                    if (abilitiesWithTargets.Count > 0)
                    {
                        foreach(AbilityData data in abilitiesWithTargets)
                        {
                            if (data.Trigger == Enumerators.AbilityTrigger.ENTRY &&
                                data.Activity == Enumerators.AbilityActivity.ACTIVE)
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
                            switch (playCardActionInfo.Target)
                            {
                                case Enumerators.SkillTarget.OPPONENT:
                                    target = _gameplayManager.CurrentPlayer;
                                    break;
                                case Enumerators.SkillTarget.OPPONENT_CARD:
                                    target = GetOpponentUnitsOnBoard().Find(x => x.Card.Prototype.Name.ToLowerInvariant() ==
                                                                            _tutorialManager.GetCardNameByTutorialObjectId(playCardActionInfo.TargetTutorialObjectId)
                                                                            .ToLowerInvariant());
                                    break;
                            }
                        }
                    }
                    else
                    {
                        target = GetAbilityTarget(boardUnitModel);
                    }
                }
                switch (boardUnitModel.Card.Prototype.Kind)
                {
                    case Enumerators.CardKind.CREATURE when _gameplayManager.OpponentPlayer.CardsOnBoard.Count < _gameplayManager.OpponentPlayer.MaxCardsInPlay:
                        _gameplayManager.OpponentPlayer.PlayerCardsController.RemoveCardFromHand(boardUnitModel);
                        

                        _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, boardUnitModel.InstanceId, target, null, (x, y) =>
                        {
                            PlayCardCompleteHandler(x, y, completeCallback);
                        });

                        _cardsController.DrawCardInfo(boardUnitModel);
                        break;
                    case Enumerators.CardKind.ITEM:
                        {
                            if ((target != null && needTargetForAbility) || !needTargetForAbility)
                            {
                                _gameplayManager.OpponentPlayer.PlayerCardsController.RemoveCardFromHand(boardUnitModel);

                                _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, boardUnitModel.InstanceId, target, null, (x, y) =>
                                {
                                    PlayCardCompleteHandler(x, y, completeCallback);
                                });
                                _cardsController.DrawCardInfo(boardUnitModel);
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

        private void PlayCardCompleteHandler(BoardUnitModel boardUnitModel, BoardObject target, Action completeCallback)
        {
            completeCallback?.Invoke();

            if ( boardUnitModel == null)
                return;

            GameplayQueueAction<object> callAbilityAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsage, blockQueue: true);
            GameplayQueueAction<object> ranksBuffAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.RankBuff);

            _gameplayManager.OpponentPlayer.CurrentGoo -= boardUnitModel.CurrentCost;

            switch (boardUnitModel.Prototype.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    {
                        BoardUnitView boardUnitViewElement = new BoardUnitView(boardUnitModel, GameObject.Find("OpponentBoard").transform);
                        GameObject boardUnit = boardUnitViewElement.GameObject;
                        boardUnit.tag = SRTags.OpponentOwned;
                        boardUnit.transform.position = Vector3.up * 2f; // Start pos before moving cards to the opponents board
                        _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardToBoard(boardUnitModel, ItemPosition.End);
                        _battlegroundController.RegisterBoardUnitView(_gameplayManager.OpponentPlayer, boardUnitViewElement, ItemPosition.End);
                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = boardUnitViewElement.Model,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });

                        if (Constants.RankSystemEnabled)
                            _gameplayManager.GetController<RanksController>().UpdateRanksByElements(boardUnitModel.Owner.CardsOnBoard, boardUnitModel, ranksBuffAction);

                        _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitViewElement.Model, false);

                        _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.OpponentPlayer,
                            () =>
                            {
                                bool createTargetArrow = false;

                                if (boardUnitModel.InstanceCard.Abilities != null && boardUnitModel.InstanceCard.Abilities.Count > 0)
                                {
                                    createTargetArrow =
                                        _abilitiesController.IsAbilityCanActivateTargetAtStart(
                                            boardUnitModel.InstanceCard.Abilities[0]);
                                }

                                if (target != null)
                                {
                                    if (!_abilitiesController.CheckAbilityOnTarget(boardUnitModel))
                                    {
                                        Action callback = () =>
                                        {
                                            _abilitiesController.CallAbility(null, boardUnitModel, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model,
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
                                        _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitModel);
                                        _actionsQueueController.ForceContinueAction(callAbilityAction);
                                    }
                                }
                                else
                                {
                                    _abilitiesController.CallAbility(null, boardUnitModel,
                                        Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, callAbilityAction);

                                    _actionsQueueController.ForceContinueAction(callAbilityAction);
                                }
                            });
                        boardUnitViewElement.PlayArrivalAnimation(playUniqueAnimation: true);
                    }
                    break;

                case Enumerators.CardKind.ITEM:
                    {
                        GameObject itemCard = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                        itemCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;

                        CurrentItemCard = new ItemBoardCard(itemCard, boardUnitModel);
                        CurrentItemCard.SetHighlightingEnabled(false);

                        BoardItem boardItem = new BoardItem(itemCard, boardUnitModel);

                        itemCard.gameObject.SetActive(false);

                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = boardItem,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });

                        bool createTargetArrow = false;

                        if (boardUnitModel.InstanceCard.Abilities != null && boardUnitModel.InstanceCard.Abilities.Count > 0)
                        {
                            createTargetArrow =
                                _abilitiesController.IsAbilityCanActivateTargetAtStart(boardUnitModel.InstanceCard.Abilities[0]);
                        }

                        if (target != null)
                        {
                            if (!_abilitiesController.CheckAbilityOnTarget(boardUnitModel))
                            {
                                Action callback = () =>
                                {
                                    _abilitiesController.CallAbility(null, boardUnitModel, Enumerators.CardKind.ITEM, boardItem, null, false, null, callAbilityAction, target);
                                    _actionsQueueController.ForceContinueAction(callAbilityAction);
                                };

                                _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(_gameplayManager.OpponentPlayer.AvatarObject.transform, target, action: callback);
                            }
                            else
                            {
                                _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitModel);
                                _actionsQueueController.ForceContinueAction(callAbilityAction);
                            }
                        }
                        else
                        {
                            _abilitiesController.CallAbility(null, boardUnitModel, Enumerators.CardKind.ITEM, boardItem, null, false, null, callAbilityAction);

                            _actionsQueueController.ForceContinueAction(callAbilityAction);
                            ranksBuffAction.ForceActionDone();
                        }
                    }
                    break;
            }
        }

        private BoardObject GetAbilityTarget(BoardUnitModel boardUnitModel)
        {
            IReadOnlyCard prototype = boardUnitModel.Prototype;
            IReadOnlyCardInstanceSpecificData instance = boardUnitModel.InstanceCard;

            BoardObject target = null;

            List<AbilityData> abilitiesWithTarget = new List<AbilityData>();

            bool needsToSelectTarget = false;
            foreach (AbilityData ability in instance.Abilities)
            {
                foreach (Enumerators.Target item in ability.Targets)
                {
                    switch (item)
                    {
                        case Enumerators.Target.OPPONENT_CARD:
                            if (_gameplayManager.CurrentPlayer.CardsOnBoard.Count > 1 ||
                                ability.Ability == Enumerators.AbilityType.CARD_RETURN &&
                                _gameplayManager.CurrentPlayer.CardsOnBoard.Count > 0)
                            {
                                needsToSelectTarget = true;
                                abilitiesWithTarget.Add(ability);
                            }

                            break;
                        case Enumerators.Target.PLAYER_CARD:
                            if (_gameplayManager.OpponentPlayer.CardsOnBoard.Count > 1 ||
                                prototype.Kind == Enumerators.CardKind.ITEM ||
                                ability.Ability == Enumerators.AbilityType.CARD_RETURN &&
                                _gameplayManager.OpponentPlayer.CardsOnBoard.Count > 0)
                            {
                                needsToSelectTarget = true;
                                abilitiesWithTarget.Add(ability);
                            }

                            break;
                        case Enumerators.Target.PLAYER:
                        case Enumerators.Target.OPPONENT:
                        case Enumerators.Target.ALL:
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
                switch (ability.Ability)
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
            if (ability.Targets.Contains(Enumerators.Target.OPPONENT_CARD))
            {
                AddRandomTargetUnit(true, ref target);
            }
            else if (ability.Targets.Contains(Enumerators.Target.OPPONENT))
            {
                target = _gameplayManager.CurrentPlayer;
            }
        }

        private void GetTargetByType(AbilityData ability, ref BoardObject target, bool checkPlayerAlso)
        {
            if (ability.Targets.Contains(Enumerators.Target.OPPONENT_CARD))
            {
                IReadOnlyList<BoardUnitModel> targets = GetHeavyUnitsOnBoard(_gameplayManager.CurrentPlayer);

                if (targets.Count > 0)
                {
                    target = targets[UnityEngine.Random.Range(0, targets.Count)];
                }

                if (checkPlayerAlso && target == null &&
                    ability.Targets.Contains(Enumerators.Target.PLAYER_CARD))
                {
                    target = _gameplayManager.CurrentPlayer;

                    targets = GetHeavyUnitsOnBoard(_gameplayManager.OpponentPlayer);

                    if (targets.Count > 0)
                    {
                        target = targets[UnityEngine.Random.Range(0, targets.Count)];
                    }
                }
            }
        }

        private IReadOnlyList<BoardUnitModel> GetHeavyUnitsOnBoard(Player player)
        {
            return player.CardsOnBoard.FindAll(x => x.HasHeavy || x.HasBuffHeavy);
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
            foreach (BoardUnitModel creature in _gameplayManager.OpponentPlayer.CardsOnBoard)
            {
                if (creature.CurrentDefense > 0 && (creature.NumTurnsOnBoard >= 1 || creature.HasFeral))
                {
                    power += creature.CurrentDamage;
                }
            }

            return power;
        }

        private int GetOpponentAttackingValue()
        {
            int power = 0;
            foreach (BoardUnitModel card in _gameplayManager.CurrentPlayer.CardsOnBoard)
            {
                power += card.CurrentDamage;
            }

            return power;
        }

        private List<BoardUnitModel> GetUnitsWithLowHp(List<BoardUnitModel> unitsToIgnore = null)
        {
            List<BoardUnitModel> finalList = new List<BoardUnitModel>();

            List<BoardUnitModel> list = GetUnitsOnBoard();

            foreach (BoardUnitModel item in list)
            {
                if (item.CurrentDefense < item.MaxCurrentDefense && item.CurrentDefense > 0 && !item.IsDead)
                {
                    finalList.Add(item);
                }
            }

            if (unitsToIgnore != null)
            {
                finalList = finalList.FindAll(x => !unitsToIgnore.Contains(x));
            }

            finalList = finalList.OrderBy(x => x.CurrentDefense).ThenBy(y => y.CurrentDefense.ToString().Length).ToList();

            return finalList;
        }

        private List<BoardUnitModel> GetUnitCardsInHand()
        {
            IReadOnlyList<BoardUnitModel> list =
                _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x =>
                    x.Prototype.Kind == Enumerators.CardKind.CREATURE);

            List<Card> cards = new List<Card>();

            foreach (BoardUnitModel item in list)
            {
                cards.Add(_dataManager.CachedCardsLibraryData.GetCardFromName(item.Prototype.Name));
            }

            cards = cards.OrderBy(x => x.Cost).ThenBy(y => y.Cost.ToString().Length).ToList();

            List<BoardUnitModel> sortedList = new List<BoardUnitModel>();

            cards.Reverse();

            foreach (Card item in cards)
            {
                sortedList.Add(list.First(x => x.Prototype.MouldId == item.MouldId && !sortedList.Contains(x)));
            }

            return sortedList;
        }

        private IReadOnlyList<BoardUnitModel> GetItemCardsInHand()
        {
            return _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x =>
                x.Prototype.Kind == Enumerators.CardKind.ITEM);
        }

        private List<BoardUnitModel> GetUnitsOnBoard()
        {
            return
                _gameplayManager.OpponentPlayer.CardsOnBoard
                    .FindAll(x => x.CurrentDefense > 0 && !x.IsDead)
                    .ToList();
        }

        private BoardUnitModel GetRandomUnit(bool lowHp = false, List<BoardUnitModel> unitsToIgnore = null)
        {
            List<BoardUnitModel> eligibleUnits;

            if (!lowHp)
            {
                eligibleUnits =
                    _gameplayManager.OpponentPlayer.CardsOnBoard
                        .FindAll(x => x.CurrentDefense > 0 && !_attackedUnitTargets.Contains(x) && !x.IsDead)
                        .ToList();
            }
            else
            {
                eligibleUnits =
                    _gameplayManager.OpponentPlayer.CardsOnBoard
                        .FindAll(x => x.CurrentDefense < x.MaxCurrentDefense && x.CurrentDefense > 0 && !_attackedUnitTargets.Contains(x) && !x.IsDead)
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
                _gameplayManager.CurrentPlayer.CardsOnBoard
                    .FindAll(x => x.CurrentDefense > 0 && !x.IsDead)
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
        return _gameplayManager.CurrentPlayer.CardsOnBoard
                    .FindAll(x => x.CurrentDefense > 0 && !x.IsDead)
                    .ToList();
        }

        private BoardUnitModel GetRandomOpponentUnit(List<BoardUnitModel> unitsToIgnore = null)
        {
            List<BoardUnitModel> eligibleCreatures =
                _gameplayManager.CurrentPlayer.CardsOnBoard
                    .Where(x => x.CurrentDefense > 0 && !_attackedUnitTargets.Contains(x) && !x.IsDead)
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
            List<BoardUnitModel> eligibleCreatures = _gameplayManager.CurrentPlayer.CardsOnBoard.FindAll(x => x.CurrentDefense > 0 && !x.IsDead);
            if (eligibleCreatures.Count > 0)
            {
                List<BoardUnitModel> provokeCreatures = eligibleCreatures.FindAll(x => x.IsHeavyUnit);
                return provokeCreatures.Count >= 1;
            }

            return false;
        }

        private void DoBoardSkill(BoardSkill skill)
        {
            BoardObject target = null;

            Enumerators.AffectObjectType selectedObjectType = Enumerators.AffectObjectType.None;

            switch (skill.Skill.Skill)
            {
                case Enumerators.Skill.HARDEN:
                case Enumerators.Skill.DRAW:
                    target = _gameplayManager.OpponentPlayer;
                    break;
                case Enumerators.Skill.STONE_SKIN:
                case Enumerators.Skill.FORTIFY:
                case Enumerators.Skill.WIND_SHIELD:
                case Enumerators.Skill.INFECT:
                case Enumerators.Skill.TOXIC_POWER:
                    {
                        List<BoardUnitModel> units = GetUnitsOnBoard().FindAll(x => x.Card.Prototype.Faction ==
                                                                        _gameplayManager.OpponentPlayer.SelfOverlord.Faction);
                        if (units.Count > 0)
                        {
                            target = units[UnityEngine.Random.Range(0, units.Count)];
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                    break;
                case Enumerators.Skill.HEALING_TOUCH:
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
                case Enumerators.Skill.ICE_WALL:
                case Enumerators.Skill.ENHANCE:
                    {
                        List<BoardUnitModel> units = GetUnitsOnBoard().FindAll(x => x.Card.Prototype.Faction ==
                                                                        _gameplayManager.OpponentPlayer.SelfOverlord.Faction);

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
                case Enumerators.Skill.MEND:
                    {
                        target = _gameplayManager.OpponentPlayer;
                        selectedObjectType = Enumerators.AffectObjectType.Player;

                        if (_gameplayManager.OpponentPlayer.Defense > 13)
                        {
                            if (skill.Skill.TargetFactions.Count > 0)
                            {
                                _unitsToIgnoreThisTurn =
                                    _gameplayManager.OpponentPlayer.CardsOnBoard
                                    .FindAll(x => !skill.Skill.TargetFactions.Contains(x.Card.Prototype.Faction) && !x.IsDead)
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
                case Enumerators.Skill.RABIES:
                    {
                        _unitsToIgnoreThisTurn =
                            _gameplayManager.OpponentPlayer.CardsOnBoard.FindAll(x =>
                                    skill.Skill.TargetFactions.Count > 0 &&
                                    !x.IsDead &&
                                    !skill.Skill.TargetFactions.Contains(x.Card.Prototype.Faction) ||
                                    x.NumTurnsOnBoard > 0 ||
                                    x.HasFeral)
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
                case Enumerators.Skill.POISON_DART:
                case Enumerators.Skill.ICE_BOLT:
                case Enumerators.Skill.FREEZE:
                case Enumerators.Skill.FIRE_BOLT:
                case Enumerators.Skill.FIREBALL:
                    {
                        target = _gameplayManager.CurrentPlayer;
                        selectedObjectType = Enumerators.AffectObjectType.Player;

                        BoardUnitModel unit = GetRandomOpponentUnit(GetOpponentUnitsOnBoard().FindAll(model => skill.BlockedUnitStatusTypes.Contains(model.UnitSpecialStatus)
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
                case Enumerators.Skill.PUSH:
                    {
                        if (skill.Skill.TargetFactions.Count > 0)
                        {
                            _unitsToIgnoreThisTurn =
                                _gameplayManager.OpponentPlayer.CardsOnBoard
                                    .FindAll(x => !skill.Skill.TargetFactions.Contains(x.Card.Prototype.Faction) && !x.IsDead)
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

                case Enumerators.Skill.SHATTER:
                    {
                        List<BoardUnitModel> units = _gameplayManager.CurrentPlayer.CardsOnBoard.FindAll(x => x.IsStun && !x.IsDead).ToList();

                        if (units.Count > 0)
                        {
                            target = units[0];
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                    break;
                case Enumerators.Skill.PHALANX:
                case Enumerators.Skill.FORTRESS:
                case Enumerators.Skill.MASS_RABIES:
                case Enumerators.Skill.METEOR_SHOWER:
                case Enumerators.Skill.BLIZZARD:
                case Enumerators.Skill.LEVITATE:
                case Enumerators.Skill.RETREAT:
                case Enumerators.Skill.BREAKOUT:
                case Enumerators.Skill.EPIDEMIC:
                case Enumerators.Skill.RESSURECT:
                case Enumerators.Skill.REANIMATE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skill.Skill.Skill), skill.Skill.Skill, null);
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
                        BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>((BoardUnitModel) target);
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
