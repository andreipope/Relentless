// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using UnityEngine;
using LoomNetwork.CZB.Common;
using DG.Tweening;

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

        private int _minTurnForAttack;

        private Dictionary<int, int> numTurnsOnBoard = new Dictionary<int, int>();

        private GameObject fightTargetingArrowPrefab;

        private List<BoardUnit> _attackedCreatureTargets;

        private bool _enabledAIBrain = true;

        private List<ActionItem> allActions;

        private GameObject _boardCreaturePrefab;

        private Player PlayerInfo;

        public GameObject currentBoardCreature;
        public BoardUnit currentCreature;
        public BoardCard currentSpellCard;

        public Player localPlayer,
                      aiPlayer;

        public Enumerators.AIType aiType;

        public bool AlreadyAttackedInThisTurn { get; set; }
        public bool IsPlayerStunned { get; set; }
        public bool IsActive { get; set; }



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

            _boardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature");
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void InitializePlayer()
        {
            PlayerInfo = new Player(GameObject.Find("Opponent"), true);

            _gameplayManager.PlayersInGame.Add(PlayerInfo);

            fightTargetingArrowPrefab = Resources.Load<GameObject>("Prefabs/Gameplay/OpponentTargetingArrow");

            _attackedCreatureTargets = new List<BoardUnit>();

            var playerDeck = new List<int>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add(10);
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

            PlayerInfo.SetDeck(playerDeck);

            PlayerInfo.SetFirstHand(_gameplayManager.IsTutorial);

            _battlegroundController.UpdatePositionOfCardsInOpponentHand();


            PlayerInfo.OnStartTurnEvent += OnStartTurnEventHandler;
            PlayerInfo.OnEndTurnEvent += OnEndTurnEventHandler;

            localPlayer = _gameplayManager.GetLocalPlayer();
            aiPlayer = _gameplayManager.GetOpponentPlayer();
        }

        public void OnStartGame()
        {
            if (!_gameplayManager.IsTutorial)
            {
                _minTurnForAttack = UnityEngine.Random.Range(1, 3);
                FillActions();

                SetAITypeByDeck();
            }
        }


        public void OnEndGame()
        {
            MainApp.Instance.StopAllCoroutines();
        }

        public void OnStartTurnEventHandler()
        {
            if (!_gameplayManager.WhoseTurn.Equals(PlayerInfo))
                return;

            MainApp.Instance.StartCoroutine(RunLogic());
        }


        public void OnEndTurnEventHandler()
        {
            if (!_gameplayManager.WhoseTurn.Equals(PlayerInfo))
                return;

            MainApp.Instance.StopAllCoroutines();

            foreach (var card in PlayerInfo.CardsOnBoard)
            {
                if (numTurnsOnBoard.ContainsKey(card.instanceId))
                {
                    numTurnsOnBoard[card.instanceId] += 1;
                }
                else
                {
                    numTurnsOnBoard.Add(card.instanceId, 1);
                }
            }
        }

        private void SetAITypeByDeck()
        {
            var deck = _dataManager.CachedOpponentDecksData.decks[_gameplayManager.OpponentDeckId];
            aiType = (Enumerators.AIType)System.Enum.Parse(typeof(Enumerators.AIType), deck.type);
        }

        /// <summary>
        /// This method runs the AI logic asynchronously.
        /// </summary>
        /// <returns>The AI logic coroutine.</returns>
        private IEnumerator RunLogic()
        {
            if (!_gameplayManager.GameStarted)
            {
                yield return null;
            }

            if (!_gameplayManager.IsLocalPlayerTurn())
            {
                PlayerInfo.ManaOnCurrentTurn++;// = Mathf.Clamp(PlayerInfo.ManaOnCurrentTurn + 1, 0, Constants.MAXIMUM_PLAYER_MANA);
                PlayerInfo.Mana = PlayerInfo.ManaOnCurrentTurn;
                if (PlayerInfo.CardsInDeck.Count > 0)
                {
                    _cardsController.AddCardToHand(PlayerInfo, PlayerInfo.CardsInDeck[0]);
                    //_cardsController.AddCardToOpponentHand(PlayerInfo.CardsInHand[PlayerInfo.CardsInHand.Count - 1]);
                    //_battlegroundController.RearrangeOpponentHand(true, true);
                }
            }


            if (PlayerInfo.CurrentBoardWeapon != null && !IsPlayerStunned)
            {
                PlayerInfo.AlreadyAttackedInThisTurn = false;
                PlayerInfo.CurrentBoardWeapon.ActivateWeapon(true);
            }

            _attackedCreatureTargets.Clear();

            // Simulate 'thinking' time. This could be random or dependent on the
            // complexity of the board state for increased realism.
            yield return new WaitForSeconds(2.0f);
            // Actually perform the AI logic in a separate coroutine.
            MainApp.Instance.StartCoroutine(PerformMove());
        }



        /// <summary>
        /// This methods performs the actual AI logic.
        /// </summary>
        /// <returns>The AI logic coroutine.</returns>
        protected virtual IEnumerator PerformMove()
        {
            if (!_enabledAIBrain)
            {
                _battlegroundController.StopTurn();
            }
            else
            {
                foreach (var creature in GetUnitCardsInHand())
                {
                    if (TryToPlayCard(creature))
                    {
                        yield return new WaitForSeconds(2.0f);
                    }

                    if (Constants.DEV_MODE)
                        break;
                }

                foreach (var spell in GetSpellCardsInHand())
                {
                    if (TryToPlayCard(spell))
                    {
                        yield return new WaitForSeconds(2.0f);
                    }
                }
                if(_tutorialManager.IsTutorial && _tutorialManager.CurrentStep == 11)
                {
                    (_tutorialManager as TutorialManager).paused = true;
                }
                else
                {
                    yield return new WaitForSeconds(3.0f);

                    var boardCreatures = new List<BoardUnit>();
                    foreach (var creature in GetBoardCreatures())
                        boardCreatures.Add(creature);

                    var usedCreatures = new List<BoardUnit>();

                    if (OpponentHasHeavyUnits())
                    {
                        foreach (var creature in boardCreatures)
                        {
                            if (creature != null && creature.HP > 0 &&
                                (numTurnsOnBoard[creature.Card.instanceId] >= 1 || creature.Card.type == Enumerators.CardType.FERAL) && creature.IsPlayable)
                            {
                                var attackedCreature = GetTargetOpponentUnit();
                                if (attackedCreature != null)
                                {
                                    PlayCreatureAttackSound(creature.Card);

                                    AttackCreature(creature, attackedCreature);

                                    _battleController.AttackCreatureByCreature(creature, attackedCreature);

                                    usedCreatures.Add(creature);
                                    yield return new WaitForSeconds(2.0f);
                                    if (!OpponentHasHeavyUnits())
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    foreach (var creature in usedCreatures)
                    {
                        boardCreatures.Remove(creature);
                    }

                    var totalPower = GetPlayerAttackingValue();
                    if ((totalPower >= _gameplayManager.GetLocalPlayer().HP ||
                        (aiType == Enumerators.AIType.BLITZ_AI ||
                         aiType == Enumerators.AIType.TIME_BLITZ_AI)) && !_tutorialManager.IsTutorial)
                    {
                        foreach (var creature in boardCreatures)
                        {
                            if (creature != null && creature.HP > 0 &&
                                (numTurnsOnBoard[creature.Card.instanceId] >= 1 || creature.Card.type == Enumerators.CardType.FERAL) && creature.IsPlayable)
                            {
                                PlayCreatureAttackSound(creature.Card);

                                AttackPlayer(creature, localPlayer);

                                _battleController.AttackPlayerByCreature(creature, localPlayer);

                                yield return new WaitForSeconds(2.0f);
                            }
                        }
                    }
                    else
                    {
                        foreach (var creature in boardCreatures)
                        {
                            if (creature != null && creature.HP > 0 &&
                                (numTurnsOnBoard[creature.Card.instanceId] >= 1 || creature.Card.type == Enumerators.CardType.FERAL) && creature.IsPlayable)
                            {
                                Debug.Log("Should Attack");
                                var playerPower = GetPlayerAttackingValue();
                                var opponentPower = GetOpponentAttackingValue();
                                if (playerPower > opponentPower && !_tutorialManager.IsTutorial)
                                {
                                    PlayCreatureAttackSound(creature.Card);

                                    _battleController.AttackPlayerByCreature(creature, _gameplayManager.GetLocalPlayer());

                                    yield return new WaitForSeconds(2.0f);
                                }
                                else
                                {
                                    var attackedCreature = GetRandomOpponentUnit();
                                    if (attackedCreature != null)
                                    {
                                        PlayCreatureAttackSound(creature.Card);

                                        AttackCreature(creature, attackedCreature);

                                        _battleController.AttackCreatureByCreature(creature, attackedCreature);
                                        yield return new WaitForSeconds(2.0f);
                                    }
                                    else
                                    {
                                        PlayCreatureAttackSound(creature.Card);

                                        AttackPlayer(creature, localPlayer);

                                        _battleController.AttackPlayerByCreature(creature, localPlayer);
                                        yield return new WaitForSeconds(2.0f);
                                    }
                                }
                            }
                        }
                    }



                    yield return new WaitForSeconds(1.0f);

                    TryToUseBoardSkill();

                    yield return new WaitForSeconds(1.0f);

                    TryToUseBoardWeapon();

                    yield return new WaitForSeconds(1.0f);

                    if (!_tutorialManager.IsTutorial)
                        foreach (var skill in PlayerInfo.BoardSkills)
                            skill.OnEndTurn();

                    _battlegroundController.StopTurn();
                }
            }
        }

        private void PlayCreatureAttackSound(WorkingCard card)
        {
           _soundManager.PlaySound(Enumerators.SoundType.CARDS, card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);
        }

        protected void TryToUseBoardSkill()
        {
            if (_gameplayManager.IsTutorial)
                return;

            var boardSkill = PlayerInfo.BoardSkills[0];

            if (PlayerInfo.Mana >= boardSkill.manaCost)
            {
                object target = null;

                Enumerators.AffectObjectType selectedObjectType = Enumerators.AffectObjectType.NONE;

                switch (boardSkill.skillType)
                {
                    case Enumerators.SetType.EARTH:
                        {
                            selectedObjectType = Enumerators.AffectObjectType.PLAYER;
                            target = aiPlayer;
                        }
                        break;
                    case Enumerators.SetType.LIFE:
                        {
                            target = localPlayer;
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
                            target = localPlayer;
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
            }
        }

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

        private IEnumerator RemoveOpponentTargetingArrow(BoardArrow arrow, System.Action action)
        {
            yield return new WaitForSeconds(1f);
            MonoBehaviour.Destroy(arrow.gameObject);

            action?.Invoke();
        }

        private void TryToUseBoardWeapon()
        {
            if (PlayerInfo.CurrentBoardWeapon != null && PlayerInfo.CurrentBoardWeapon.CanAttack)
            {
                var target = GetRandomOpponentUnit();

                if (target != null)
                {
                    var creature = _battlegroundController.playerBoardCards.Find(x => x.Card.instanceId == target.Card.instanceId);

                    PlayerInfo.CurrentBoardWeapon.ImmediatelyAttack(creature);
                }
                else
                {
                    PlayerInfo.CurrentBoardWeapon.ImmediatelyAttack(localPlayer);
                }
            }
        }

        private bool TryToPlayCard(WorkingCard card)
        {

            if ((card.libraryCard.cost <= PlayerInfo.Mana && _battlegroundController.currentTurn > _minTurnForAttack) || Constants.DEV_MODE)
            {
                var target = GetAbilityTarget(card);
                if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.CREATURE &&
                    _battlegroundController.opponentBoardCards.Count < Constants.MAX_BOARD_CREATURES)
                {
                    //if (libraryCard.abilities.Find(x => x.abilityType == Enumerators.AbilityType.CARD_RETURN) != null)
                    //    if (target.Count == 0)
                    //        return false;

                    PlayerInfo.RemoveCardFromHand(card);
                    PlayerInfo.AddCardToBoard(card);

                    numTurnsOnBoard[card.instanceId] = 0;

                    MoveCard(card, target);

                    // AttackCreature(PlayerInfo.BoardCards.Find(x => x.Card == card), target);

                    AddCardInfo(card);

                    // if (GameClient.Get<ITutorialManager>().IsTutorial && card.cardId == 11)
                    //      FightCreatureBySkill(1, card);

                }
                else if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
                {
                    if (target != null)
                    {
                        MoveCard(card, target);

                        //   PlaySpellCard(card, target);

                        AddCardInfo(card);
                    }
                }

                PlayerInfo.Mana -= card.libraryCard.cost;
                return true;
            }
            return false;
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
                                if (_gameplayManager.GetLocalPlayer().BoardCards.Count > 1
                                     || (ability.abilityType == Enumerators.AbilityType.CARD_RETURN && _gameplayManager.GetLocalPlayer().BoardCards.Count > 0))
                                {
                                    needsToSelectTarget = true;
                                    abilitiesWithTarget.Add(ability);
                                }
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            {
                                if (_gameplayManager.GetOpponentPlayer().BoardCards.Count > 1 || (Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.SPELL
                                    || (ability.abilityType == Enumerators.AbilityType.CARD_RETURN && _gameplayManager.GetOpponentPlayer().BoardCards.Count > 0))
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
                                target = aiPlayer;
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
                                    target = localPlayer;
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
                                target = localPlayer;
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
                                    target = units[UnityEngine.Random.Range(0, units.Count)];
                                }
                                else
                                {
                                    target = aiPlayer;
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

        private void CheckAndAddTargets(LoomNetwork.CZB.Data.AbilityData ability, ref object targetInfo)
        {
            if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
            {
                AddRandomTargetUnit(true, ref targetInfo);
            }
            else if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT))
            {
                targetInfo = localPlayer;
            }
        }

        private bool AddRandomTargetUnit(bool opponent, ref object targetInfo, bool lowHP = false, bool addAttackIgnore = false)
        {
            BoardUnit target = null;

            if (opponent)
                target = GetRandomOpponentUnit();
            else
                target = GetRandomUnit(lowHP);

            if (target != null)
            {
                targetInfo = target;

                if (addAttackIgnore)
                    _attackedCreatureTargets.Add(target);

                return true;
            }

            return false;
        }

        private void AddCardInfo(WorkingCard card)
        {
            string cardSetName = string.Empty;
            foreach (var cardSet in _dataManager.CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            GameObject go = null;
            BoardCard boardCard = null;
            if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                go = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard"));
                boardCard = new UnitBoardCard(go);
            }
            else if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                go = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard"));
                boardCard = new SpellBoardCard(go);

            }
            boardCard.Init(card, cardSetName);
            go.transform.position = new Vector3(-6, 0, 0);
            go.transform.localScale = Vector3.one * .3f;
            boardCard.SetHighlightingEnabled(false);

            _timerManager.StopTimer(DestroyCardInfo);
            _timerManager.AddTimer(DestroyCardInfo, new object[] { go }, 2, false);
        }

        private void DestroyCardInfo(object[] param)
        {
            MonoBehaviour.Destroy((GameObject)param[0]);
        }

        private int GetPlayerAttackingValue()
        {
            var power = 0;
            foreach (var creature in PlayerInfo.CardsOnBoard)
            {
                if (creature.health > 0 &&
                    (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL))
                {
                    power += creature.damage;
                }
            }
            return power;
        }

        private int GetOpponentAttackingValue()
        {
            var power = 0;
            foreach (var card in localPlayer.CardsOnBoard)
            {
                power += card.damage;
            }
            return power;
        }


        private List<BoardUnit> GetUnitsWithLowHP()
        {
            List<BoardUnit> finalList = new List<BoardUnit>();

            var list = GetBoardCreatures();

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
            List<WorkingCard> list = PlayerInfo.CardsInHand.FindAll(x => x.libraryCard.cardKind == Enumerators.CardKind.CREATURE);

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
            return PlayerInfo.CardsInHand.FindAll(x => x.libraryCard.cardKind == Enumerators.CardKind.SPELL);
        }

        private List<BoardUnit> GetBoardCreatures()
        {
            var board = PlayerInfo.BoardCards;
            var eligibleCreatures = board.FindAll(x => x.HP > 0);
            return eligibleCreatures;
        }

        private BoardUnit GetRandomUnit(bool lowHP = false)
        {
            var board = PlayerInfo.BoardCards;
            List<BoardUnit> eligibleCreatures = null;

            if (!lowHP)
                eligibleCreatures = board.FindAll(x => x.HP > 0 && !_attackedCreatureTargets.Contains(x));
            else
                eligibleCreatures = board.FindAll(x => x.HP < x.initialHP && !_attackedCreatureTargets.Contains(x));

            if (eligibleCreatures.Count > 0)
            {
                return eligibleCreatures[UnityEngine.Random.Range(0, eligibleCreatures.Count)];
            }
            return null;
        }

        private BoardUnit GetTargetOpponentUnit()
        {
            var board = localPlayer.BoardCards;
            var eligibleCreatures = board.FindAll(x => x.HP > 0);

            if (eligibleCreatures.Count > 0)
            {
                var provokeCreatures = eligibleCreatures.FindAll(x => x.Card.type == Enumerators.CardType.HEAVY);
                if (provokeCreatures != null && provokeCreatures.Count >= 1)
                {
                    return provokeCreatures[UnityEngine.Random.Range(0, provokeCreatures.Count)];
                }
                else
                {
                    return eligibleCreatures[UnityEngine.Random.Range(0, eligibleCreatures.Count)];
                }
            }
            return null;
        }

        private BoardUnit GetRandomOpponentUnit()
        {
            var board = localPlayer.BoardCards;

            var eligibleCreatures = board.FindAll(x => x.HP > 0 && !_attackedCreatureTargets.Contains(x));
            if (eligibleCreatures.Count > 0)
            {
                return eligibleCreatures[UnityEngine.Random.Range(0, eligibleCreatures.Count)];
            }
            return null;
        }

        private bool OpponentHasHeavyUnits()
        {
            var board = localPlayer.BoardCards;
            var eligibleCreatures = board.FindAll(x => x.HP > 0);
            if (eligibleCreatures.Count > 0)
            {
                var provokeCreatures = eligibleCreatures.FindAll(x => x.Card.type == Enumerators.CardType.HEAVY);
                return (provokeCreatures != null && provokeCreatures.Count >= 1);
            }
            return false;
        }

        private void FillActions()
        {
            allActions = new List<ActionItem>();

            var allActionsType = _dataManager.CachedOpponentDecksData.decks[_gameplayManager.OpponentDeckId].opponentActions;
            allActions = _dataManager.CachedActionsLibraryData.GetActions(allActionsType.ToArray());
        }

        private void MoveCard(WorkingCard card, object target)
        {
            var randomCard = _battlegroundController.opponentHandCards[0];

            _battlegroundController.opponentHandCards.Remove(randomCard);

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            randomCard.transform.DOMove(Vector3.up * 2.5f, 0.6f).OnComplete(() =>
            {
                //GameClient.Get<ITimerManager>().AddTimer(DestroyRandomCard, new object[] { randomCard }, 1f, false);
                //randomCard.GetComponent<Animator>().SetTrigger("RemoveCard");
                randomCard.transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>().Play();

                randomCard.transform.DOScale(Vector3.one * 1.2f, 0.6f).OnComplete(() =>
                {
                    _cardsController.RemoveOpponentCard(new object[] { randomCard });

                    _timerManager.AddTimer(OnMovedCardCompleted, new object[] { card, target }, 0.1f);

                    _timerManager.AddTimer((creat) =>
                    {
                        PlayerInfo.GraveyardCardsCount++;
                    }, null, 1f);


                });
            });

            randomCard.transform.DORotate(Vector3.zero, 0.5f);

            _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
        }

        private void DestroyRandomCard(object[] param)
        {
            GameObject randomCard = param[0] as GameObject;
            MonoBehaviour.Destroy(randomCard);
        }

        private void OnMovedCardCompleted(object[] param)
        {
            WorkingCard card = (WorkingCard)param[0];
            object target = param[1];

            string cardSetName = string.Empty;
            foreach (var cardSet in _dataManager.CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            var runtimeCard = PlayerInfo.CardsOnBoard[PlayerInfo.CardsOnBoard.Count - 1];

            if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                var boardCreatureElement = new BoardUnit(GameObject.Find("OpponentBoard").transform);
                var boardCreature = boardCreatureElement.gameObject;
                boardCreature.tag = "OpponentOwned";
                boardCreatureElement.ownerPlayer = card.owner;

                boardCreatureElement.SetObjectInfo(runtimeCard, cardSetName);
                _battlegroundController.opponentBoardCards.Add(boardCreatureElement);

                boardCreature.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board
                                                                     //PlayArrivalAnimation(boardCreature, libraryCard.cardType);

                PlayerInfo.BoardCards.Add(boardCreatureElement);

                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent(() =>
                 {
                     // opponentHandZone.numCards -= 1;
                     //  opponentManaStat.baseValue -= libraryCard.cost;

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
                                  () => { _abilitiesController.CallAbility(card.libraryCard, null, runtimeCard, Enumerators.CardKind.CREATURE, boardCreatureElement, null, false, null, target); });

                     }
                     else
                     {
                         _abilitiesController.CallAbility(card.libraryCard, null, runtimeCard, Enumerators.CardKind.CREATURE, boardCreatureElement, null, false, null);
                     }
                 });


                boardCreatureElement.PlayArrivalAnimation();

                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
                //GameClient.Get<ITimerManager>().AddTimer(RemoveOpponentCard, new object[] { randomCard }, 0.1f, false);
            }
            else if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                var spellCard = MonoBehaviour.Instantiate(_cardsController.spellCardViewPrefab);
                spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;

                currentSpellCard = new SpellBoardCard(spellCard);

                currentSpellCard.Init(runtimeCard, cardSetName);
                currentSpellCard.SetHighlightingEnabled(false);

                var boardSpell = new BoardSpell(spellCard);

                spellCard.gameObject.SetActive(false);

                //    opponentManaStat.baseValue -= libraryCard.cost;


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

                    CreateOpponentTarget(createTargetArrow, aiPlayer.AvatarObject, targetObject,
                        () => { _abilitiesController.CallAbility(card.libraryCard, null, runtimeCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null, target); });
                }

                else
                {
                    _abilitiesController.CallAbility(card.libraryCard, null, runtimeCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null);
                }

                //GameClient.Get<ITimerManager>().AddTimer(RemoveOpponentCard, new object[] { randomCard }, 0.1f, false);
            }
        }

        public void AttackPlayer(BoardUnit attackingCard, Player attackedPlayer)
        {
            if (attackingCard != null && attackedPlayer != null)
            {
                _actionsQueueController.AddNewActionInToQueue((parameter, completeCallback) =>
                {
                    attackingCard.SetHighlightingEnabled(false);

                    _animationsController.PlayFightAnimation(attackingCard.gameObject, attackedPlayer.AvatarObject, 0.1f, () =>
                    {
                        _vfxController.PlayAttackVFX(attackingCard.Card.type, attackedPlayer.AvatarObject.transform.position, attackingCard.Damage);

                        attackingCard.CreatureOnAttack(attackedPlayer);

                        completeCallback?.Invoke();
                    });
                });
            }
        }
    
        public void AttackCreature(BoardUnit attackingCard, BoardUnit attackedCard)
        {
            if (attackingCard != null && attackedCard != null)
            {
                //        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                _actionsQueueController.AddNewActionInToQueue((parameter, completeCallback) =>
                {
                    attackingCard.transform.position = new Vector3(attackingCard.transform.position.x, attackingCard.transform.position.y, attackingCard.transform.position.z - 0.2f);

                    attackingCard.SetHighlightingEnabled(false);

                    _animationsController.PlayFightAnimation(attackingCard.gameObject, attackedCard.gameObject, 0.5f, () =>
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.CARDS, attackingCard.Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                        _vfxController.PlayAttackVFX(attackingCard.Card.type, attackedCard.transform.position, attackingCard.Damage);

                        attackingCard.CreatureOnAttack(attackedCard);

                        attackingCard.transform.position = new Vector3(attackingCard.transform.position.x, attackingCard.transform.position.y, attackingCard.transform.position.z + 0.2f);

                      

                        completeCallback?.Invoke();
                    });
                });
            }
        }

        // need to check
        private void EffectActivateEventHandler(Enumerators.EffectActivateType effectActivateType, object[] param)
        {
            switch (effectActivateType)
            {
                case Enumerators.EffectActivateType.PLAY_SKILL_EFFECT:
                    {
                        Player player = (Player)param[0];
                        int from = (int)param[2];
                        int to = (int)param[3];
                        int toType = (int)param[4];

                        if (player.Equals(aiPlayer))
                        {
                            CreateOpponentTarget(true, GameObject.Find("Opponent/Spell"), PlayerInfo.AvatarObject, () =>
                            {
                                aiPlayer.BoardSkills[0].DoOnUpSkillAction();
                            });
                        }
                    }
                    break;
                default: break;
            }
        }
    }
}
