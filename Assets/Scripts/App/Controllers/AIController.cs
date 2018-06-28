using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using UnityEngine;
using GrandDevs.CZB.Common;


namespace GrandDevs.CZB
{
    public class AIController : IController
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;

        private BattlegroundController _battlegroundController;
        private CardsController _cardsController;
        private ActionsQueueController _actionsQueueController;
        private AbilitiesController _abilitiesController;

        private int _minTurnForAttack;

        private Dictionary<int, int> numTurnsOnBoard = new Dictionary<int, int>();
      
        private GameObject fightTargetingArrowPrefab;

        private List<int> _attackedCreatureTargets;

        private bool _enabledAIBrain = false;

        private List<ActionItem> allActions;

        private Player PlayerInfo;

        public GameObject currentBoardCreature;
        public BoardCreature currentCreature;
        public CardView currentSpellCard;

        public PlayerAvatar player,
                            opponent;

        public Enumerators.AIType aiType;

        public bool AlreadyAttackedInThisTurn { get; set; }
        public bool IsPlayerStunned { get; set; }
        public bool IsActive { get; set; }



        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
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

            _attackedCreatureTargets = new List<int>();

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


            for(int i =0; i < PlayerInfo.CardsInDeck.Count; i++)
            {
                if (i >= Constants.DEFAULT_CARDS_IN_HAND_AT_START_GAME)
                    break;

                _cardsController.AddCardToHand(PlayerInfo, PlayerInfo.CardsInDeck[i]);
            }

            foreach (var card in PlayerInfo.CardsInHand)
                _cardsController.AddCardToOpponentHand(card);

