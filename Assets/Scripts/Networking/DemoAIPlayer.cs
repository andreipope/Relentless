// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CCGKit;
using GrandDevs.CZB;
using GrandDevs.CZB.Common;

/// <summary>
/// Computer-controlled player that is used in the single-player mode from the demo game.
/// </summary>
public class DemoAIPlayer : DemoPlayer
{
    /// <summary>
    /// Cached reference to the human opponent player.
    /// </summary>
    protected Player humanPlayer;

    private int _minTurnForAttack;
    /// <summary>
    /// True if the current game has ended; false otherwise.
    /// </summary>
    protected bool gameEnded;

    protected Dictionary<int, int> numTurnsOnBoard = new Dictionary<int, int>();

    /// <summary>
    /// Unity's Awake.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        isHuman = false;
    }

    /// <summary>
    /// Called when the game starts.
    /// </summary>
    /// <param name="msg">Start game message.</param>
    public override void OnStartGame(StartGameMessage msg)
    {
        base.OnStartGame(msg);
        humanPlayer = NetworkingUtils.GetHumanLocalPlayer();
        if (!GameManager.Instance.tutorial)
            _minTurnForAttack = Random.Range(1,3);
    }

    /// <summary>
    /// Called when the game ends.
    /// </summary>
    /// <param name="msg">End game message.</param>
    public override void OnEndGame(EndGameMessage msg)
    {
        base.OnEndGame(msg);
        gameEnded = true;
        StopAllCoroutines();
    }

    /// <summary>
    /// Called when a new turn for this player starts.
    /// </summary>
    /// <param name="msg">Start turn message.</param>
    public override void OnStartTurn(StartTurnMessage msg)
    {

        base.OnStartTurn(msg);
        if (msg.isRecipientTheActivePlayer)
        {
            StartCoroutine(RunLogic());
        }
    }

    /// <summary>
    /// Called when the current turn ends.
    /// </summary>
    /// <param name="msg">End turn message.</param>
    public override void OnEndTurn(EndTurnMessage msg)
    {
        base.OnEndTurn(msg);
        StopAllCoroutines();

        foreach (var card in playerInfo.namedZones["Board"].cards)
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

    /// <summary>
    /// This method runs the AI logic asynchronously.
    /// </summary>
    /// <returns>The AI logic coroutine.</returns>
    private IEnumerator RunLogic()
    {
        if (gameEnded)
        {
            yield return null;
        }

        // Simulate 'thinking' time. This could be random or dependent on the
        // complexity of the board state for increased realism.
        yield return new WaitForSeconds(2.0f);
        // Actually perform the AI logic in a separate coroutine.
        StartCoroutine(PerformMove());
    }

    /// <summary>
    /// This methods performs the actual AI logic.
    /// </summary>
    /// <returns>The AI logic coroutine.</returns>
    protected virtual IEnumerator PerformMove()
    {
        foreach (var creature in GetCreatureCardsInHand())
        {
            if (TryToPlayCard(creature))
            {
                yield return new WaitForSeconds(2.0f);
            }
        }

        foreach (var spell in GetSpellCardsInHand())
        {
            if (TryToPlayCard(spell))
            {
                yield return new WaitForSeconds(2.0f);
            }
        }

        yield return new WaitForSeconds(2.0f);

        var boardCreatures = new List<RuntimeCard>();
        foreach (var creature in GetBoardCreatures())
        {
            boardCreatures.Add(creature);
        }

        var usedCreatures = new List<RuntimeCard>();

        if (OpponentHasProvokeCreatures())
        {
            foreach (var creature in boardCreatures)
            {
                if (creature != null && creature.namedStats["HP"].effectiveValue > 0 &&
                    (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL) && creature.isPlayable)
                {
                    var attackedCreature = GetTargetOpponentCreature();
                    if (attackedCreature != null)
                    {
                        FightCreature(creature, attackedCreature);
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
        if (totalPower >= opponentInfo.namedStats["Life"].effectiveValue)
        {
            foreach (var creature in boardCreatures)
            {
                if (creature != null && creature.namedStats["HP"].effectiveValue > 0 &&
                    (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL) && creature.isPlayable)
                {
					FightPlayer(creature);
                    yield return new WaitForSeconds(2.0f);
                }
            }
        }
        else
        {
            foreach (var creature in boardCreatures)
            {
                if (creature != null && creature.namedStats["HP"].effectiveValue > 0 &&
                    (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL) && creature.isPlayable)
                {
					var playerPower = GetPlayerAttackingPower();
                    var opponentPower = GetOpponentAttackingPower();
                    if (playerPower > opponentPower)
                    {
                        FightPlayer(creature);
                        yield return new WaitForSeconds(2.0f);
                    }
                    else
                    {
                        var attackedCreature = GetRandomOpponentCreature();
                        if (attackedCreature != null)
                        {
                            FightCreature(creature, attackedCreature);
                            yield return new WaitForSeconds(2.0f);
                        }
                        else
                        {
                            FightPlayer(creature);
                            yield return new WaitForSeconds(2.0f);
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(1.0f);
        StopTurn();
    }

    protected bool TryToPlayCard(RuntimeCard card)
    {
        var availableMana = playerInfo.namedStats["Mana"].effectiveValue;

        var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

        if (libraryCard.cost <= availableMana && CurrentTurn > _minTurnForAttack)
        {

            List<int> target = null; //= GetAbilityTarget(card);
            if ((Enumerators.CardKind)libraryCard.cardTypeId == Enumerators.CardKind.CREATURE)
            {
                playerInfo.namedZones["Hand"].RemoveCard(card);
                playerInfo.namedZones["Board"].AddCard(card);
                numTurnsOnBoard[card.instanceId] = 0;
                PlayCreatureCard(card, target);
                AddCardInfo(card);
            }
            else if ((Enumerators.CardKind)libraryCard.cardTypeId == Enumerators.CardKind.SPELL)
            {
                if (target != null)
                {
                    PlaySpellCard(card, target);
                    AddCardInfo(card);
                }
            }
            return true;
        }
        return false;
    }

    protected virtual void AddCardInfo(RuntimeCard card)
    {
        var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

        string cardSetName = string.Empty;
        foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
        {
            if (cardSet.cards.IndexOf(libraryCard) > -1)
                cardSetName = cardSet.name;
        }

        GameObject prefab = null;
        if ((Enumerators.CardKind)libraryCard.cardTypeId == Enumerators.CardKind.CREATURE)
        {
            prefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/CreatureCard");
        }
        else if ((Enumerators.CardKind)libraryCard.cardTypeId == Enumerators.CardKind.SPELL)
        {
            prefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/SpellCard");
        }
        GameObject go = MonoBehaviour.Instantiate(prefab);

        var cardView = go.GetComponent<CardView>();
        cardView.PopulateWithInfo(card, cardSetName);
        go.transform.position = new Vector3(-6, 0, 0);
        go.transform.localScale = Vector3.one * 1.5f;
        GameClient.Get<ITimerManager>().AddTimer((x) => { Destroy(go); }, null, 2, false);
    }


    protected List<int> GetAbilityTarget(RuntimeCard card)
    {
        var config = GameManager.Instance.config;
        //var boardZoneId = config.gameZones.Find(x => x.name == "Board").id;
        var libraryCard = config.GetCard(card.cardId);
        var triggeredAbilities = libraryCard.abilities.FindAll(x => x is CCGKit.TriggeredAbility);

        var needsToSelectTarget = false;
        foreach (var ability in triggeredAbilities)
        {
            var triggeredAbility = ability as CCGKit.TriggeredAbility;
            var trigger = triggeredAbility.trigger as OnCardEnteredZoneTrigger;
           /* if (trigger != null && trigger.zoneId == boardZoneId && triggeredAbility.target is IUserTarget)
            {
                needsToSelectTarget = true;
                break;
            }   */
        }

        if (needsToSelectTarget)
        {
            var targetInfo = new List<int>();
            foreach (var ability in triggeredAbilities)
            {
                var triggeredAbility = ability as CCGKit.TriggeredAbility;
                if (IsBuffEffect(triggeredAbility.effect))
                {
                    if (triggeredAbility.effect is PlayerEffect)
                    {
                        targetInfo.Add(0);
                    }
                    else if (triggeredAbility.effect is CardEffect)
                    {
                        var target = GetRandomCreature();
                        if (target != null)
                        {
                            //targetInfo.Add(boardZoneId);
                            targetInfo.Add(target.instanceId);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    if (triggeredAbility.effect is PlayerEffect)
                    {
                        targetInfo.Add(1);
                    }
                    else if (triggeredAbility.effect is CardEffect)
                    {
                        var target = GetRandomOpponentCreature();
                        if (target != null)
                        {
                            //targetInfo.Add(boardZoneId);
                            targetInfo.Add(target.instanceId);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            return targetInfo;
        }
        else
        {
            return null;
        }
    }

    protected int GetPlayerAttackingPower()
    {
        var power = 0;
        foreach (var creature in playerInfo.namedZones["Board"].cards)
        {
            if (creature.namedStats["HP"].effectiveValue > 0 &&
                (numTurnsOnBoard[creature.instanceId] >= 1 || creature.type == Enumerators.CardType.FERAL))
            {
                power += creature.namedStats["DMG"].effectiveValue;
            }
        }
        return power;
    }

    protected int GetOpponentAttackingPower()
    {
        var power = 0;
        foreach (var card in opponentInfo.namedZones["Board"].cards)
        {
            power += card.namedStats["DMG"].effectiveValue;
        }
        return power;
    }

    protected bool IsBuffEffect(Effect effect)
    {
        return effect is IncreasePlayerStatEffect || effect is IncreaseCardStatEffect;
    }

    protected List<RuntimeCard> GetCreatureCardsInHand()
    {
        return playerInfo.namedZones["Hand"].cards.FindAll(x => x.cardType.name == "Creature");
    }

    protected List<RuntimeCard> GetSpellCardsInHand()
    {
        return playerInfo.namedZones["Hand"].cards.FindAll(x => x.cardType.name == "Spell");
    }

    protected List<RuntimeCard> GetBoardCreatures()
    {
        var board = playerInfo.namedZones["Board"].cards;
        var eligibleCreatures = board.FindAll(x => x.namedStats["HP"].effectiveValue > 0);
        return eligibleCreatures;
    }

    protected RuntimeCard GetRandomCreature()
    {
        var board = playerInfo.namedZones["Board"].cards;
        var eligibleCreatures = board.FindAll(x => x.namedStats["HP"].effectiveValue > 0);
        if (eligibleCreatures.Count > 0)
        {
            return eligibleCreatures[Random.Range(0, eligibleCreatures.Count)];
        }
        return null;
    }

    protected RuntimeCard GetTargetOpponentCreature()
    {
        var opponentBoard = opponentInfo.namedZones["Board"].cards;
        var eligibleCreatures = opponentBoard.FindAll(x => x.namedStats["HP"].effectiveValue > 0);
        if (eligibleCreatures.Count > 0)
        {
            var provokeCreatures = eligibleCreatures.FindAll(x => x.type == Enumerators.CardType.HEAVY);
            if (provokeCreatures != null && provokeCreatures.Count >= 1)
            {
                return provokeCreatures[Random.Range(0, provokeCreatures.Count)];
            }
            else
            {
                return eligibleCreatures[Random.Range(0, eligibleCreatures.Count)];
            }
        }
        return null;
    }

    protected RuntimeCard GetRandomOpponentCreature()
    {
        var board = opponentInfo.namedZones["Board"].cards;
        Debug.Log("!!! - " + opponentInfo.namedZones[Constants.ZONE_BOARD].cards.Count);

        var eligibleCreatures = board.FindAll(x => x.namedStats["HP"].effectiveValue > 0);
        if (eligibleCreatures.Count > 0)
        {
            return eligibleCreatures[Random.Range(0, eligibleCreatures.Count)];
        }
        return null;
    }

    protected bool OpponentHasProvokeCreatures()
    {
        var opponentBoard = opponentInfo.namedZones["Board"].cards;
        var eligibleCreatures = opponentBoard.FindAll(x => x.namedStats["HP"].effectiveValue > 0);
        if (eligibleCreatures.Count > 0)
        {
            var provokeCreatures = eligibleCreatures.FindAll(x => x.type == Enumerators.CardType.HEAVY);
            return (provokeCreatures != null && provokeCreatures.Count >= 1);
        }
        return false;
    }
}
