using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using UnityEngine;
using GrandDevs.CZB.Common;
using DG.Tweening;

namespace GrandDevs.CZB
{
    public class AIController : IController
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private ILoadObjectsManager _loadObjectsManager;

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

        private List<BoardCreature> _attackedCreatureTargets;

        private bool _enabledAIBrain = true;

        private List<ActionItem> allActions;

        private GameObject _boardCreaturePrefab;

        private Player PlayerInfo;

        public GameObject currentBoardCreature;
        public BoardCreature currentCreature;
        public CardView currentSpellCard;

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

            _attackedCreatureTargets = new List<BoardCreature>();

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
                            //card.cardId = 1;
                        }
                        playerDeck.Add(card.cardId);
                    }
                }

            }

            PlayerInfo.SetDeck(playerDeck);

            PlayerInfo.SetFirstHand(_gameplayManager.IsTutorial);

            _battlegroundController.RearrangeOpponentHand();


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
            var deck = GameClient.Get<IDataManager>().CachedOpponentDecksData.decks[GameClient.Get<IGameplayManager>().OpponentDeckId];
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
                foreach (var creature in GetCreatureCardsInHand())
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
                if (GameClient.Get<ITutorialManager>().IsTutorial && GameClient.Get<ITutorialManager>().CurrentStep == 11)
                {
                    (GameClient.Get<ITutorialManager>() as TutorialManager).paused = true;
                }
                else
                {
                    yield return new WaitForSeconds(2.0f);

                    var boardCreatures = new List<BoardCreature>();
                    foreach (var creature in GetBoardCreatures())
                        boardCreatures.Add(creature);

                    var usedCreatures = new List<BoardCreature>();

                    if (OpponentHasProvokeCreatures())
                    {
                        foreach (var creature in boardCreatures)
                        {
                            if (creature != null && creature.HP > 0 &&
                                (numTurnsOnBoard[creature.Card.instanceId] >= 1 || creature.Card.type == Enumerators.CardType.FERAL) && creature.IsPlayable)
                            {
                                var attackedCreature = GetTargetOpponentCreature();
                                if (attackedCreature != null)
                                {
                                    PlayCreatureAttackSound(creature.Card);

                                    AttackCreature(creature, attackedCreature);

                                    _battleController.AttackCreatureByCreature(creature, attackedCreature);

                                    usedCreatures.Add(creature);
                                    yield return new WaitForSeconds(2.0f);
                                    if (!OpponentHasProvokeCreatures())
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

                    var totalPower = GetPlayerAttackingPower();
                    if ((totalPower >= _gameplayManager.GetLocalPlayer().HP ||
                        (aiType == Enumerators.AIType.BLITZ_AI ||
                         aiType == Enumerators.AIType.TIME_BLITZ_AI)) && !GameClient.Get<ITutorialManager>().IsTutorial)
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
                                var playerPower = GetPlayerAttackingPower();
                                var opponentPower = GetOpponentAttackingPower();
                                if (playerPower > opponentPower && !GameClient.Get<ITutorialManager>().IsTutorial)
                                {
                                    PlayCreatureAttackSound(creature.Card);

                                    _battleController.AttackPlayerByCreature(creature, _gameplayManager.GetLocalPlayer());

                                    yield return new WaitForSeconds(2.0f);
                                }
                                else
                                {
                                    var attackedCreature = GetRandomOpponentCreature();
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

                    if (!GameClient.Get<ITutorialManager>().IsTutorial)
                        foreach (var skill in PlayerInfo.BoardSkills)
                            skill.OnEndTurn();

                    _battlegroundController.StopTurn();
                }
            }
        }

        private void PlayCreatureAttackSound(WorkingCard card)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);
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

                            var creatures = GetCreaturesWithLowHP();

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

                            var creature = GetRandomOpponentCreature();

                            if (creature != null)
                            {
                                target = creature;
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                        }
                        break;
                    case Enumerators.SetType.AIR:
                        {
                            var creatures = GetCreaturesWithLowHP();

                            if (creatures.Count > 0)
                            {
                                target = creatures[0];
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                            else
                            {
                                var creature = GetRandomOpponentCreature();

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
                    var creature = target as BoardCreature;

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

        private OpponentTargetingArrow CreateOpponentTarget(bool createTargetArrow, GameObject startObj, GameObject targetObject, System.Action action)
        {
            if (!createTargetArrow)
            {
                action?.Invoke();
                return null;
            }

            var targetingArrow = MonoBehaviour.Instantiate(fightTargetingArrowPrefab).GetComponent<OpponentTargetingArrow>();
            targetingArrow.Begin(startObj.transform.position);

            targetingArrow.SetTarget(targetObject);

            MainApp.Instance.StartCoroutine(RemoveOpponentTargetingArrow(targetingArrow, action));

            return targetingArrow;
        }

        private IEnumerator RemoveOpponentTargetingArrow(TargetingArrow arrow, System.Action action)
        {
            yield return new WaitForSeconds(1f);
            MonoBehaviour.Destroy(arrow.gameObject);

            action?.Invoke();
        }

        private void TryToUseBoardWeapon()
        {
            if (PlayerInfo.CurrentBoardWeapon != null && PlayerInfo.CurrentBoardWeapon.CanAttack)
            {
                var target = GetRandomOpponentCreature();

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

            var abilitiesWithTarget = new List<GrandDevs.CZB.Data.AbilityData>();

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
                                if (!AddRandomTargetCreature(true, ref target, false, true))
                                {
                                    AddRandomTargetCreature(false, ref target, true, true);
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
                                if (!AddRandomTargetCreature(true, ref target))
                                    target = localPlayer;
                            }
                            break;
                        case Enumerators.AbilityType.MASSIVE_DAMAGE:
                            {
                                AddRandomTargetCreature(true, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.MODIFICATOR_STATS:
                            {
                                if (ability.value > 0)
                                    AddRandomTargetCreature(false, ref target);
                                else
                                    AddRandomTargetCreature(true, ref target);
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
                                    AddRandomTargetCreature(false, ref target);
                                else
                                    AddRandomTargetCreature(true, ref target);
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
                                AddRandomTargetCreature(true, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.SPELL_ATTACK:
                            {
                                CheckAndAddTargets(ability, ref target);
                            }
                            break;
                        case Enumerators.AbilityType.HEAL:
                            {
                                var creatures = GetCreaturesWithLowHP();

                                if (creatures.Count > 0)
                                {
                                    target = creatures[UnityEngine.Random.Range(0, creatures.Count)];
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

        private void CheckAndAddTargets(GrandDevs.CZB.Data.AbilityData ability, ref object targetInfo)
        {
            if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
            {
                AddRandomTargetCreature(true, ref targetInfo);
            }
            else if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT))
            {
                targetInfo = localPlayer;
            }
        }

        private bool AddRandomTargetCreature(bool opponent, ref object targetInfo, bool lowHP = false, bool addAttackIgnore = false)
        {
            BoardCreature target = null;

            if (opponent)
                target = GetRandomOpponentCreature();
            else
                target = GetRandomCreature(lowHP);

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
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            GameObject prefab = null;
            if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            }
            else if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");
            }
            GameObject go = MonoBehaviour.Instantiate(prefab);

            var cardView = go.GetComponent<CardView>();
            cardView.PopulateWithInfo(card, cardSetName);
            go.transform.position = new Vector3(-6, 0, 0);
            go.transform.localScale = Vector3.one * .3f;
            cardView.SetHighlightingEnabled(false);

            GameClient.Get<ITimerManager>().StopTimer(DestroyCardInfo);
            GameClient.Get<ITimerManager>().AddTimer(DestroyCardInfo, new object[] { go }, 2, false);
        }

        private void DestroyCardInfo(object[] param)
        {
            MonoBehaviour.Destroy((GameObject)param[0]);
        }

        private int GetPlayerAttackingPower()
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

        private int GetOpponentAttackingPower()
        {
            var power = 0;
            foreach (var card in localPlayer.CardsOnBoard)
            {
                power += card.damage;
            }
            return power;
        }


        private List<BoardCreature> GetCreaturesWithLowHP()
        {
            List<BoardCreature> finalList = new List<BoardCreature>();

            var list = GetBoardCreatures();

            foreach (var item in list)
            {
                if (item.HP < item.initialHP)
                    finalList.Add(item);
            }

            list = list.OrderBy(x => x.HP).OrderBy(y => y.HP.ToString().Length).ToList();

            return finalList;
        }

        private List<WorkingCard> GetCreatureCardsInHand()
        {
            List<WorkingCard> list = PlayerInfo.CardsInHand.FindAll(x => x.libraryCard.cardKind == Enumerators.CardKind.CREATURE);

            List<Data.Card> cards = new List<Data.Card>();

            foreach (var item in list)
                cards.Add(GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(item.cardId));

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

        private List<BoardCreature> GetBoardCreatures()
        {
            var board = PlayerInfo.BoardCards;
            var eligibleCreatures = board.FindAll(x => x.HP > 0);
            return eligibleCreatures;
        }

        private BoardCreature GetRandomCreature(bool lowHP = false)
        {
            var board = PlayerInfo.BoardCards;
            List<BoardCreature> eligibleCreatures = null;

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

        private BoardCreature GetTargetOpponentCreature()
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

        private BoardCreature GetRandomOpponentCreature()
        {
            var board = localPlayer.BoardCards;

            var eligibleCreatures = board.FindAll(x => x.HP > 0 && !_attackedCreatureTargets.Contains(x));
            if (eligibleCreatures.Count > 0)
            {
                return eligibleCreatures[UnityEngine.Random.Range(0, eligibleCreatures.Count)];
            }
            return null;
        }

        private bool OpponentHasProvokeCreatures()
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

            var allActionsType = GameClient.Get<IDataManager>().CachedOpponentDecksData.decks[GameClient.Get<IGameplayManager>().OpponentDeckId].opponentActions;
            allActions = GameClient.Get<IDataManager>().CachedActionsLibraryData.GetActions(allActionsType.ToArray());
        }

        private void MoveCard(WorkingCard card, object target)
        {
            var randomCard = _battlegroundController.opponentHandCards[0];

            _battlegroundController.opponentHandCards.Remove(randomCard);

            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            randomCard.transform.DOMove(Vector3.up * 2.5f, 0.6f).OnComplete(() =>
            {
                //GameClient.Get<ITimerManager>().AddTimer(DestroyRandomCard, new object[] { randomCard }, 1f, false);
                //randomCard.GetComponent<Animator>().SetTrigger("RemoveCard");
                randomCard.transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>().Play();

                randomCard.transform.DOScale(Vector3.one * 1.2f, 0.6f).OnComplete(() =>
                {
                    _cardsController.RemoveOpponentCard(new object[] { randomCard });

                    GameClient.Get<ITimerManager>().AddTimer(OnMovedCardCompleted, new object[] { card, target }, 0.1f);

                    GameClient.Get<ITimerManager>().AddTimer((creat) =>
                    {
                        PlayerInfo.GraveyardCardsCount++;
                    }, null, 1f);


                });
            });

            randomCard.transform.DORotate(Vector3.zero, 0.5f);

            _battlegroundController.RearrangeOpponentHand(true);
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
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            var runtimeCard = PlayerInfo.CardsOnBoard[PlayerInfo.CardsOnBoard.Count - 1];

            if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                var boardCreatureElement = new BoardCreature(GameObject.Find("OpponentBoard").transform);
                var boardCreature = boardCreatureElement.gameObject;
                boardCreature.tag = "OpponentOwned";
                boardCreatureElement.ownerPlayer = card.owner;

                boardCreatureElement.PopulateWithInfo(runtimeCard, cardSetName);
                _battlegroundController.opponentBoardCards.Add(boardCreatureElement);

                boardCreature.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board
                                                                     //PlayArrivalAnimation(boardCreature, libraryCard.cardType);

                PlayerInfo.BoardCards.Add(boardCreatureElement);

                _battlegroundController.RearrangeTopBoard(() =>
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
                         else if (target is BoardCreature)
                             targetObject = (target as BoardCreature).gameObject;

                         CreateOpponentTarget(createTargetArrow, boardCreature.gameObject, targetObject,
                                  () => { _abilitiesController.CallAbility(card.libraryCard, null, runtimeCard, Enumerators.CardKind.CREATURE, boardCreatureElement, null, false, null, target); });

                     }
                     else
                     {
                         _abilitiesController.CallAbility(card.libraryCard, null, runtimeCard, Enumerators.CardKind.CREATURE, boardCreatureElement, null, false, null);
                     }
                 });


                boardCreatureElement.PlayArrivalAnimation();

                _battlegroundController.RearrangeTopBoard();
                //GameClient.Get<ITimerManager>().AddTimer(RemoveOpponentCard, new object[] { randomCard }, 0.1f, false);
            }
            else if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                var spellCard = MonoBehaviour.Instantiate(_cardsController.spellCardViewPrefab);
                spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;
                spellCard.GetComponent<SpellCardView>().PopulateWithInfo(runtimeCard, cardSetName);
                spellCard.GetComponent<SpellCardView>().SetHighlightingEnabled(false);

                currentSpellCard = spellCard.GetComponent<SpellCardView>();

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
                    else if (target is BoardCreature)
                        targetObject = (target as BoardCreature).gameObject;

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

        public void AttackPlayer(BoardCreature attackingCard, Player attackedPlayer)
        {
            if (attackingCard != null && attackedPlayer != null)
            {
                _actionsQueueController.AddNewActionInToQueue((parameter, completeCallback) =>
                {
                    _animationsController.PlayFightAnimation(attackingCard.gameObject, attackedPlayer.AvatarObject, 0.1f, () =>
                    {
                        _vfxController.PlayAttackVFX(attackingCard.Card.type, attackedPlayer.AvatarObject.transform.position, attackingCard.Damage);

                        attackingCard.CreatureOnAttack(attackedPlayer);

                        completeCallback?.Invoke();
                    });
                });
            }
        }
    
        public void AttackCreature(BoardCreature attackingCard, BoardCreature attackedCard)
        {
            if (attackingCard != null && attackedCard != null)
            {
                //        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                _actionsQueueController.AddNewActionInToQueue((parameter, completeCallback) =>
                {
                    attackingCard.transform.position = new Vector3(attackingCard.transform.position.x, attackingCard.transform.position.y, attackingCard.transform.position.z - 0.2f);

                    _animationsController.PlayFightAnimation(attackingCard.gameObject, attackedCard.gameObject, 0.5f, () =>
                    {
                        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, attackingCard.Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);


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
