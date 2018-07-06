// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using System.Collections.Generic;
using System.Linq;
using System.Collections;

using UnityEngine;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AIController : IController
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ISoundManager _soundManager;
        private ITutorialManager _tutorialManager;
        private ITimerManager _timerManager;

        private BattlegroundController _battlegroundController;
        private CardsController _cardsController;
        private ActionsQueueController _actionsQueueController;
        private AbilitiesController _abilitiesController;
        private BattleController _battleController;
        private AnimationsController _animationsController;
        private VFXController _vfxController;
        private SkillsController _skillsController;

        private bool _enabledAIBrain = false;
        private int _minTurnForAttack = 0;

        private Enumerators.AIType _aiType;
        private List<ActionItem> _allActions;

        private List<BoardUnit> _attackedUnitTargets;

        private Dictionary<int, int> _unitNumberOfTunrsOnBoard = new Dictionary<int, int>();
        private GameObject fightTargetingArrowPrefab;// rewrite

        private System.Random _random = new System.Random();

        public BoardCard currentSpellCard;

        public bool IsPlayerStunned { get; set; }



        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();

            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;
            _gameplayManager.OnGameStartedEvent += OnGameStartedEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void InitializePlayer()
        {
            _gameplayManager.OpponentPlayer = new Player(GameObject.Find("Opponent"), true);

            fightTargetingArrowPrefab = Resources.Load<GameObject>("Prefabs/Gameplay/OpponentTargetingArrow");

            _attackedUnitTargets = new List<BoardUnit>();

            var playerDeck = new List<int>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add(6);
                playerDeck.Add(7);
                playerDeck.Add(11);
                playerDeck.Add(10);
                playerDeck.Add(10);
            }
            else
            {
                var deckId = _gameplayManager.OpponentDeckId;
                foreach (var card in _dataManager.CachedOpponentDecksData.decks[deckId].cards)
                {
                    for (var i = 0; i < card.amount; i++)
                    {
                        if (Constants.DEV_MODE)
                        {
                           // card.cardId = 1;
                        }
                        playerDeck.Add(card.cardId);
                    }
                }

            }

            _gameplayManager.OpponentPlayer.SetDeck(playerDeck);

            _gameplayManager.OpponentPlayer.SetFirstHand(_gameplayManager.IsTutorial);

            _battlegroundController.UpdatePositionOfCardsInOpponentHand();

            _gameplayManager.OpponentPlayer.OnStartTurnEvent += OnStartTurnEventHandler;
            _gameplayManager.OpponentPlayer.OnEndTurnEvent += OnEndTurnEventHandler;
        }

        private void SetAITypeByDeck()
        {
            var deck = _dataManager.CachedOpponentDecksData.decks[_gameplayManager.OpponentDeckId];
            _aiType = (Enumerators.AIType)System.Enum.Parse(typeof(Enumerators.AIType), deck.type);
        }

        private void FillActions()
        {
            _allActions = new List<ActionItem>();

            var allActionsType = _dataManager.CachedOpponentDecksData.decks[_gameplayManager.OpponentDeckId].opponentActions;
            _allActions = _dataManager.CachedActionsLibraryData.GetActions(allActionsType.ToArray());
        }

        private void OnGameEndedEventHandler(Enumerators.EndGameType obj)
        {
            ThreadTool.Instance.AbortAllThreads(this);
        }

        private void OnGameStartedEventHandler()
        {
            if (!_gameplayManager.IsTutorial)
            {
                _minTurnForAttack = _random.Next(1, 3);
                FillActions();

                SetAITypeByDeck();
            }
        }

        private void OnStartTurnEventHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.OpponentPlayer) || !_gameplayManager.GameStarted)
                return;

            DoAIBrain();
        }

        private void OnEndTurnEventHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.OpponentPlayer))
                return;

            ThreadTool.Instance.AbortAllThreads(this);

            foreach (var card in _gameplayManager.OpponentPlayer.CardsOnBoard)
            {
                if (_unitNumberOfTunrsOnBoard.ContainsKey(card.instanceId))
                    _unitNumberOfTunrsOnBoard[card.instanceId] += 1;
                else
                    _unitNumberOfTunrsOnBoard.Add(card.instanceId, 1);
            }

            _attackedUnitTargets.Clear();
        }

        private void DoAIBrain()
        {
            if (!_enabledAIBrain && Constants.DEV_MODE)
            {
                _timerManager.AddTimer((x) =>
                {
                    _battlegroundController.StopTurn();
                }, null, 2f);
                return;
            }

            _timerManager.AddTimer((x) =>
            {
                ThreadTool.Instance.StartOneTimeThread(PlayCardsFromHand, () =>
                {
                    if (_tutorialManager.IsTutorial && _tutorialManager.CurrentStep == 11)
                        (_tutorialManager as TutorialManager).paused = true;
                    else
                    {
                        ThreadTool.Instance.StartOneTimeThread(UseUnitsOnBoard, () =>
                        {
                            ThreadTool.Instance.StartOneTimeThread(UsePlayerSkills, () =>
                            {
                                _battlegroundController.StopTurn();
                            }, this);
                        }, this);
                    }
                }, this);

            }, null, 2f);
        }
        // ai step 1
        private void PlayCardsFromHand()
        {
            foreach (var unit in GetUnitCardsInHand())
            {
                if (CardCanBePlayable(unit))
                {
                    ThreadTool.Instance.RunInMainThread(() => { PlayCardOnBoard(unit); });
                    System.Threading.Thread.Sleep(Constants.DELAY_BETWEEN_AI_ACTIONS);
                }

                if (Constants.DEV_MODE)
                    break;
            }

            foreach (var spell in GetSpellCardsInHand())
            {
                if (CardCanBePlayable(spell))
                {
                    ThreadTool.Instance.RunInMainThread(() => { PlayCardOnBoard(spell); });
                    System.Threading.Thread.Sleep(Constants.DELAY_BETWEEN_AI_ACTIONS);
                }

                if (Constants.DEV_MODE)
                    break;
            }

            LetsThink();
        }
        // ai step 2
        private void UseUnitsOnBoard()
        {
            var unitsOnBoard = new List<BoardUnit>();
            var alreadyUsedUnits = new List<BoardUnit>();

            unitsOnBoard.AddRange(GetUnitsOnBoard());

            if (OpponentHasHeavyUnits())
            {
                foreach (var unit in unitsOnBoard)
                {
                    if (UnitCanBeUsable(unit))
                    {
                        var attackedUnit = GetTargetOpponentUnit();
                        if (attackedUnit != null)
                        {
                            ThreadTool.Instance.RunInMainThread(() => { unit.DoCombat(attackedUnit); });

                            alreadyUsedUnits.Add(unit);

                            LetsThink();

                            if (!OpponentHasHeavyUnits())
                                break;
                        }
                    }
                }
            }

            foreach (var creature in alreadyUsedUnits)
                unitsOnBoard.Remove(creature);

            var totalValue = GetPlayerAttackingValue();
            if ((totalValue >= _gameplayManager.CurrentPlayer.HP ||
                (_aiType == Enumerators.AIType.BLITZ_AI ||
                 _aiType == Enumerators.AIType.TIME_BLITZ_AI)) && !_tutorialManager.IsTutorial)
            {
                foreach (var unit in unitsOnBoard)
                {
                    if (UnitCanBeUsable(unit))
                    {
                        ThreadTool.Instance.RunInMainThread(() => { unit.DoCombat(_gameplayManager.CurrentPlayer); });
                        LetsThink();
                    }
                }
            }
            else
            {
                foreach (var unit in unitsOnBoard)
                {
                    if (UnitCanBeUsable(unit))
                    {
                        if (GetPlayerAttackingValue() > GetOpponentAttackingValue() && !_tutorialManager.IsTutorial)
                        {
                            ThreadTool.Instance.RunInMainThread(() => { unit.DoCombat(_gameplayManager.CurrentPlayer); });
                            LetsThink();
                        }
                        else
                        {
                            var attackedCreature = GetRandomOpponentUnit();
                            if (attackedCreature != null)
                            {
                                ThreadTool.Instance.RunInMainThread(() => { unit.DoCombat(attackedCreature); });
                                LetsThink();
                            }
                            else
                            {
                                ThreadTool.Instance.RunInMainThread(() => { unit.DoCombat(_gameplayManager.CurrentPlayer); });
                                LetsThink();
                            }
                        }
                    }
                }
            }

            LetsThink();
        }
        // ai step 3
        private void UsePlayerSkills()
        {
            ThreadTool.Instance.RunInMainThread(() => { TryToUseBoardSkill(); });

            LetsThink();
        }

        // some thinking - delay between general actions
        private void LetsThink()
        {      
            System.Threading.Thread.Sleep(Constants.DELAY_BETWEEN_AI_ACTIONS);
        }

        private bool CardCanBePlayable(WorkingCard card)
        {
            return ((card.libraryCard.cost <= _gameplayManager.OpponentPlayer.Mana && _battlegroundController.currentTurn > _minTurnForAttack) || Constants.DEV_MODE);
        }

        private bool UnitCanBeUsable(BoardUnit unit)
        {
            return (unit != null && unit.HP > 0 && (_unitNumberOfTunrsOnBoard[unit.Card.instanceId] >= 1 || unit.Card.type == Enumerators.CardType.FERAL) && unit.IsPlayable);
        }

        private void PlayCardOnBoard(WorkingCard card)
        {
            var target = GetAbilityTarget(card);
            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE && _battlegroundController.opponentBoardCards.Count < Constants.MAX_BOARD_CREATURES)
            {
                //if (libraryCard.abilities.Find(x => x.abilityType == Enumerators.AbilityType.CARD_RETURN) != null)
                //    if (target.Count == 0)
                //        return false;

                _gameplayManager.OpponentPlayer.RemoveCardFromHand(card);
                _gameplayManager.OpponentPlayer.AddCardToBoard(card);

                _unitNumberOfTunrsOnBoard[card.instanceId] = 0;

                _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card, target, PlayCardCompleteHandler);

                _cardsController.DrawCardInfo(card);
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                if (target != null)
                {
                    _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card, target, PlayCardCompleteHandler);
                    _cardsController.DrawCardInfo(card);
                }
            }

            _gameplayManager.OpponentPlayer.Mana -= card.libraryCard.cost;
        }

        private void PlayCardCompleteHandler(WorkingCard card, object target)
        {
            string cardSetName = string.Empty;
            foreach (var cardSet in _dataManager.CachedCardsLibraryData.sets)
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;

            var workingCard = _gameplayManager.OpponentPlayer.CardsOnBoard[_gameplayManager.OpponentPlayer.CardsOnBoard.Count - 1];

            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                var boardUnitElement = new BoardUnit(GameObject.Find("OpponentBoard").transform);
                var boardCreature = boardUnitElement.gameObject;
                boardCreature.tag = Constants.TAG_OPPONENT_OWNED;
                boardUnitElement.ownerPlayer = card.owner;

                boardUnitElement.SetObjectInfo(workingCard, cardSetName);
                _battlegroundController.opponentBoardCards.Add(boardUnitElement);

                boardCreature.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board
                                                                     //PlayArrivalAnimation(boardCreature, libraryCard.cardType);

                _gameplayManager.OpponentPlayer.BoardCards.Add(boardUnitElement);

                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent(() =>
                {
                    bool createTargetArrow = false;

                    if (card.libraryCard.abilities != null && card.libraryCard.abilities.Count > 0)
                        createTargetArrow = _abilitiesController.IsAbilityCanActivateTargetAtStart(card.libraryCard.abilities[0]);

                    if (target != null)
                    {
                        GameObject targetObject = null;

                        if (target is Player)
                            targetObject = (target as Player).AvatarObject;
                        else if (target is BoardUnit)
                            targetObject = (target as BoardUnit).gameObject;

                        CreateOpponentTarget(createTargetArrow, boardCreature.gameObject, targetObject,
                                 () => { _abilitiesController.CallAbility(card.libraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitElement, null, false, null, target); });
                    }
                    else
                    {
                        _abilitiesController.CallAbility(card.libraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitElement, null, false, null);
                    }
                });

                boardUnitElement.PlayArrivalAnimation();

                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                var spellCard = MonoBehaviour.Instantiate(_cardsController.spellCardViewPrefab);
                spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;

                currentSpellCard = new SpellBoardCard(spellCard);

                currentSpellCard.Init(workingCard, cardSetName);
                currentSpellCard.SetHighlightingEnabled(false);

                var boardSpell = new BoardSpell(spellCard);

                spellCard.gameObject.SetActive(false);

                bool createTargetArrow = false;

                if (card.libraryCard.abilities != null && card.libraryCard.abilities.Count > 0)
                    createTargetArrow = _abilitiesController.IsAbilityCanActivateTargetAtStart(card.libraryCard.abilities[0]);

                if (target != null)
                {
                    GameObject targetObject = null;

                    if (target is Player)
                        targetObject = (target as Player).AvatarObject;
                    else if (target is BoardUnit)
                        targetObject = (target as BoardUnit).gameObject;

                    CreateOpponentTarget(createTargetArrow, _gameplayManager.OpponentPlayer.AvatarObject, targetObject,
                        () => { _abilitiesController.CallAbility(card.libraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null, target); });
                }

                else
                {
                    _abilitiesController.CallAbility(card.libraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null);
                }
            }
        }

        private object GetAbilityTarget(WorkingCard card)
        {
            var libraryCard = card.libraryCard;

            object target = null;

            var abilitiesWithTarget = new List<LoomNetwork.CZB.Data.AbilityData>();

            var needsToSelectTarget = false;
            foreach (var ability in libraryCard.abilities)
            {
                foreach (var item in ability.abilityTargetTypes)
                {
                    switch (item)
                    {
                        case Enumerators.AbilityTargetType.OPPONENT_CARD:
                            {
                                if (_gameplayManager.CurrentPlayer.BoardCards.Count > 1
                                     || (ability.abilityType == Enumerators.AbilityType.CARD_RETURN && _gameplayManager.CurrentPlayer.BoardCards.Count > 0))
                                {
                                    needsToSelectTarget = true;
                                    abilitiesWithTarget.Add(ability);
                                }
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            {
                                if (_gameplayManager.CurrentPlayer.BoardCards.Count > 1 || (Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.SPELL
                                    || (ability.abilityType == Enumerators.AbilityType.CARD_RETURN && _gameplayManager.CurrentPlayer.BoardCards.Count > 0))
                                {
                                    needsToSelectTarget = true;
                                    abilitiesWithTarget.Add(ability);
                                }
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER:
                        case Enumerators.AbilityTargetType.OPPONENT:
                        case Enumerators.AbilityTargetType.ALL:
                            {
                                needsToSelectTarget = true;
                                abilitiesWithTarget.Add(ability);
                            }
                            break;
                        default: break;
                    }
                }
            }

            if (needsToSelectTarget)
            {
                foreach (var ability in abilitiesWithTarget)
                {
                    switch (ability.abilityType)
                    {
                        case Enumerators.AbilityType.ADD_GOO_VIAL:
                            {
                                target = _gameplayManager.OpponentPlayer;
                            }
                            break;
                        case Enumerators.AbilityType.CARD_RETURN:
                            {
                                if (!AddRandomTargetUnit(true, ref target, false, true))
                                {
                                    AddRandomTargetUnit(false, ref target, true, true);
                                }
                            }
                            break;
                        case Enumerators.AbilityType.DAMAGE_TARGET:
                            {
                                CheckAndAddTargets(ability, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.DAMAGE_TARGET_ADJUSTMENTS:
                            {
                                if (!AddRandomTargetUnit(true, ref target))
                                    target = _gameplayManager.CurrentPlayer;
                            }
                            break;
                        case Enumerators.AbilityType.MASSIVE_DAMAGE:
                            {
                                AddRandomTargetUnit(true, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.MODIFICATOR_STATS:
                            {
                                if (ability.value > 0)
                                    AddRandomTargetUnit(false, ref target);
                                else
                                    AddRandomTargetUnit(true, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.STUN:
                            {
                                CheckAndAddTargets(ability, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.STUN_OR_DAMAGE_ADJUSTMENTS:
                            {
                                CheckAndAddTargets(ability, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.CHANGE_STAT:
                            {
                                if (ability.value > 0)
                                    AddRandomTargetUnit(false, ref target);
                                else
                                    AddRandomTargetUnit(true, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.SUMMON:
                            {

                            }
                            break;
                        case Enumerators.AbilityType.WEAPON:
                            {
                                target = _gameplayManager.CurrentPlayer;
                            }
                            break;
                        case Enumerators.AbilityType.SPURT:
                            {
                                AddRandomTargetUnit(true, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.SPELL_ATTACK:
                            {
                                CheckAndAddTargets(ability, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.HEAL:
                            {
                                var units = GetUnitsWithLowHP();

                                if (units.Count > 0)
                                {
                                    target = units[_random.Next(0, units.Count)];
                                }
                                else
                                {
                                    target = _gameplayManager.OpponentPlayer;
                                }
                            }
                            break;
                        case Enumerators.AbilityType.DOT:
                            {
                                CheckAndAddTargets(ability, ref target);
                            }
                            break;
                        default: break;
                    }

                    return target; // hack to handle only one ability
                }

                return target;
            }
            else
            {
                return null;
            }
        }

        private void CheckAndAddTargets(AbilityData ability, ref object target)
        {
            if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
            {
                AddRandomTargetUnit(true, ref target);
            }
            else if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT))
            {
                target = _gameplayManager.CurrentPlayer;
            }
        }

        private bool AddRandomTargetUnit(bool opponent, ref object target, bool lowHP = false, bool addAttackIgnore = false)
        {
            BoardUnit boardUnit = null;

            if (opponent)
                boardUnit = GetRandomOpponentUnit();
            else
                boardUnit = GetRandomUnit(lowHP);

            if (boardUnit != null)
            {
                target = boardUnit;

                if (addAttackIgnore)
                    _attackedUnitTargets.Add(boardUnit);

                return true;
            }

            return false;
        }

        private int GetPlayerAttackingValue()
        {
            int power = 0;
            foreach (var creature in _gameplayManager.OpponentPlayer.CardsOnBoard)
                if (creature.health > 0 && (_unitNumberOfTunrsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL))
                    power += creature.damage;
            return power;
        }

        private int GetOpponentAttackingValue()
        {
            int power = 0;
            foreach (var card in _gameplayManager.CurrentPlayer.CardsOnBoard)
                power += card.damage;
            return power;
        }
    
        private List<BoardUnit> GetUnitsWithLowHP()
        {
            List<BoardUnit> finalList = new List<BoardUnit>();

            var list = GetUnitsOnBoard();

            foreach (var item in list)
            {
                if (item.HP < item.initialHP)
                    finalList.Add(item);
            }

            list = list.OrderBy(x => x.HP).OrderBy(y => y.HP.ToString().Length).ToList();

            return finalList;
        }

        private List<WorkingCard> GetUnitCardsInHand()
        {
            List<WorkingCard> list = _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x => x.libraryCard.cardKind == Enumerators.CardKind.CREATURE);

            List<Data.Card> cards = new List<Data.Card>();

            foreach (var item in list)
                cards.Add(_dataManager.CachedCardsLibraryData.GetCard(item.cardId));

            cards = cards.OrderBy(x => x.cost).ThenBy(y => y.cost.ToString().Length).ToList();

            List<WorkingCard> sortedList = new List<WorkingCard>();

            cards.Reverse();

            foreach (var item in cards)
                sortedList.Add(list.Find(x => x.cardId == item.id && !sortedList.Contains(x)));

            list.Clear();
            cards.Clear();

            return sortedList;
        }

        private List<WorkingCard> GetSpellCardsInHand()
        {
            return _gameplayManager.OpponentPlayer.CardsInHand.FindAll(x => x.libraryCard.cardKind == Enumerators.CardKind.SPELL);
        }

        private List<BoardUnit> GetUnitsOnBoard()
        {
            return _gameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.HP > 0);
        }

        private BoardUnit GetRandomUnit(bool lowHP = false)
        {
            List<BoardUnit> eligibleUnits = null;

            if (!lowHP)
                eligibleUnits = _gameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.HP > 0 && !_attackedUnitTargets.Contains(x));
            else
                eligibleUnits = _gameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.HP < x.initialHP && !_attackedUnitTargets.Contains(x));

            if (eligibleUnits.Count > 0)
                return eligibleUnits[_random.Next(0, eligibleUnits.Count)];
            return null;
        }

        private BoardUnit GetTargetOpponentUnit()
        {
            var eligibleUnits = _gameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.HP > 0);

            if (eligibleUnits.Count > 0)
            {
                var heavyUnits = eligibleUnits.FindAll(x => x.Card.type == Enumerators.CardType.HEAVY);
                if (heavyUnits != null && heavyUnits.Count >= 1)
                    return heavyUnits[_random.Next(0, heavyUnits.Count)];
                else
                    return eligibleUnits[_random.Next(0, eligibleUnits.Count)];
            }
            return null;
        }

        private BoardUnit GetRandomOpponentUnit()
        {
            var eligibleCreatures = _gameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.HP > 0 && !_attackedUnitTargets.Contains(x));
            if (eligibleCreatures.Count > 0)
                return eligibleCreatures[_random.Next(0, eligibleCreatures.Count)];
            return null;
        }

        private bool OpponentHasHeavyUnits()
        {
            var board = _gameplayManager.CurrentPlayer.BoardCards;
            var eligibleCreatures = board.FindAll(x => x.HP > 0);
            if (eligibleCreatures.Count > 0)
            {
                var provokeCreatures = eligibleCreatures.FindAll(x => x.Card.type == Enumerators.CardType.HEAVY);
                return (provokeCreatures != null && provokeCreatures.Count >= 1);
            }
            return false;
        }


        // rewrite
        private void TryToUseBoardSkill()
        {
            if (_gameplayManager.IsTutorial)
                return;

      /*      var boardSkill = _aiPlayer.BoardSkills[0];

            if (_gameplayManager.OpponentPlayer.Mana >= boardSkill.manaCost)
            {
                object target = null;

                Enumerators.AffectObjectType selectedObjectType = Enumerators.AffectObjectType.NONE;

                switch (boardSkill.skillType)
                {
                    case Enumerators.SetType.EARTH:
                        {
                            selectedObjectType = Enumerators.AffectObjectType.PLAYER;
                            target = _gameplayManager.OpponentPlayer;
                        }
                        break;
                    case Enumerators.SetType.LIFE:
                        {
                            target = _gameplayManager.CurrentPlayer;
                            selectedObjectType = Enumerators.AffectObjectType.PLAYER;

                            var creatures = GetUnitsWithLowHP();

                            if (creatures.Count > 0)
                            {
                                target = creatures[0];
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                        }
                        break;
                    case Enumerators.SetType.FIRE:
                    case Enumerators.SetType.TOXIC:
                    case Enumerators.SetType.WATER:
                        {
                            target = _gameplayManager.CurrentPlayer;
                            selectedObjectType = Enumerators.AffectObjectType.PLAYER;

                            var creature = GetRandomOpponentUnit();

                            if (creature != null)
                            {
                                target = creature;
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                        }
                        break;
                    case Enumerators.SetType.AIR:
                        {
                            var creatures = GetUnitsWithLowHP();

                            if (creatures.Count > 0)
                            {
                                target = creatures[0];
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                            else
                            {
                                var creature = GetRandomOpponentUnit();

                                if (creature != null)
                                {
                                    target = creature;
                                    selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                                }
                                else return;
                            }
                        }
                        break;
                    default: return;
                }

                if (selectedObjectType == Enumerators.AffectObjectType.PLAYER)
                {
                    boardSkill.fightTargetingArrow = CreateOpponentTarget(true, boardSkill.gameObject, (target as Player).AvatarObject, () =>
                    {
                        boardSkill.fightTargetingArrow.selectedPlayer = target as Player;
                        boardSkill.DoOnUpSkillAction();
                    });
                }
                else
                {
                    var creature = target as BoardUnit;

                    if (creature != null)
                    {
                        boardSkill.fightTargetingArrow = CreateOpponentTarget(true, boardSkill.gameObject, creature.gameObject, () =>
                        {
                            boardSkill.fightTargetingArrow.selectedCard = creature;
                            boardSkill.DoOnUpSkillAction();
                        });
                    }
                }
            } */
        }
        // rewrite
        private OpponentBoardArrow CreateOpponentTarget(bool createTargetArrow, GameObject startObj, GameObject targetObject, System.Action action)
        {
            if (!createTargetArrow)
            {
                action?.Invoke();
                return null;
            }

            var targetingArrow = MonoBehaviour.Instantiate(fightTargetingArrowPrefab).GetComponent<OpponentBoardArrow>();
            targetingArrow.Begin(startObj.transform.position);

            targetingArrow.SetTarget(targetObject);

            MainApp.Instance.StartCoroutine(RemoveOpponentTargetingArrow(targetingArrow, action));

            return targetingArrow;
        }
        // rewrite
        private IEnumerator RemoveOpponentTargetingArrow(BoardArrow arrow, System.Action action)
        {
            yield return new WaitForSeconds(1f);
            MonoBehaviour.Destroy(arrow.gameObject);

            action?.Invoke();
        }
    }
}