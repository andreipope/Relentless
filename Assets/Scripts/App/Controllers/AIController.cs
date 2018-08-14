// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using System.Collections.Generic;
using System.Linq;
using System.Collections;

using UnityEngine;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;

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

        private bool _enabledAIBrain = true;
        private int _minTurnForAttack = 0;

        private Enumerators.AIType _aiType;
        private List<ActionItem> _allActions;

        private List<BoardUnit> _attackedUnitTargets;
        private List<BoardUnit> _unitsToIgnoreThisTurn;

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

        public void ResetAll()
        {
            ThreadTool.Instance.AbortAllThreads(this);
            ThreadTool.Instance.ClearMainthreadActions();
        }

        public void InitializePlayer()
        {
            _gameplayManager.OpponentPlayer = new Player(GameObject.Find("Opponent"), true);

            fightTargetingArrowPrefab = Resources.Load<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _attackedUnitTargets = new List<BoardUnit>();
            _unitsToIgnoreThisTurn = new List<BoardUnit>();

            var playerDeck = new List<string>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add("MonZoon");
                playerDeck.Add("Burrrnn");
                playerDeck.Add("Golem");
                playerDeck.Add("Rockky");
                playerDeck.Add("Rockky");
            }
            else
            {
                var deckId = _gameplayManager.OpponentDeckId;
                foreach (var card in _dataManager.CachedOpponentDecksData.decks.First(d => d.id == deckId).cards)
                {
                    for (var i = 0; i < card.amount; i++)
                    {
                        playerDeck.Add(card.cardName);
                       //  playerDeck.Add("Zeptic");
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
            var deck = _dataManager.CachedOpponentDecksData.decks.First(d => d.id == _gameplayManager.OpponentDeckId);
            _aiType = (Enumerators.AIType)System.Enum.Parse(typeof(Enumerators.AIType), deck.type);
        }

        private void FillActions()
        {
            _allActions = new List<ActionItem>();

            var allActionsType = _dataManager.CachedOpponentDecksData.decks.First(d => d.id == _gameplayManager.OpponentDeckId).opponentActions;
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
            {
                ThreadTool.Instance.AbortAllThreads(this);
                return;
            }

            DoAIBrain();
        }

        private void OnEndTurnEventHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.OpponentPlayer))
                return;

            ThreadTool.Instance.AbortAllThreads(this);

            _attackedUnitTargets.Clear();
            _unitsToIgnoreThisTurn.Clear();
        }

        private void DoAIBrain()
        {
            if (!_enabledAIBrain && Constants.DEV_MODE)
            {
                _timerManager.AddTimer((x) =>
                {
                    _battlegroundController.StopTurn();
                }, null, 1f);
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

            }, null, 1f);
        }
        // ai step 1
        private void PlayCardsFromHand()
        {
            try
            {
                foreach (var card in GetUnitCardsInHand())
                {
                    if (_gameplayManager.OpponentPlayer.BoardCards.Count >= Constants.MAX_BOARD_UNITS)
                        break;

                    if (CardCanBePlayable(card) && CheckSpecialCardRules(card))
                    {
                        ThreadTool.Instance.RunInMainThread(() => { PlayCardOnBoard(card); });
                        LetsThink();
                        LetsThink();
                        LetsThink();
                    }

                    //  if (Constants.DEV_MODE)
                    //     break;
                }

                foreach (var card in GetSpellCardsInHand())
                {
                    if (CardCanBePlayable(card) && CheckSpecialCardRules(card))
                    {
                        ThreadTool.Instance.RunInMainThread(() => { PlayCardOnBoard(card); });
                        LetsThink();
                        LetsThink();
                    }

                    // if (Constants.DEV_MODE)
                    //     break;
                }
                LetsThink();
                LetsThink();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }
        }
        // ai step 2
        private void UseUnitsOnBoard()
        {
          //  return;

            try
            {
                var unitsOnBoard = new List<BoardUnit>();
                var alreadyUsedUnits = new List<BoardUnit>();

                unitsOnBoard.AddRange(GetUnitsOnBoard());

                if (OpponentHasHeavyUnits())
                {
                    foreach (var unit in unitsOnBoard)
                    {
                        while (UnitCanBeUsable(unit))
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
                        while (UnitCanBeUsable(unit))
                        {
                            if (UnitCanBeUsable(unit))
                            {
                                ThreadTool.Instance.RunInMainThread(() => { unit.DoCombat(_gameplayManager.CurrentPlayer); });
                                LetsThink();
                            }
                        }
                    }
                }
                else
                {
                    foreach (var unit in unitsOnBoard)
                    {
                        while (UnitCanBeUsable(unit))
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
                }
            }
            catch(System.Exception ex)
            {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }
        }
        // ai step 3
        private void UsePlayerSkills()
        {
          //  return;

            try
            {
                if (_gameplayManager.IsTutorial || _gameplayManager.OpponentPlayer.IsStunned)
                return;
          
                ThreadTool.Instance.RunInMainThread(() =>
                {
                    if (_skillsController.opponentPrimarySkill.IsSkillReady)
                        DoBoardSkill(_skillsController.opponentPrimarySkill);
                });

                LetsThink();

                ThreadTool.Instance.RunInMainThread(() =>
                {
                    if (_skillsController.opponentSecondarySkill.IsSkillReady)
                        DoBoardSkill(_skillsController.opponentSecondarySkill);
                });

                LetsThink();
                LetsThink();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }
        }

        // some thinking - delay between general actions
        private void LetsThink()
        {      
            System.Threading.Thread.Sleep(Constants.DELAY_BETWEEN_AI_ACTIONS);
        }

        private bool CardCanBePlayable(WorkingCard card)
        {
            return ((card.libraryCard.cost <= _gameplayManager.OpponentPlayer.Goo && _gameplayManager.OpponentPlayer.turn > _minTurnForAttack) || Constants.DEV_MODE);
        }

        private bool UnitCanBeUsable(BoardUnit unit)
        {
            return unit.UnitCanBeUsable();
        }

        private bool CheckSpecialCardRules(WorkingCard card)
        {
            if(card.libraryCard.abilities != null)
            {
                foreach(var ability in card.libraryCard.abilities)
                {
                    if(ability.type.Equals("ATTACK_OVERLORD"))
                    {
                        if (ability.value >= _gameplayManager.OpponentPlayer.HP)
                            return false;
                    }
                }
            }

            return true;
        }

        private void PlayCardOnBoard(WorkingCard card)
        {
            var target = GetAbilityTarget(card);
            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE && _battlegroundController.opponentBoardCards.Count < Constants.MAX_BOARD_UNITS)
            {
                //if (libraryCard.abilities.Find(x => x.abilityType == Enumerators.AbilityType.CARD_RETURN) != null)
                //    if (target.Count == 0)
                //        return false;

                _gameplayManager.OpponentPlayer.RemoveCardFromHand(card);
                _gameplayManager.OpponentPlayer.AddCardToBoard(card);

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

            _gameplayManager.OpponentPlayer.Goo -= card.libraryCard.cost;
        }

        private void PlayCardCompleteHandler(WorkingCard card, object target)
        {
            var workingCard = _gameplayManager.OpponentPlayer.CardsOnBoard[_gameplayManager.OpponentPlayer.CardsOnBoard.Count - 1];

            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                var boardUnitElement = new BoardUnit(GameObject.Find("OpponentBoard").transform);
                var boardCreature = boardUnitElement.gameObject;
                boardCreature.tag = Constants.TAG_OPPONENT_OWNED;
                boardCreature.transform.position = Vector3.zero;
                boardUnitElement.ownerPlayer = card.owner;

                boardUnitElement.SetObjectInfo(workingCard);
                _battlegroundController.opponentBoardCards.Add(boardUnitElement);

                boardCreature.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board
                                                                     //PlayArrivalAnimation(boardCreature, libraryCard.cardType);

                _gameplayManager.OpponentPlayer.BoardCards.Add(boardUnitElement);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.PLAY_UNIT_CARD, new object[]
                {
                        boardUnitElement.ownerPlayer,
                        boardUnitElement
                }));


                boardUnitElement.PlayArrivalAnimation();


                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent(() =>
                {
                    bool createTargetArrow = false;

                    if (card.libraryCard.abilities != null && card.libraryCard.abilities.Count > 0)
                        createTargetArrow = _abilitiesController.IsAbilityCanActivateTargetAtStart(card.libraryCard.abilities[0]);

                    if (target != null)
                    {
                        CreateOpponentTarget(createTargetArrow, false, boardCreature.gameObject, target,
                                 () => { _abilitiesController.CallAbility(card.libraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitElement, null, false, null, target); });
                    }
                    else
                    {
                        _abilitiesController.CallAbility(card.libraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitElement, null, false, null);
                    }
                });

                

                //Debug.Log("UpdatePositionOfBoardUnitsOfOpponent");

                //_battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                var spellCard = MonoBehaviour.Instantiate(_cardsController.spellCardViewPrefab);
                spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;

                currentSpellCard = new SpellBoardCard(spellCard);

                currentSpellCard.Init(workingCard);
                currentSpellCard.SetHighlightingEnabled(false);

                var boardSpell = new BoardSpell(spellCard, workingCard);

                spellCard.gameObject.SetActive(false);

                bool createTargetArrow = false;

                if (card.libraryCard.abilities != null && card.libraryCard.abilities.Count > 0)
                    createTargetArrow = _abilitiesController.IsAbilityCanActivateTargetAtStart(card.libraryCard.abilities[0]);

                if (target != null)
                {
                    CreateOpponentTarget(createTargetArrow, false, _gameplayManager.OpponentPlayer.AvatarObject, target,
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
                                if (_gameplayManager.OpponentPlayer.BoardCards.Count > 1 || (Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.SPELL
                                    || (ability.abilityType == Enumerators.AbilityType.CARD_RETURN && _gameplayManager.OpponentPlayer.BoardCards.Count > 0))
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
                        case Enumerators.AbilityType.DESTROY_UNIT_BY_TYPE:
                            {
                                GetTargetByType(ability, ref target, false);
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

        private void GetTargetByType(AbilityData ability, ref object target, bool checkPlayerAlso)
        {
            if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
            {
                var targets= GetHeavyUnitsOnBoard(_gameplayManager.CurrentPlayer);

                if (targets.Count > 0)
                    target = targets[UnityEngine.Random.Range(0, targets.Count)];

                if (checkPlayerAlso && target == null && ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER_CARD))
                {
                    target = _gameplayManager.CurrentPlayer;

                    targets = GetHeavyUnitsOnBoard(_gameplayManager.OpponentPlayer);

                    if (targets.Count > 0)
                        target = targets[UnityEngine.Random.Range(0, targets.Count)];
                }
            }
        }

        private List<BoardUnit> GetHeavyUnitsOnBoard(Player player)
        {
            return player.BoardCards.FindAll(x => x.hasHeavy || x.HasBuffHeavy);
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
            foreach (var creature in _gameplayManager.OpponentPlayer.BoardCards)
                if (creature.CurrentHP > 0 && (creature.numTurnsOnBoard >= 1 || creature.IsFeralUnit()))
                    power += creature.CurrentDamage;
            return power;
        }

        private int GetOpponentAttackingValue()
        {
            int power = 0;
            foreach (var card in _gameplayManager.CurrentPlayer.BoardCards)
                power += card.CurrentDamage;
            return power;
        }
    
        private List<BoardUnit> GetUnitsWithLowHP(List<BoardUnit> unitsToIgnore = null)
        {
            List<BoardUnit> finalList = new List<BoardUnit>();

            var list = GetUnitsOnBoard();

            foreach (var item in list)
            {
                if (item.CurrentHP < item.MaxCurrentHP)
                    finalList.Add(item);
            }

            if (unitsToIgnore != null)
                finalList = finalList.FindAll(x => !unitsToIgnore.Contains(x));


            finalList = finalList.OrderBy(x => x.CurrentHP).OrderBy(y => y.CurrentHP.ToString().Length).ToList();

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
            return _gameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.CurrentHP > 0);
        }

        private BoardUnit GetRandomUnit(bool lowHP = false)
        {
            List<BoardUnit> eligibleUnits = null;

            if (!lowHP)
                eligibleUnits = _gameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.CurrentHP > 0 && !_attackedUnitTargets.Contains(x));
            else
                eligibleUnits = _gameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.CurrentHP < x.MaxCurrentHP && !_attackedUnitTargets.Contains(x));

            if (eligibleUnits.Count > 0)
                return eligibleUnits[_random.Next(0, eligibleUnits.Count)];
            return null;
        }

        private BoardUnit GetTargetOpponentUnit()
        {
            var eligibleUnits = _gameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.CurrentHP > 0);

            if (eligibleUnits.Count > 0)
            {
                var heavyUnits = eligibleUnits.FindAll(x => x.IsHeavyUnit());
                if (heavyUnits != null && heavyUnits.Count >= 1)
                    return heavyUnits[_random.Next(0, heavyUnits.Count)];
                else
                    return eligibleUnits[_random.Next(0, eligibleUnits.Count)];
            }
            return null;
        }

        private BoardUnit GetRandomOpponentUnit(List<BoardUnit> unitsToIgnore = null)
        {
            var eligibleCreatures = _gameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.CurrentHP > 0 && !_attackedUnitTargets.Contains(x));

            if (unitsToIgnore != null)
                eligibleCreatures = eligibleCreatures.FindAll(x => !unitsToIgnore.Contains(x));

            if (eligibleCreatures.Count > 0)
                return eligibleCreatures[_random.Next(0, eligibleCreatures.Count)];
            return null;
        }

        private bool OpponentHasHeavyUnits()
        {
            var board = _gameplayManager.CurrentPlayer.BoardCards;
            var eligibleCreatures = board.FindAll(x => x.CurrentHP > 0);
            if (eligibleCreatures.Count > 0)
            {
                var provokeCreatures = eligibleCreatures.FindAll(x => x.IsHeavyUnit());
                return (provokeCreatures != null && provokeCreatures.Count >= 1);
            }
            return false;
        }

        private void DoBoardSkill(BoardSkill skill)
        {
            object target = null;

            Enumerators.AffectObjectType selectedObjectType = Enumerators.AffectObjectType.NONE;

#region find target
            switch (skill.skill.overlordSkill)
            {
                case Enumerators.OverlordSkill.HARDEN:
                case Enumerators.OverlordSkill.STONE_SKIN:
                case Enumerators.OverlordSkill.DRAW:
                    {
                        selectedObjectType = Enumerators.AffectObjectType.PLAYER;
                        target = _gameplayManager.OpponentPlayer;
                    }
                    break;
                case Enumerators.OverlordSkill.HEALING_TOUCH:
                case Enumerators.OverlordSkill.MEND:              
                    {
                        target = _gameplayManager.OpponentPlayer;
                        selectedObjectType = Enumerators.AffectObjectType.PLAYER;

                        var units = GetUnitsWithLowHP();

                        if (units.Count > 0)
                        {
                            target = units[0];
                            selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                        }
                    }
                    break;
                case Enumerators.OverlordSkill.RABIES:
                    {
                        var unit = GetRandomUnit();

                        if (unit != null)
                        {
                            target = unit;
                            selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                        }
                    }
                    break;
                case Enumerators.OverlordSkill.POISON_DART:
                case Enumerators.OverlordSkill.TOXIC_POWER:
                case Enumerators.OverlordSkill.ICE_BOLT:
                case Enumerators.OverlordSkill.FREEZE:
                case Enumerators.OverlordSkill.FIRE_BOLT:                    
                    {
                        target = _gameplayManager.CurrentPlayer;
                        selectedObjectType = Enumerators.AffectObjectType.PLAYER;

                        var unit = GetRandomOpponentUnit();

                        if (unit != null)
                        {
                            target = unit;
                            selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                        }
                    }
                    break;
                case Enumerators.OverlordSkill.PUSH: 
                    {
                        var units = GetUnitsWithLowHP(_unitsToIgnoreThisTurn);

                        if (units.Count > 0)
                        {
                            target = units[0];

                            _unitsToIgnoreThisTurn.Add(target as BoardUnit);

                            selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                        }
                        else
                        {
                            var unit = GetRandomOpponentUnit(_unitsToIgnoreThisTurn);

                            if (unit != null)
                            {
                                target = unit;

                                _unitsToIgnoreThisTurn.Add(target as BoardUnit);

                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                            else return;
                        }
                    }
                    break;
                default: return;
            }
#endregion

            skill.StartDoSkill();

            if (selectedObjectType == Enumerators.AffectObjectType.PLAYER)
            {
                skill.fightTargetingArrow = CreateOpponentTarget(true, skill.IsPrimary, skill.selfObject, (target as Player), () =>
                {
                    skill.fightTargetingArrow.selectedPlayer = target as Player;
                    skill.EndDoSkill();
                });
            }
            else
            {
                if (target != null)
                {
                    var unit = target as BoardUnit;

                    skill.fightTargetingArrow = CreateOpponentTarget(true, skill.IsPrimary, skill.selfObject, unit, () =>
                    {
                        skill.fightTargetingArrow.selectedCard = unit;
                        skill.EndDoSkill();
                    });
                }
            }
        }


        // rewrite
        private OpponentBoardArrow CreateOpponentTarget(bool createTargetArrow, bool isReverseArrow, GameObject startObj, object target, System.Action action)
        {
            if (!createTargetArrow)
            {
                action?.Invoke();
                return null;
            }

            var targetingArrow = MonoBehaviour.Instantiate(fightTargetingArrowPrefab).AddComponent<OpponentBoardArrow>();
            targetingArrow.Begin(startObj.transform.position);

            targetingArrow.SetTarget(target);

            MainApp.Instance.StartCoroutine(RemoveOpponentTargetingArrow(targetingArrow, action));

            return targetingArrow;
        }
        // rewrite
        private IEnumerator RemoveOpponentTargetingArrow(OpponentBoardArrow arrow, System.Action action)
        {
            yield return new WaitForSeconds(1f);
            arrow.Dispose();
            MonoBehaviour.Destroy(arrow.gameObject);

            action?.Invoke();
        }
    }
}