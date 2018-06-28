
/*
/// <summary>
/// The demo player is a subclass of the core HumanPlayer type which extends it with demo-specific
/// functionality. Most of which is straightforward updating of the user interface when receiving
/// new state from the server.
/// </summary>
public class DemoHumanPlayer : DemoPlayer
{
    public static DemoHumanPlayer Instance;

    [SerializeField]
    private GameObject creatureCardViewPrefab;

    [SerializeField]
    private GameObject spellCardViewPrefab;

    [SerializeField]
    private GameObject opponentCardPrefab;

    [SerializeField]
    private GameObject boardCreaturePrefab;

    [SerializeField]
    private GameObject spellTargetingArrowPrefab;

    [SerializeField]
    private GameObject fightTargetingArrowPrefab;

    [SerializeField]
    private GameObject opponentTargetingArrowPrefab;

    public GameUI gameUI;
    //protected PopupChat chatPopup;

    protected float accTime;
    protected float secsAccTime;


    public bool isCardSelected;
    protected GameObject currentCardPreview;
    protected bool isPreviewActive;
    protected int currentPreviewedCardId;
    protected Coroutine createPreviewCoroutine;

    protected AbilitiesController _abilitiesController;

    public Player opponent;

    protected override void Start()
    {
        base.Start();

        GameClient.Get<IPlayerManager>().PlayerGraveyardCards = playerGraveyardCards;
        GameClient.Get<IPlayerManager>().OpponentGraveyardCards = opponentGraveyardCards;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        _rearrangingTopBoard = false;
        _rearrangingBottomBoard = false;

        GraveyardCardsCount = 0;
        OpponentGraveyardCardsCount = 0;

        gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
        Assert.IsNotNull(gameUI);


        lifeStat.onValueChanged += (oldValue, newValue) =>
        {
            CheckGameDynamic();
            gameUI.SetPlayerHealth(lifeStat.effectiveValue);
        };
        manaStat.onValueChanged += (oldValue, newValue) =>
        {
            gameUI.SetPlayerMana(manaStat.maxValue, manaStat.effectiveValue);
            UpdateHandCardsHighlight();
        };

        opponentLifeStat.onValueChanged += (oldValue, newValue) =>
        {
            CheckGameDynamic();
            gameUI.SetOpponentHealth(opponentLifeStat.effectiveValue);
        };
        opponentManaStat.onValueChanged += (oldValue, newValue) =>
        {
            gameUI.SetOpponentMana(opponentManaStat.maxValue, opponentManaStat.effectiveValue);
        };

        deckZone = playerInfo.namedZones["Deck"];
        deckZone.onZoneChanged += numCards =>
        {
            gameUI.SetPlayerDeckCards(numCards);
        };

        handZone = playerInfo.namedZones["Hand"];
        handZone.onZoneChanged += numCards =>
        {
            gameUI.SetPlayerHandCards(numCards);
        };
        handZone.onCardAdded += card =>
        {

        };
        handZone.onCardRemoved += card =>
        {

        };

        boardZone = playerInfo.namedZones["Board"];
        boardZone.onCardRemoved += card =>
        {
           
        };

        graveyardZone = playerInfo.namedZones["Graveyard"];
        graveyardZone.onZoneChanged += numCards =>
        {
            gameUI.SetPlayerGraveyardCards(numCards);
        };

        opponentDeckZone = opponentInfo.namedZones["Deck"];
        opponentDeckZone.onZoneChanged += numCards =>
        {
            gameUI.SetOpponentDeckCards(numCards);
        };

        opponentHandZone = opponentInfo.namedZones["Hand"];
        opponentHandZone.onZoneChanged += numCards =>
        {
            gameUI.SetOpponentHandCards(numCards);
        };
        opponentHandZone.onCardRemoved += card =>
        {

        };

        opponentBoardZone = opponentInfo.namedZones["Board"];
        opponentBoardZone.onCardRemoved += card =>
        {

        };

        opponentGraveyardZone = opponentInfo.namedZones["Graveyard"];
        opponentGraveyardZone.onZoneChanged += numCards =>
        {
            gameUI.SetOpponentGraveyardCards(numCards);
        };
    }



    public override void OnStartGame(StartGameMessage msg)
    {
        base.OnStartGame(msg);

        GameObject.Find("Player/Avatar").GetComponent<PlayerAvatar>().playerInfo = playerInfo;
        GameObject.Find("Opponent/Avatar").GetComponent<PlayerAvatar>().playerInfo = opponentInfo;

        for (var i = 0; i < opponentHandZone.numCards; i++)
        {
            AddCardToOpponentHand();
        }

        RearrangeOpponentHand();

        // Update the UI as appropriate.
        gameUI.SetPlayerHealth(lifeStat.effectiveValue);
        gameUI.SetOpponentHealth(opponentLifeStat.effectiveValue);
        gameUI.SetPlayerMana(manaStat.maxValue, manaStat.effectiveValue);
        gameUI.SetOpponentMana(opponentManaStat.maxValue, opponentManaStat.effectiveValue);

        gameUI.SetPlayerHandCards(handZone.cards.Count);
        gameUI.SetPlayerGraveyardCards(graveyardZone.numCards);
        gameUI.SetPlayerDeckCards(deckZone.numCards);
        gameUI.SetOpponentHandCards(opponentHandZone.numCards);
        gameUI.SetOpponentGraveyardCards(opponentGraveyardZone.numCards);
        gameUI.SetOpponentDeckCards(opponentDeckZone.numCards);


        EffectSolver.EffectActivateEvent += EffectActivateEventHandler;
    }


    private void UpdateOpponentInfo()
    {
        //   opponent.playerInfo = opponentInfo;
        //     opponent.opponentInfo = playerInfo;
        opponent.opponentBoardZone = boardZone;
        opponent.opponentHandZone = handZone;
        opponent.boardZone = opponentBoardZone;
        opponent.handZone = opponentHandZone;
        opponent.playerBoardCardsList = opponentBoardCardsList;
        opponent.opponentBoardCardsList = playerBoardCardsList;
        //opponent.EffectSolver = new EffectSolver(gameState, System.Environment.TickCount);
        //opponent.EffectSolver.SetTriggers(opponentInfo);
        //opponent.EffectSolver.SetTriggers(playerInfo);
    }

    public override void OnStartTurn(StartTurnMessage msg)
    {
        base.OnStartTurn(msg);
     

        if (opponent != null)
        {
            UpdateOpponentInfo();
            opponent.CallOnStartTurnEvent();
        }

        CallOnStartTurnEvent();
    }

    
    public override void OnEndTurn(EndTurnMessage msg)
    {
        base.OnEndTurn(msg);

        if (isHuman)
            CallOnEndTurnEvent();

        if (opponent != null)
            opponent.CallOnEndTurnEvent();
    }

    public override void StopTurn()
    {
        GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.END_TURN);
		var msg = new StopTurnMessage();
        client.Send(NetworkProtocol.StopTurn, msg);
    }

    public override void OnEndGame(EndGameMessage msg)
    {
        base.OnEndGame(msg);


        EffectSolver.EffectActivateEvent -= EffectActivateEventHandler;
    }

    public override void OnCardMoved(CardMovedMessage msg)
    {
        base.OnCardMoved(msg);

        var randomIndex = UnityEngine.Random.Range(0, opponentHandCards.Count);
        var randomCard = opponentHandCards[randomIndex];
        opponentHandCards.Remove(randomCard);

        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

        randomCard.transform.DOMove(Vector3.up * 2.5f, 0.6f).OnComplete(() => 
        {
            //GameClient.Get<ITimerManager>().AddTimer(DestroyRandomCard, new object[] { randomCard }, 1f, false);
            //randomCard.GetComponent<Animator>().SetTrigger("RemoveCard");
            randomCard.transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>().Play();
            
            

            randomCard.transform.DOScale(Vector3.one * 1.2f, 0.6f).OnComplete(() =>
            {
                RemoveOpponentCard(new object[] { randomCard });

                GameClient.Get<ITimerManager>().AddTimer(OnMovedCardCompleted, new object[] { msg }, 0.1f);

                GameClient.Get<ITimerManager>().AddTimer((creat) =>
                {
                    OpponentGraveyardCardsCount++;
                }, null, 1f);

                
            });
        });
        
        randomCard.transform.DORotate(Vector3.zero, 0.5f);

        RearrangeOpponentHand(true);
        gameUI.SetOpponentHandCards(opponentHandCards.Count);
    }

    private void DestroyRandomCard(object[] param)
    {
        GameObject randomCard = param[0] as GameObject;
        Destroy(randomCard);
    }

    private void OnMovedCardCompleted(object[] param)
    {
        CardMovedMessage msg = param[0] as CardMovedMessage;

        var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(msg.card.cardId);

        string cardSetName = string.Empty;
        foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
        {
            if (cardSet.cards.IndexOf(libraryCard) > -1)
                cardSetName = cardSet.name;
        }

        var opponentBoard = opponentInfo.namedZones[Constants.ZONE_BOARD];
        var runtimeCard = opponentBoard.cards[opponentBoard.cards.Count - 1];

        if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.CREATURE)
        {
            effectSolver.SetDestroyConditions(runtimeCard);
            effectSolver.SetTriggers(runtimeCard);
            var boardCreature = Instantiate(boardCreaturePrefab);
            boardCreature.tag = "OpponentOwned";
            boardCreature.GetComponent<BoardCreature>().PopulateWithInfo(runtimeCard, cardSetName);
            boardCreature.transform.parent = GameObject.Find("OpponentBoard").transform;
            opponentBoardCards.Add(boardCreature.GetComponent<BoardCreature>());

            boardCreature.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board
            //PlayArrivalAnimation(boardCreature, libraryCard.cardType);
            RearrangeTopBoard(() =>
            {
                opponentHandZone.numCards -= 1;
                opponentManaStat.baseValue -= libraryCard.cost;

                BoardCreature targetCreature = null;
                PlayerAvatar targetPlayerAvatar = null;
                object target = null;

                if (msg.targetInfo != null && msg.targetInfo.Length > 0)
                {
                    var playerCard = opponentInfo.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == msg.targetInfo[0]);
                    var opponentCard = playerInfo.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == msg.targetInfo[0]);

                    if (opponentCard != null)
                        targetCreature = playerBoardCards.Find(x => x.card.instanceId == opponentCard.instanceId);
                    else if (playerCard != null)
                        targetCreature = opponentBoardCards.Find(x => x.card.instanceId == playerCard.instanceId);
                    else
                    {
                        var playerAvatar = GameObject.Find("PlayerAvatar").GetComponent<PlayerAvatar>();
                        var opponentAvatar = GameObject.Find("OpponentAvatar").GetComponent<PlayerAvatar>();

                        if (playerAvatar.playerInfo.id == msg.targetInfo[0])
                            targetPlayerAvatar = playerAvatar;
                        else if (opponentAvatar.playerInfo.id == msg.targetInfo[0])
                            targetPlayerAvatar = opponentAvatar;
                    }
                }


                bool createTargetArrow = false;

                if(libraryCard.abilities != null && libraryCard.abilities.Count > 0)
                    createTargetArrow = _abilitiesController.IsAbilityCanActivateTargetAtStart(libraryCard.abilities[0]);

                if (targetCreature != null)
                {
                    target = targetCreature;

                    CreateOpponentTarget(createTargetArrow, boardCreature.gameObject, targetCreature.gameObject,
                         () => { CallAbility(libraryCard, null, runtimeCard, Enumerators.CardKind.CREATURE, boardCreature.GetComponent<BoardCreature>(), null, false, target); });
                }
                else if (targetPlayerAvatar != null)
                {
                    target = targetPlayerAvatar;
                    
                    CreateOpponentTarget(createTargetArrow, boardCreature.gameObject, targetPlayerAvatar.gameObject,
                         () => { CallAbility(libraryCard, null, runtimeCard, Enumerators.CardKind.CREATURE, boardCreature.GetComponent<BoardCreature>(), null, false, target); });
                }
                else
                {
                    CallAbility(libraryCard, null, runtimeCard, Enumerators.CardKind.CREATURE, boardCreature.GetComponent<BoardCreature>(), null, false);
                }
            });


            boardCreature.GetComponent<BoardCreature>().PlayArrivalAnimation();
            //GameClient.Get<ITimerManager>().AddTimer(RemoveOpponentCard, new object[] { randomCard }, 0.1f, false);
        }
        else if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.SPELL)
        {
            effectSolver.SetDestroyConditions(runtimeCard);
            effectSolver.SetTriggers(runtimeCard);
            var spellCard = Instantiate(spellCardViewPrefab);
            spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;
            spellCard.GetComponent<SpellCardView>().PopulateWithInfo(runtimeCard, cardSetName);
            spellCard.GetComponent<SpellCardView>().SetHighlightingEnabled(false);

            currentSpellCard = spellCard.GetComponent<SpellCardView>();

            var boardSpell = spellCard.AddComponent<BoardSpell>();

            spellCard.gameObject.SetActive(false);

            opponentManaStat.baseValue -= libraryCard.cost;

            BoardCreature targetCreature = null;
            PlayerAvatar targetPlayerAvatar = null;
            object target = null;

            var playerAvatar = GameObject.Find("PlayerAvatar").GetComponent<PlayerAvatar>();
            var opponentAvatar = GameObject.Find("OpponentAvatar").GetComponent<PlayerAvatar>();

            if (msg.targetInfo != null && msg.targetInfo.Length > 0)
            {
                var playerCard = playerInfo.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == msg.targetInfo[0]);
                var opponentCard = opponentInfo.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == msg.targetInfo[0]);

                if (playerCard != null)
                    targetCreature = playerBoardCards.Find(x => x.card.instanceId == playerCard.instanceId);
                else if (opponentCard != null)
                    targetCreature = opponentBoardCards.Find(x => x.card.instanceId == opponentCard.instanceId);
                else
                {                    
                    if (playerAvatar.playerInfo.id == msg.targetInfo[0])
                        targetPlayerAvatar = playerAvatar;
                    else if (opponentAvatar.playerInfo.id == msg.targetInfo[0])
                        targetPlayerAvatar = opponentAvatar;
                }
            }


            bool createTargetArrow = false;

            if (libraryCard.abilities != null && libraryCard.abilities.Count > 0)
                createTargetArrow = _abilitiesController.IsAbilityCanActivateTargetAtStart(libraryCard.abilities[0]);


            if (targetCreature != null)
            {
                target = targetCreature;

                CreateOpponentTarget(createTargetArrow, opponentAvatar.gameObject, targetCreature.gameObject,
                    () => { CallAbility(libraryCard, null, runtimeCard, Enumerators.CardKind.SPELL, boardSpell, null, false, target); });
            }
            else if (targetPlayerAvatar != null)
            {
                target = targetPlayerAvatar;

                CreateOpponentTarget(createTargetArrow, opponentAvatar.gameObject, targetPlayerAvatar.gameObject, 
                    () => { CallAbility(libraryCard, null, runtimeCard, Enumerators.CardKind.SPELL, boardSpell, null, false, target); });
            }
            else
            {
                CallAbility(libraryCard, null, runtimeCard, Enumerators.CardKind.SPELL, boardSpell, null, false);
            }

            //GameClient.Get<ITimerManager>().AddTimer(RemoveOpponentCard, new object[] { randomCard }, 0.1f, false);
        }
    }

    private void CreateOpponentTarget(bool createTargetArrow, GameObject startObj, GameObject targetObject, Action action)
    {
        if(!createTargetArrow)
        {
            action?.Invoke();
            return;
        }

        var targetingArrow = Instantiate(opponentTargetingArrowPrefab).GetComponent<OpponentTargetingArrow>();
        targetingArrow.opponentBoardZone = boardZone;
        targetingArrow.Begin(startObj.transform.position);

        targetingArrow.SetTarget(targetObject);

        StartCoroutine(RemoveOpponentTargetingArrow(targetingArrow, action));
    }

    private IEnumerator RemoveOpponentSpellCard(SpellCardView spellCard)
    {
        yield return new WaitForSeconds(2.0f);
    }

    private IEnumerator RemoveOpponentTargetingArrow(TargetingArrow arrow, Action action)
    {
        yield return new WaitForSeconds(1f);
        Destroy(arrow.gameObject);

        action?.Invoke();
    }

    public override void OnPlayerAttacked(PlayerAttackedMessage msg)
    {
        base.OnPlayerAttacked(msg);

        var attackingCard = opponentBoardCards.Find(x => x.card.instanceId == msg.attackingCardInstanceId);
        if (attackingCard != null)
        {
            var avatar = GameObject.Find("Player/Avatar").GetComponent<PlayerAvatar>(); ;
            CombatAnimation.PlayFightAnimation(attackingCard.gameObject, avatar.gameObject, 0.1f, () =>
            {
				PlayAttackVFX(attackingCard.card.type, avatar.transform.position, attackingCard.Damage.effectiveValue);

				effectSolver.FightPlayer(msg.attackingPlayerNetId, msg.attackingCardInstanceId);
                attackingCard.CreatureOnAttack(avatar);
            });
        }
    }

    public override void OnCreatureAttacked(CreatureAttackedMessage msg)
    {

        base.OnCreatureAttacked(msg);
        var attackingCard = opponentBoardCards.Find(x => x.card.instanceId == msg.attackingCardInstanceId);
        var attackedCard = playerBoardCards.Find(x => x.card.instanceId == msg.attackedCardInstanceId);

        if (attackingCard != null && attackedCard != null)
        {
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(attackingCard.card.cardId);
    //        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);


            attackingCard.transform.position = new Vector3(attackingCard.transform.position.x, attackingCard.transform.position.y, attackingCard.transform.position.z - 0.2f);

            CombatAnimation.PlayFightAnimation(attackingCard.gameObject, attackedCard.gameObject, 0.5f, () =>
            {
                GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);


                PlayAttackVFX(attackingCard.card.type, attackedCard.transform.position, attackingCard.Damage.effectiveValue);

				effectSolver.FightCreature(msg.attackingPlayerNetId, attackingCard.card, attackedCard.card);
                attackingCard.CreatureOnAttack(attackedCard);

                attackingCard.transform.position = new Vector3(attackingCard.transform.position.x, attackingCard.transform.position.y, attackingCard.transform.position.z + 0.2f);
            });
        }
    }

    private void EffectActivateEventHandler(Enumerators.EffectActivateType effectActivateType, object[] param)
    {
        Debug.LogError("EffectActivateEventHandler");

        switch (effectActivateType)
        {
            case Enumerators.EffectActivateType.PLAY_SKILL_EFFECT:
                {
                    PlayerInfo player = (PlayerInfo)param[0];
                    int from = (int)param[2];
                    int to = (int)param[3];
                    int toType = (int)param[4];

                    Debug.LogError(player.id + " player.id");

                    if (player.Equals(opponent.playerInfo))
                    {
                        CreateOpponentTarget(true, GameObject.Find("Opponent/Spell"), GameObject.Find("Player/Avatar"), () =>
                        {
                            opponent.boardSkill.DoOnUpSkillAction();
                        });
                    }
                }
                break;
            default: break;
        }
    }
} */