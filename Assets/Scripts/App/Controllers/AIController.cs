using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Loom.ZombieBattleground
{
    public class AIController : IController
    {
        public BoardCard CurrentSpellCard;

        private const int MinTurnForAttack = 0;

        private readonly Random _random = new Random();

        private IGameplayManager _gameplayManager;

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private ILoadObjectsManager _loadObjectsManager;

        private BattlegroundController _battlegroundController;

        private CardsController _cardsController;

        private ActionsQueueController _actionsQueueController;

        private AbilitiesController _abilitiesController;

        private SkillsController _skillsController;

        private BoardArrowController _boardArrowController;

        private Enumerators.AiType _aiType;

        private List<BoardUnitModel> _attackedUnitTargets;

        private List<BoardUnitModel> _unitsToIgnoreThisTurn;

        private List<WorkingCard> _normalUnitCardInHand, _normalSpellCardInHand;

        private GameObject _fightTargetingArrowPrefab; // rewrite

        private CancellationTokenSource _aiBrainCancellationTokenSource;

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

        public void InitializePlayer(int playerId)
        {
            _gameplayManager.OpponentPlayer = new Player(playerId, GameObject.Find("Opponent"), true);

            _fightTargetingArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _attackedUnitTargets = new List<BoardUnitModel>();
            _unitsToIgnoreThisTurn = new List<BoardUnitModel>();

            if (!_gameplayManager.IsSpecificGameplayBattleground)
            {
                List<string> playerDeck = new List<string>();

                int deckId = _gameplayManager.OpponentDeckId;
                foreach (DeckCardData card in _dataManager.CachedOpponentDecksData.Decks.First(d => d.Id == deckId)
                    .Cards)
                {
                    for (int i = 0; i < card.Amount; i++)
                    {
                        playerDeck.Add(card.CardName);
                    }
                }

                _gameplayManager.OpponentPlayer.SetDeck(playerDeck, true);

                _battlegroundController.UpdatePositionOfCardsInOpponentHand();
            }

            _gameplayManager.OpponentPlayer.TurnStarted += TurnStartedHandler;
            _gameplayManager.OpponentPlayer.TurnEnded += TurnEndedHandler;
        }

        public async Task LaunchAIBrain()
        {
            _aiBrainCancellationTokenSource = new CancellationTokenSource();
            Debug.Log("brain started");

            try
            {
                await DoAiBrain(_aiBrainCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("brain canceled!");
            }

            Debug.Log("brain finished");
        }

        private void SetAiTypeByDeck()
        {
            OpponentDeck deck =
                _dataManager.CachedOpponentDecksData.Decks.Find(d => d.Id == _gameplayManager.OpponentDeckId);

            if (deck != null)
            {
                SetAiType((Enumerators.AiType)Enum.Parse(typeof(Enumerators.AiType), deck.Type));
            }
            else
            {
                throw new NullReferenceException($"OpponentDeck with id {_gameplayManager.OpponentDeckId} is null!");
            }
        }

        public void SetAiType(Enumerators.AiType aiType)
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

            if (_tutorialManager.IsTutorial && _gameplayManager.IsSpecificGameplayBattleground)
            {
                if (!_tutorialManager.CurrentTutorialDataStep.IsLaunchAIBrain)
                    return;
            }

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

            await PlayCardsFromHand(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (_tutorialManager.IsTutorial && _tutorialManager.CurrentTutorialDataStep.IsPauseTutorial)
            {
                ((TutorialManager) _tutorialManager).Paused = true;
            }
            else
            {
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);

                await UseUnitsOnBoard(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await UsePlayerSkills(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (_gameplayManager.OpponentPlayer.SelfHero.HeroElement == Enumerators.SetType.FIRE)
                {
                    await UseUnitsOnBoard(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    await LetsThink(cancellationToken);
                    await LetsThink(cancellationToken);
                    _battlegroundController.StopTurn();

                }
                else
                {
                    await LetsThink(cancellationToken);
                    await LetsThink(cancellationToken);
                    _battlegroundController.StopTurn();
                }
            }
        }

        // ai step 1
        private async Task PlayCardsFromHand(CancellationToken cancellationToken)
        {
            // CHECK about CardsInHand in modification collection!
            await CheckGooCard(cancellationToken);

            List<WorkingCard> cardsInHand = new List<WorkingCard>();
            cardsInHand.AddRange(_normalUnitCardInHand);

            bool wasAction = false;
            foreach (WorkingCard card in cardsInHand)
            {
                if (_gameplayManager.OpponentPlayer.BoardCards.Count >= Constants.MaxBoardUnits)
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
            List<BoardUnitModel> unitsOnBoard = new List<BoardUnitModel>();
            List<BoardUnitModel> alreadyUsedUnits = new List<BoardUnitModel>();

            unitsOnBoard.AddRange(GetUnitsOnBoard());

            if (OpponentHasHeavyUnits())
            {
                foreach (BoardUnitModel unit in unitsOnBoard)
                {
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
                    }
                }
            }

            foreach (BoardUnitModel creature in alreadyUsedUnits)
            {
                unitsOnBoard.Remove(creature);
            }

            int totalValue = GetPlayerAttackingValue();
            if ((totalValue >= _gameplayManager.OpponentPlayer.Health || _aiType == Enumerators.AiType.BLITZ_AI ||
                _aiType == Enumerators.AiType.TIME_BLITZ_AI))
            {
                foreach (BoardUnitModel unit in unitsOnBoard)
                {
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
                    while (UnitCanBeUsable(unit))
                    {
                        if (GetPlayerAttackingValue() > GetOpponentAttackingValue() && !_tutorialManager.IsTutorial)
                        {
                            unit.DoCombat(_gameplayManager.CurrentPlayer);
                            await LetsThink(cancellationToken);
                        }
                        else
                        {
                            BoardUnitModel attackedCreature = GetRandomOpponentUnit();
                            if (attackedCreature != null)
                            {
                                unit.DoCombat(attackedCreature);
                                await LetsThink(cancellationToken);
                            }
                            else
                            {
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
            if (_gameplayManager.IsTutorial || _gameplayManager.OpponentPlayer.IsStunned)
                return;

            if (_skillsController.OpponentPrimarySkill.IsSkillReady)
            {
                DoBoardSkill(_skillsController.OpponentPrimarySkill);
                wasAction = true;
            }

            if (wasAction)
            {
                await LetsThink(cancellationToken);
            }

            if (_skillsController.OpponentSecondarySkill.IsSkillReady)
            {
                DoBoardSkill(_skillsController.OpponentSecondarySkill);
                wasAction = true;
            }

            if (wasAction)
            {
                await LetsThink(cancellationToken);
                await LetsThink(cancellationToken);
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
            int gooAmount = _gameplayManager.OpponentPlayer.Goo;
            List<WorkingCard> overflowGooCards = new List<WorkingCard>();
            List<WorkingCard> cards = new List<WorkingCard>();
            cards.AddRange(GetUnitCardsInHand());
            cards.AddRange(GetSpellCardsInHand());
            cards = cards.FindAll(x => CardBePlayableForOverflowGoo(x.LibraryCard.Cost, gooAmount));
            AbilityData overflowGooAbility;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].LibraryCard.Abilities != null)
                {
                    AbilityData attackOverlordAbility = cards[i].LibraryCard.Abilities
                        .Find(x => x.AbilityType == Enumerators.AbilityType.ATTACK_OVERLORD);
                    if (attackOverlordAbility != null)
                    {
                        if (attackOverlordAbility.Value * 2 >= _gameplayManager.OpponentPlayer.Health)
                            break;
                    }

                    overflowGooAbility = cards[i].LibraryCard.Abilities
                        .Find(x => x.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO);
                    if (overflowGooAbility != null)
                    {
                        if (_gameplayManager.OpponentPlayer.BoardCards.Count + boardCount < Constants.MaxBoardUnits - 1)
                        {
                            boardCount++;
                            gooAmount -= cards[i].LibraryCard.Cost;
                            benefit += overflowGooAbility.Value - cards[i].LibraryCard.Cost;
                            overflowGooCards.Add(cards[i]);
                            cards = cards.FindAll(x => CardBePlayableForOverflowGoo(x.LibraryCard.Cost, gooAmount));
                        }
                    }
                }
            }

            WorkingCard expensiveCard =
                GetUnitCardsInHand()
                    .Find(
                        x => x.LibraryCard.Cost > _gameplayManager.OpponentPlayer.Goo &&
                            x.LibraryCard.Cost <= _gameplayManager.OpponentPlayer.Goo + benefit);
            if (expensiveCard != null)
            {
                bool wasAction = false;
                foreach (WorkingCard card in overflowGooCards)
                {
                    if (_gameplayManager.OpponentPlayer.BoardCards.Count >= Constants.MaxBoardUnits)
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
                    x.LibraryCard.Abilities.Exists(z => z.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO));
                _normalSpellCardInHand.Clear();
                _normalSpellCardInHand.AddRange(GetSpellCardsInHand());
                _normalSpellCardInHand.RemoveAll(x =>
                    x.LibraryCard.Abilities.Exists(z => z.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO));
            }

            await LetsThink(cancellationToken);
        }

        private bool CardBePlayableForOverflowGoo(int cost, int goo)
        {
#if !DEV_MODE
            return cost <= goo && _gameplayManager.OpponentPlayer.Turn > MinTurnForAttack;
#else
            return true;
#endif
        }

        private bool CardCanBePlayable(WorkingCard card)
        {
#if !DEV_MODE
            return card.LibraryCard.Cost <= _gameplayManager.OpponentPlayer.Goo &&
                _gameplayManager.OpponentPlayer.Turn > MinTurnForAttack;
#else
            return true;
#endif
        }

        private bool UnitCanBeUsable(BoardUnitModel unit)
        {
            return unit.UnitCanBeUsable();
        }

        private bool CheckSpecialCardRules(WorkingCard card)
        {
            if (card.LibraryCard.Abilities != null)
            {
                foreach (AbilityData ability in card.LibraryCard.Abilities)
                {
                    if (ability.AbilityType == Enumerators.AbilityType.ATTACK_OVERLORD)
                    {
                        // smart enough HP to use goo carriers
                        if (ability.Value * 2 >= _gameplayManager.OpponentPlayer.Health)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void PlayCardOnBoard(WorkingCard card)
        {
            bool needTargetForAbility = false;

            if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
            {
                needTargetForAbility =
                    card.LibraryCard.Abilities.FindAll(x => x.AbilityTargetTypes.Count > 0).Count > 0;
            }

            BoardObject target = null;

            if (needTargetForAbility)
            {
                target = GetAbilityTarget(card);
            }

            switch (card.LibraryCard.CardKind)
            {
                case Enumerators.CardKind.CREATURE
                    when _battlegroundController.OpponentBoardCards.Count < Constants.MaxBoardUnits:
                    _gameplayManager.OpponentPlayer.RemoveCardFromHand(card);
                    _gameplayManager.OpponentPlayer.AddCardToBoard(card);

                    _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card, target, PlayCardCompleteHandler);

                    _cardsController.DrawCardInfo(card);
                    break;
                case Enumerators.CardKind.SPELL:
                {
                    if (target != null && needTargetForAbility || !needTargetForAbility)
                    {
                        _gameplayManager.OpponentPlayer.RemoveCardFromHand(card);
                        _gameplayManager.OpponentPlayer.AddCardToBoard(card);

                        _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card, target, PlayCardCompleteHandler);
                        _cardsController.DrawCardInfo(card);
                    }

                    break;
                }
            }

            _gameplayManager.OpponentPlayer.Goo -= card.LibraryCard.Cost;
        }

        private void PlayCardCompleteHandler(WorkingCard card, BoardObject target)
        {
            WorkingCard workingCard = null;

            if (_gameplayManager.OpponentPlayer.CardsOnBoard.Count > 0)
            {
                workingCard = _gameplayManager.OpponentPlayer.CardsOnBoard[_gameplayManager.OpponentPlayer.CardsOnBoard.Count - 1];
            }

            if (workingCard == null || card == null) 
                return;

            switch (card.LibraryCard.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                {
                    BoardUnitView boardUnitViewElement = new BoardUnitView(new BoardUnitModel(), GameObject.Find("OpponentBoard").transform);
                    GameObject boardUnit = boardUnitViewElement.GameObject;
                    boardUnit.tag = SRTags.OpponentOwned;
                    boardUnit.transform.position = Vector3.zero;
                    boardUnitViewElement.Model.OwnerPlayer = card.Owner;

                    boardUnitViewElement.SetObjectInfo(workingCard);
                    _battlegroundController.OpponentBoardCards.Add(boardUnitViewElement);

                    boardUnit.transform.position +=
                        Vector3.up * 2f; // Start pos before moving cards to the opponents board

                    _gameplayManager.OpponentPlayer.BoardCards.Add(boardUnitViewElement);

                    _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                    {
                        ActionType = Enumerators.ActionType.PlayCardFromHand,
                        Caller = boardUnitViewElement.Model,
                        TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    });

                    boardUnitViewElement.PlayArrivalAnimation();

                    _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitViewElement.Model, false);

                    _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent(
                        () =>
                        {
                            bool createTargetArrow = false;

                            if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                            {
                                createTargetArrow =
                                    _abilitiesController.IsAbilityCanActivateTargetAtStart(
                                        card.LibraryCard.Abilities[0]);
                            }

                            if (target != null)
                            {
                                Action callback = () =>
                                {
                                    _abilitiesController.CallAbility(card.LibraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, target);
                                };

                                _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(boardUnit.transform, target, action: callback);
                            }
                            else
                            {
                                _abilitiesController.CallAbility(card.LibraryCard, null, workingCard,
                                    Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null);
                            }
                        });
                    break;
                }
                case Enumerators.CardKind.SPELL:
                {
                    GameObject spellCard = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                    spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;

                    CurrentSpellCard = new SpellBoardCard(spellCard);

                    CurrentSpellCard.Init(workingCard);
                    CurrentSpellCard.SetHighlightingEnabled(false);

                    BoardSpell boardSpell = new BoardSpell(spellCard, workingCard);

                    spellCard.gameObject.SetActive(false);

                    bool createTargetArrow = false;

                    if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                    {
                        createTargetArrow =
                            _abilitiesController.IsAbilityCanActivateTargetAtStart(card.LibraryCard.Abilities[0]);
                    }

                    if (target != null)
                    {
                        Action callback = () =>
                        {
                            _abilitiesController.CallAbility(card.LibraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null, target);
                        };

                        _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(_gameplayManager.OpponentPlayer.AvatarObject.transform, target, action: callback);
                    }
                    else
                    {
                        _abilitiesController.CallAbility(card.LibraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null);
                    }

                    break;
                }
            }
        }

        private BoardObject GetAbilityTarget(WorkingCard card)
        {
            Card libraryCard = card.LibraryCard;

            BoardObject target = null;

            List<AbilityData> abilitiesWithTarget = new List<AbilityData>();

            bool needsToSelectTarget = false;
            foreach (AbilityData ability in libraryCard.Abilities)
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
                                libraryCard.CardKind == Enumerators.CardKind.SPELL ||
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
                        if (!AddRandomTargetUnit(true, ref target))
                        {
                            target = _gameplayManager.CurrentPlayer;
                        }

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
                List<BoardUnitView> targets = GetHeavyUnitsOnBoard(_gameplayManager.CurrentPlayer);

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

        private List<BoardUnitView> GetHeavyUnitsOnBoard(Player player)
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
                if (item.CurrentHp < item.MaxCurrentHp)
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
            List<WorkingCard> list =
                _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x =>
                    x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE);

            List<Card> cards = new List<Card>();

            foreach (WorkingCard item in list)
            {
                cards.Add(_dataManager.CachedCardsLibraryData.GetCard(item.CardId));
            }

            cards = cards.OrderBy(x => x.Cost).ThenBy(y => y.Cost.ToString().Length).ToList();

            List<WorkingCard> sortedList = new List<WorkingCard>();

            cards.Reverse();

            foreach (Card item in cards)
            {
                sortedList.Add(list.Find(x => x.CardId == item.Id && !sortedList.Contains(x)));
            }

            list.Clear();
            cards.Clear();

            return sortedList;
        }

        private List<WorkingCard> GetSpellCardsInHand()
        {
            return _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x =>
                x.LibraryCard.CardKind == Enumerators.CardKind.SPELL);
        }

        private List<BoardUnitModel> GetUnitsOnBoard()
        {
            return
                _gameplayManager.OpponentPlayer.BoardCards
                    .FindAll(x => x.Model.CurrentHp > 0)
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
                        .FindAll(x => x.Model.CurrentHp > 0 && !_attackedUnitTargets.Contains(x.Model))
                        .Select(x => x.Model)
                        .ToList();
            }
            else
            {
                eligibleUnits =
                    _gameplayManager.OpponentPlayer.BoardCards
                        .FindAll(x => x.Model.CurrentHp < x.Model.MaxCurrentHp && !_attackedUnitTargets.Contains(x.Model))
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
                    .FindAll(x => x.Model.CurrentHp > 0)
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

        private BoardUnitModel GetRandomOpponentUnit(List<BoardUnitModel> unitsToIgnore = null)
        {
            List<BoardUnitModel> eligibleCreatures =
                _gameplayManager.CurrentPlayer.BoardCards
                    .Select(x => x.Model)
                    .Where(x => x.CurrentHp > 0 && !_attackedUnitTargets.Contains(x))
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
            List<BoardUnitModel> eligibleCreatures = board.FindAll(x => x.CurrentHp > 0);
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
                case Enumerators.OverlordSkill.STONE_SKIN:
                case Enumerators.OverlordSkill.DRAW:
                {
                    selectedObjectType = Enumerators.AffectObjectType.Player;
                    target = _gameplayManager.OpponentPlayer;
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
                case Enumerators.OverlordSkill.MEND:
                {
                    target = _gameplayManager.OpponentPlayer;
                    selectedObjectType = Enumerators.AffectObjectType.Player;

                    if (_gameplayManager.OpponentPlayer.Health > 13)
                    {
                        if (skill.Skill.ElementTargetTypes.Count > 0)
                        {
                            _unitsToIgnoreThisTurn =
                                _gameplayManager.OpponentPlayer.BoardCards
                                .FindAll(x => !skill.Skill.ElementTargetTypes.Contains(x.Model.Card.LibraryCard.CardSetType))
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
                        skill.Skill.ElementTargetTypes.Count > 0 &&
                        !skill.Skill.ElementTargetTypes.Contains(x.Model.Card.LibraryCard.CardSetType) ||
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
                case Enumerators.OverlordSkill.TOXIC_POWER:
                case Enumerators.OverlordSkill.ICE_BOLT:
                case Enumerators.OverlordSkill.FREEZE:
                case Enumerators.OverlordSkill.FIRE_BOLT:
                {
                    target = _gameplayManager.CurrentPlayer;
                    selectedObjectType = Enumerators.AffectObjectType.Player;

                    BoardUnitModel unit = GetRandomOpponentUnit();

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
                                .FindAll(x => !skill.Skill.ElementTargetTypes.Contains(x.Model.Card.LibraryCard.CardSetType))
                                .Select(x => x.Model)
                                .ToList();
                    }

                    List<BoardUnitModel> units = GetUnitsWithLowHp(_unitsToIgnoreThisTurn);

                    if (units.Count > 0)
                    {
                        target = units[0];

                        _unitsToIgnoreThisTurn.Add((BoardUnitModel) target);

                        selectedObjectType = Enumerators.AffectObjectType.Character;
                    }
                    else
                    {
                        BoardUnitModel unit = GetRandomOpponentUnit(_unitsToIgnoreThisTurn);

                        if (unit != null)
                        {
                            target = unit;

                            _unitsToIgnoreThisTurn.Add((BoardUnitModel) target);

                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                }

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

                skill.EndDoSkill();
            };

            skill.FightTargetingArrow = _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(skill.SelfObject.transform, target, action: callback);
        }
    }
}