            _battlegroundController.RearrangeOpponentHand();
        }

        public void OnStartGame()
        {
            if (!_gameplayManager.IsTutorial)
            {
                _minTurnForAttack = UnityEngine.Random.Range(1, 3);
                FillActions();

                player = GameObject.Find("Opponent/Avatar").GetComponent<PlayerAvatar>();
                opponent = GameObject.Find("Player/Avatar").GetComponent<PlayerAvatar>();

                SetAITypeByDeck();
            }
        }


        public void OnEndGame()
        {
            MainApp.Instance.StopAllCoroutines();
        }

        public void OnStartTurn()
        {
            MainApp.Instance.StartCoroutine(RunLogic());
        }


        public void OnEndTurn()
        {
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
                if (!GameClient.Get<ITutorialManager>().IsTutorial)
                {
                    foreach (var skill in PlayerInfo.BoardSkills)
                        skill.OnEndTurn();
                }

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

                    var boardCreatures = new List<WorkingCard>();
                    foreach (var creature in GetBoardCreatures())
                    {
                        boardCreatures.Add(creature);
                    }

                    var usedCreatures = new List<WorkingCard>();

                    if (OpponentHasProvokeCreatures())
                    {
                        foreach (var creature in boardCreatures)
                        {
                            if (creature != null && creature.health > 0 &&
                                (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL) && creature.IsPlayable)
                            {
                                var attackedCreature = GetTargetOpponentCreature();
                                if (attackedCreature != null)
                                {
                                    PlayCreatureAttackSound(creature);

                                   // FightCreature(creature, attackedCreature);

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
                            Debug.Log(creature != null);
                            Debug.Log(creature.health);
                            Debug.Log(numTurnsOnBoard[creature.instanceId] >= 1);
                            Debug.Log(creature.type == Enumerators.CardType.FERAL);
                            Debug.Log(creature.IsPlayable);
                            if (creature != null && creature.health > 0 &&
                                (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL) && creature.IsPlayable)
                            {
                                PlayCreatureAttackSound(creature);

                             //   FightPlayer(creature);

                                yield return new WaitForSeconds(2.0f);
                            }
                        }
                    }
                    else
                    {
                        foreach (var creature in boardCreatures)
                        {
                            Debug.Log(creature != null);
                            Debug.Log(creature.health);
                            Debug.Log(numTurnsOnBoard[creature.instanceId] >= 1);
                            Debug.Log(creature.type == Enumerators.CardType.FERAL);
                            Debug.Log(creature.IsPlayable);
                            if (creature != null && creature.health > 0 &&
                                (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL) && creature.IsPlayable)
                            {
                                Debug.Log("Should Attack");
                                var playerPower = GetPlayerAttackingPower();
                                var opponentPower = GetOpponentAttackingPower();
                                if (playerPower > opponentPower && !GameClient.Get<ITutorialManager>().IsTutorial)
                                {
                                    PlayCreatureAttackSound(creature);

                                  //  FightPlayer(creature);

                                    yield return new WaitForSeconds(2.0f);
                                }
                                else
                                {
                                    var attackedCreature = GetRandomOpponentCreature();
                                    if (attackedCreature != null)
                                    {
                                        PlayCreatureAttackSound(creature);

                                       // FightCreature(creature, attackedCreature);
                                        yield return new WaitForSeconds(2.0f);
                                    }
                                    else
                                    {
                                        PlayCreatureAttackSound(creature);

                                     //   FightPlayer(creature);
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
            if (GameClient.Get<ITutorialManager>().IsTutorial)
                return;

            var boardSkill = PlayerInfo.BoardSkills[0];

            if (PlayerInfo.Mana >= boardSkill.manaCost)
            {
               // GetServer().gameState.currentPlayer.namedStats[Constants.TAG_MANA].baseValue -= boardSkill.manaCost;
                //   playerInfo.namedStats[Constants.TAG_MANA].baseValue -= boardSkill.manaCost;

                int target = 0;

                Enumerators.AffectObjectType selectedObjectType = Enumerators.AffectObjectType.NONE;

                switch (boardSkill.skillType)
                {
                    case Enumerators.SetType.EARTH:
                        {
                            selectedObjectType = Enumerators.AffectObjectType.PLAYER;
                            target = 1;
                        }
                        break;
                    case Enumerators.SetType.LIFE:
                        {
                            target = 1;
                            selectedObjectType = Enumerators.AffectObjectType.PLAYER;

                            var creatures = GetCreaturesWithLowHP();

                            if (creatures.Count > 0)
                            {
                                target = creatures[0].instanceId;
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                        }
                        break;
                    case Enumerators.SetType.FIRE:
                    case Enumerators.SetType.TOXIC:
                    case Enumerators.SetType.WATER:
                        {
                            target = 0;
                            selectedObjectType = Enumerators.AffectObjectType.PLAYER;

                            var creature = GetRandomOpponentCreature();

                            if (creature != null)
                            {
                                target = creature.instanceId;
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                        }
                        break;
                    case Enumerators.SetType.AIR:
                        {
                            var creatures = GetCreaturesWithLowHP();

                            if (creatures.Count > 0)
                            {
                                target = creatures[0].instanceId;
                                selectedObjectType = Enumerators.AffectObjectType.CHARACTER;
                            }
                            else
                            {
                                var creature = GetRandomOpponentCreature();

                                if (creature != null)
                                {
                                    target = creature.instanceId;
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
                    boardSkill.fightTargetingArrow = CreateOpponentTarget(true, boardSkill.gameObject, target == 0 ? opponent.gameObject : player.gameObject, () =>
                    {
                        boardSkill.fightTargetingArrow.selectedPlayer = target == 0 ? opponent : player;
                        boardSkill.DoOnUpSkillAction();
                    });
                }
                else
                {
                    var creature = _battlegroundController.opponentBoardCards.Find(x => x.Card.instanceId == target);

                    if (creature == null)
                        creature = _battlegroundController.playerBoardCards.Find(x => x.Card.instanceId == target);

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

        protected void TryToUseBoardWeapon()
        {
            if (PlayerInfo.CurrentBoardWeapon != null && PlayerInfo.CurrentBoardWeapon.CanAttack)
            {
                var target = GetRandomOpponentCreature();

                if (target != null)
                {
                    var creature = _battlegroundController.playerBoardCards.Find(x => x.Card.instanceId == target.instanceId);

                    PlayerInfo.CurrentBoardWeapon.ImmediatelyAttack(creature);
                }
                else
                {
                    PlayerInfo.CurrentBoardWeapon.ImmediatelyAttack(opponent);
                }
            }
        }

        protected bool TryToPlayCard(WorkingCard card)
        {
            var availableMana = PlayerInfo.Mana;

            if ((card.libraryCard.cost <= availableMana && _battlegroundController.currentTurn > _minTurnForAttack) || Constants.DEV_MODE)
            {
                List<int> target = GetAbilityTarget(card);
                if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.CREATURE && _battlegroundController.opponentBoardCards.Count < Constants.MAX_BOARD_CREATURES)
                {
                    //if (libraryCard.abilities.Find(x => x.abilityType == Enumerators.AbilityType.CARD_RETURN) != null)
                    //    if (target.Count == 0)
                    //        return false;

                    PlayerInfo.RemoveCardFromHand(card);
                    PlayerInfo.AddCardToBoard(card);

                    numTurnsOnBoard[card.instanceId] = 0;

                  //  PlayCreatureCard(card, target);

                    AddCardInfo(card);

                //    if (GameClient.Get<ITutorialManager>().IsTutorial && card.cardId == 11)
                //        FightCreatureBySkill(1, card);

                }
                else if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
                {
                    if (target != null)
                    {
                     //   PlaySpellCard(card, target);

                        AddCardInfo(card);
                    }
                }
                return true;
            }
            return false;
        }

        protected List<int> GetAbilityTarget(WorkingCard card)
        {
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

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
                var targetInfo = new List<int>();
                foreach (var ability in abilitiesWithTarget)
                {
                    switch (ability.abilityType)
                    {
                        case Enumerators.AbilityType.ADD_GOO_VIAL:
                            {
                                targetInfo.Add(1);
                            }
                            break;
                        case Enumerators.AbilityType.CARD_RETURN:
                            {
                                if (!AddRandomTargetCreature(true, ref targetInfo, false, true))
                                {
                                    AddRandomTargetCreature(false, ref targetInfo, true, true);
                                }
                            }
                            break;
                        case Enumerators.AbilityType.DAMAGE_TARGET:
                            {
                                CheckAndAddTargets(ability, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.DAMAGE_TARGET_ADJUSTMENTS:
                            {
                                if (!AddRandomTargetCreature(true, ref targetInfo))
                                    targetInfo.Add(0);
                            }
                            break;
                        case Enumerators.AbilityType.MASSIVE_DAMAGE:
                            {
                                AddRandomTargetCreature(true, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.MODIFICATOR_STATS:
                            {
                                if (ability.value > 0)
                                    AddRandomTargetCreature(false, ref targetInfo);
                                else
                                    AddRandomTargetCreature(true, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.STUN:
                            {
                                CheckAndAddTargets(ability, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.STUN_OR_DAMAGE_ADJUSTMENTS:
                            {
                                CheckAndAddTargets(ability, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.CHANGE_STAT:
                            {
                                if (ability.value > 0)
                                    AddRandomTargetCreature(false, ref targetInfo);
                                else
                                    AddRandomTargetCreature(true, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.SUMMON:
                            {

                            }
                            break;
                        case Enumerators.AbilityType.WEAPON:
                            {
                                targetInfo.Add(1);
                            }
                            break;
                        case Enumerators.AbilityType.SPURT:
                            {
                                AddRandomTargetCreature(true, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.SPELL_ATTACK:
                            {
                                CheckAndAddTargets(ability, ref targetInfo);
                            }
                            break;
                        case Enumerators.AbilityType.HEAL:
                            {
                                var creatures = GetCreaturesWithLowHP();

                                if (creatures.Count > 0)
                                {
                                    targetInfo.Add(creatures[UnityEngine.Random.Range(0, creatures.Count)].instanceId);
                                }
                                else
                                {
                                    targetInfo.Add(1);
                                }
                            }
                            break;
                        case Enumerators.AbilityType.DOT:
                            {
                                CheckAndAddTargets(ability, ref targetInfo);
                            }
                            break;
                        default: break;
                    }

                    return targetInfo; // hack to handle only one ability
                }

                return targetInfo;
            }
            else
            {
                return null;
            }
        }

        private void CheckAndAddTargets(GrandDevs.CZB.Data.AbilityData ability, ref List<int> targetInfo)
        {
            if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
            {
                AddRandomTargetCreature(true, ref targetInfo);
            }
            else if (ability.abilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT))
            {
                targetInfo.Add(0);
            }
        }

        private bool AddRandomTargetCreature(bool opponent, ref List<int> targetInfo, bool lowHP = false, bool addAttackIgnore = false)
        {
            WorkingCard target = null;

            if (opponent)
                target = GetRandomOpponentCreature();
            else
                target = GetRandomCreature(lowHP);

            if (target != null)
            {
                targetInfo.Add(target.instanceId);

                if (addAttackIgnore)
                    _attackedCreatureTargets.Add(target.instanceId);

                return true;
            }

            return false;
        }

        protected virtual void AddCardInfo(WorkingCard card)
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
                prefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/CreatureCard");
            }
            else if ((Enumerators.CardKind)card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                prefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/SpellCard");
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

        protected void DestroyCardInfo(object[] param)
        {
            MonoBehaviour.Destroy((GameObject)param[0]);
        }

        protected int GetPlayerAttackingPower()
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

        protected int GetOpponentAttackingPower()
        {
            var power = 0;
            foreach (var card in PlayerInfo.CardsOnBoard)
            {
                power += card.damage;
            }
            return power;
        }


        protected List<WorkingCard> GetCreaturesWithLowHP()
        {
            List<WorkingCard> finalList = new List<WorkingCard>();

            var list = GetBoardCreatures();

            foreach (var item in list)
            {
                if (item.health < item.initialHealth)
                    finalList.Add(item);
            }

            list = list.OrderBy(x => x.health).OrderBy(y => y.health.ToString().Length).ToList();

            return finalList;
        }

        protected List<WorkingCard> GetCreatureCardsInHand()
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

        protected List<WorkingCard> GetSpellCardsInHand()
        {
            return PlayerInfo.CardsInHand.FindAll(x => x.libraryCard.cardKind == Enumerators.CardKind.SPELL);
        }

        protected List<WorkingCard> GetBoardCreatures()
        {
            var board = PlayerInfo.CardsOnBoard;
            var eligibleCreatures = board.FindAll(x => x.health > 0);
            return eligibleCreatures;
        }

        protected WorkingCard GetRandomCreature(bool lowHP = false)
        {
            var board = PlayerInfo.CardsOnBoard;
            List<WorkingCard> eligibleCreatures = null;

            if (!lowHP)
                eligibleCreatures = board.FindAll(x => x.health > 0 && !_attackedCreatureTargets.Contains(x.instanceId));
            else
                eligibleCreatures = board.FindAll(x => x.health < x.initialHealth && !_attackedCreatureTargets.Contains(x.instanceId));

            if (eligibleCreatures.Count > 0)
            {
                return eligibleCreatures[UnityEngine.Random.Range(0, eligibleCreatures.Count)];
            }
            return null;
        }

        protected WorkingCard GetTargetOpponentCreature()
        {
            var board = PlayerInfo.CardsOnBoard;
            var eligibleCreatures = board.FindAll(x => x.health > 0);

            if (eligibleCreatures.Count > 0)
            {
                var provokeCreatures = eligibleCreatures.FindAll(x => x.type == Enumerators.CardType.HEAVY);
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

        protected WorkingCard GetRandomOpponentCreature()
        {
            var board = PlayerInfo.CardsOnBoard;

            var eligibleCreatures = board.FindAll(x => x.health > 0 && !_attackedCreatureTargets.Contains(x.instanceId));
            if (eligibleCreatures.Count > 0)
            {
                return eligibleCreatures[UnityEngine.Random.Range(0, eligibleCreatures.Count)];
            }
            return null;
        }

        protected bool OpponentHasProvokeCreatures()
        {
            var board = PlayerInfo.CardsOnBoard;
            var eligibleCreatures = board.FindAll(x => x.health > 0);
            if (eligibleCreatures.Count > 0)
            {
                var provokeCreatures = eligibleCreatures.FindAll(x => x.type == Enumerators.CardType.HEAVY);
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
    }
}
