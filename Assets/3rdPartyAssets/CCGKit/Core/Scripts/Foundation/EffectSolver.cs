// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using GrandDevs.CZB;
using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This class is responsible for resolving all the effects that take place in a game.
    /// There is an effect solver on the server side and another one on the client side. The
    /// goal of this duplicity is to allow for lag-free gameplay: the logic is always evaluated
    /// locally first so that clients do not need to wait for the server to present its results
    /// on the screen. The client's game state is still always synchronized with that of the
    /// server; it just happens to be executed locally first too.
    /// </summary>
    public class EffectSolver
    {
        /// <summary>
        /// The current state of the game.
        /// </summary>
        public GameState gameState;

        /// <summary>
        /// The random number generator of the game.
        /// </summary>
        public Random rng;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameState">The state of the game.</param>
        /// <param name="rngSeed">The random number generator's seed.</param>
        public EffectSolver(GameState gameState, int rngSeed)
        {
            this.gameState = gameState;
            this.gameState.config = GameManager.Instance.config;
            this.gameState.effectSolver = this;
            rng = new Random(rngSeed);
        }

        /// <summary>
        /// This method is automatically called when the turn starts.
        /// </summary>
        public void OnTurnStarted()
        {
            foreach (var zone in gameState.currentPlayer.zones)
            {
                var zoneDefinition = gameState.config.gameZones.Find(x => x.id == zone.Value.zoneId);
                if (zoneDefinition.type == ZoneType.Dynamic && zoneDefinition.opponentVisibility == ZoneOpponentVisibility.Visible)
                {
                    foreach (var card in zone.Value.cards)
                    {
                        TriggerEffect<OnPlayerTurnStartedTrigger>(gameState.currentPlayer, card, x => { return true; });
                    }
                }
            }
        }

        /// <summary>
        /// This method is automatically called when the turn ends.
        /// </summary>
        public void OnTurnEnded()
        {
            foreach (var zone in gameState.currentPlayer.zones)
            {
                var zoneDefinition = gameState.config.gameZones.Find(x => x.id == zone.Value.zoneId);
                if (zoneDefinition.type == ZoneType.Dynamic && zoneDefinition.opponentVisibility == ZoneOpponentVisibility.Visible)
                {
                    foreach (var card in zone.Value.cards)
                    {
                        TriggerEffect<OnPlayerTurnEndedTrigger>(gameState.currentPlayer, card, x => { return true; });
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the combat between the specified card and its opponent player.
        /// </summary>
        /// <param name="attackingPlayerNetId">The network identifier of the attacking player.</param>
        /// <param name="attackingCardInstanceId">The instance identifier of the attacking card.</param>
        public void FightPlayer(NetworkInstanceId attackingPlayerNetId, int attackingCardInstanceId)
        {
            var attackingPlayer = gameState.players.Find(x => x.netId == attackingPlayerNetId);
            var attackedPlayer = gameState.players.Find(x => x.netId != attackingPlayerNetId);
            if (attackingPlayer != null && attackedPlayer != null)
            {
                var board = attackingPlayer.namedZones["Board"];
                var card = board.cards.Find(x => x.instanceId == attackingCardInstanceId);
                if (card != null)
                {
                    attackedPlayer.namedStats["Life"].baseValue -= card.namedStats["DMG"].effectiveValue;
                }
            }
        }

        public void FightPlayerBySkill(NetworkInstanceId attackingPlayerNetId, int value, bool isOpponent)
        {
            var attackedPlayer = gameState.players.Find(x => (isOpponent && x.netId != attackingPlayerNetId) || (!isOpponent && x.netId == attackingPlayerNetId));
            attackedPlayer.namedStats[Constants.TAG_LIFE].baseValue -= value;
        }

        public void HealPlayerBySkill(NetworkInstanceId callerPlayerNetId, int value, bool isOpponent)
        {
            var choosedPlayer = gameState.players.Find(x => (isOpponent && x.netId != callerPlayerNetId) || (!isOpponent && x.netId == callerPlayerNetId));

            int maxHPPlayer = choosedPlayer.namedStats[Constants.TAG_LIFE].originalValue;
            foreach (var item in choosedPlayer.namedStats[Constants.TAG_LIFE].modifiers)
                maxHPPlayer += item.value;

            choosedPlayer.namedStats[Constants.TAG_LIFE].baseValue += value;

            if (choosedPlayer.namedStats[Constants.TAG_LIFE].baseValue > maxHPPlayer)
                choosedPlayer.namedStats[Constants.TAG_LIFE].baseValue = maxHPPlayer;
        }

        /// <summary>
        /// Resolves the combat between the specified creatures.
        /// </summary>
        /// <param name="attackingPlayerNetId">The network identifier of the attacking player.</param>
        /// <param name="attackingCreature">The attacking creature.</param>
        /// <param name="attackedCreature">The attacked creature.</param>
        public void FightCreature(NetworkInstanceId attackingPlayerNetId, RuntimeCard attackingCreature, RuntimeCard attackedCreature)
        {
            var attackingPlayer = gameState.players.Find(x => x.netId == attackingPlayerNetId);
            var attackedPlayer = gameState.players.Find(x => x.netId != attackingPlayerNetId);
            if (attackingPlayer != null && attackedPlayer != null)
            {
                var abilitiesController = GameClient.Get<IGameplayManager>().GetController<AbilitiesController>();

                int additionalDamageAttacker = abilitiesController.GetStatModificatorByAbility(attackingCreature, attackedCreature);
                int additionalDamageAttacked = abilitiesController.GetStatModificatorByAbility(attackedCreature, attackingCreature);

                attackedCreature.namedStats["HP"].baseValue -= attackingCreature.namedStats["DMG"].effectiveValue + additionalDamageAttacker;
                attackingCreature.namedStats["HP"].baseValue -= attackedCreature.namedStats["DMG"].effectiveValue + additionalDamageAttacked;
            }
        }

        public void FightCreatureBySkill(NetworkInstanceId attackingPlayerNetId, RuntimeCard attackedCreature, int attack)
        {
            var attackedPlayer = gameState.players.Find(x => x.netId != attackingPlayerNetId);
            if (attackedPlayer != null)
            {
                attackedCreature.namedStats[Constants.TAG_HP].baseValue -= attack;
            }
        }

        public void HealCreatureBySkill(NetworkInstanceId playerNetId, RuntimeCard creature, int value)
        {
            var player = gameState.players.Find(x => x.netId != playerNetId);
            if (player != null)
            {
                int maxHPCreature = creature.namedStats[Constants.TAG_HP].originalValue;
                foreach (var item in creature.namedStats[Constants.TAG_HP].modifiers)
                    maxHPCreature += item.value;

                creature.namedStats[Constants.TAG_HP].baseValue += value;

                if (creature.namedStats[Constants.TAG_HP].baseValue > maxHPCreature)
                    creature.namedStats[Constants.TAG_HP].baseValue = maxHPCreature;
            }
        }

        /// <summary>
        /// Moves the specified card from the specified origin zone to the specified destination zone.
        /// </summary>
        /// <param name="playerNetId">The network identifier of the card's owner player.</param>
        /// <param name="card">The card to move.</param>
        /// <param name="originZone">The origin zone.</param>
        /// <param name="destinationZone">The destination zone.</param>
        /// <param name="targetInfo">The optional target information.</param>
        public void MoveCard(NetworkInstanceId playerNetId, RuntimeCard card, string originZone, string destinationZone, List<int> targetInfo = null)
        {
            var player = gameState.players.Find(x => x.netId == playerNetId);
            if (player != null)
            {
                player.namedZones[originZone].RemoveCard(card);
                player.namedZones[destinationZone].AddCard(card);
                TriggerEffect<OnCardLeftZoneTrigger>(player, card, x => { return x.IsTrue(gameState, originZone); }, targetInfo);
                TriggerEffect<OnCardEnteredZoneTrigger>(player, card, x => { return x.IsTrue(gameState, destinationZone); }, targetInfo);

                var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);
                        
                if ((Enumerators.CardKind)libraryCard.cardTypeId == Enumerators.CardKind.SPELL)
                {
                    var finalDestinationZone = gameState.config.gameZones.Find(x => x.id == 3); // GRAVEYARD ID is 3
                    // We do not use the MoveCards function here, because we do not want to trigger any effects
                    // (which would cause an infinite recursion).
                    player.namedZones[destinationZone].RemoveCard(card);
                    player.namedZones[finalDestinationZone.name].AddCard(card);
                }
            }
        }

        /// <summary>
        /// Triggers the triggered effects of the specified card.
        /// </summary>
        /// <typeparam name="T">The type of the trigger.</typeparam>
        /// <param name="player">The owner player of the card that is triggering the effect.</param>
        /// <param name="card">The card that is triggering the effect.</param>
        /// <param name="predicate">The predicate that needs to be satisfied in order to trigger the effect.</param>
        /// <param name="targetInfo">The optional target information.</param>
        public void TriggerEffect<T>(PlayerInfo player, RuntimeCard card, Predicate<T> predicate, List<int> targetInfo = null) where T : Trigger
        {
            return;
            var libraryCard = gameState.config.GetCard(card.cardId);
            var triggeredAbilities = libraryCard.abilities.FindAll(x => x is TriggeredAbility);
            foreach (var ability in triggeredAbilities)
            {
                var triggeredAbility = ability as TriggeredAbility;
                var trigger = triggeredAbility.trigger as T;
                if (trigger != null && predicate(trigger) == true)
                {
                    if (triggeredAbility.effect is PlayerEffect && AreTargetsAvailable(triggeredAbility.effect, card, triggeredAbility.target))
                    {
                        var targets = GetPlayerTargets(player, triggeredAbility.target, targetInfo);
                        foreach (var t in targets)
                        {
                            (triggeredAbility.effect as PlayerEffect).Resolve(gameState, t);
                        }
                    }
                    else if (triggeredAbility.effect is CardEffect && AreTargetsAvailable(triggeredAbility.effect, card, triggeredAbility.target))
                    {
                        var cardEffect = triggeredAbility.effect as CardEffect;
                        var targets = GetCardTargets(player, card, triggeredAbility.target, cardEffect.gameZoneId, cardEffect.cardTypeId, targetInfo);
                        foreach (var t in targets)
                        {
                            (triggeredAbility.effect as CardEffect).Resolve(gameState, t);
                        }
                    }
                    else if (triggeredAbility.effect is MoveCardEffect && AreTargetsAvailable(triggeredAbility.effect, card, triggeredAbility.target))
                    {
                        var moveCardEffect = triggeredAbility.effect as MoveCardEffect;
                        var targets = GetCardTargets(player, card, triggeredAbility.target, moveCardEffect.originGameZoneId, moveCardEffect.cardTypeId, targetInfo);
                        foreach (var t in targets)
                        {
                            (triggeredAbility.effect as MoveCardEffect).Resolve(gameState, t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activates the specified ability of the specified card.
        /// </summary>
        /// <param name="player">The owner player of the card that is activating the effect.</param>
        /// <param name="card">The card that is activating the effect.</param>
        /// <param name="abilityIndex">The index of the ability to activate.</param>
        /// <param name="targetInfo">The optional target information.</param>
        public void CreateActiveAbility(PlayerInfo player, RuntimeCard card, int abilityIndex, List<int> targetInfo = null)
        {
            var libraryCard = gameState.config.GetCard(card.cardId);
            var activatedAbilities = libraryCard.abilities.FindAll(x => x is ActivatedAbility);
            var activatedAbility = activatedAbilities[abilityIndex] as ActivatedAbility;
            if (activatedAbility.effect is PlayerEffect && AreTargetsAvailable(activatedAbility.effect, card, activatedAbility.target))
            {
                var targets = GetPlayerTargets(player, activatedAbility.target, targetInfo);
                foreach (var t in targets)
                {
                    (activatedAbility.effect as PlayerEffect).Resolve(gameState, t);
                }
            }
            else if (activatedAbility.effect is CardEffect && AreTargetsAvailable(activatedAbility.effect, card, activatedAbility.target))
            {
                var cardEffect = activatedAbility.effect as CardEffect;
                var targets = GetCardTargets(player, card, activatedAbility.target, cardEffect.gameZoneId, cardEffect.cardTypeId, targetInfo);
                foreach (var t in targets)
                {
                    (activatedAbility.effect as CardEffect).Resolve(gameState, t);
                }
            }
            else if (activatedAbility.effect is MoveCardEffect && AreTargetsAvailable(activatedAbility.effect, card, activatedAbility.target))
            {
                var moveCardEffect = activatedAbility.effect as MoveCardEffect;
                var targets = GetCardTargets(player, card, activatedAbility.target, moveCardEffect.originGameZoneId, moveCardEffect.cardTypeId, targetInfo);
                foreach (var t in targets)
                {
                    (activatedAbility.effect as MoveCardEffect).Resolve(gameState, t);
                }
            }
        }

        /// <summary>
        /// Sets the destroy conditions of the specified card.
        /// </summary>
        /// <param name="card">The card to set.</param>
        public void SetDestroyConditions(RuntimeCard card)
        {
            var cardType = card.cardType;
            foreach (var condition in cardType.destroyConditions)
            {
                if (condition is StatDestroyCardCondition)
                {
                    var statCondition = condition as StatDestroyCardCondition;
                    card.stats[statCondition.statId].onValueChanged += (oldValue, newValue) =>
                    {
                        if (statCondition.IsTrue(card))
                        {
                            MoveCard(card.ownerPlayer.netId, card, "Board", "Graveyard");
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Sets the triggers of the specified player.
        /// </summary>
        /// <param name="player">The player to set.</param>
        public void SetTriggers(PlayerInfo player)
        {
            foreach (var stat in player.stats)
            {
                stat.Value.onValueChanged += (oldValue, newValue) =>
                {
                    foreach (var zone in player.zones)
                    {
                        var zoneDefinition = gameState.config.gameZones.Find(x => x.id == zone.Value.zoneId);
                        if (zoneDefinition.type == ZoneType.Dynamic && zoneDefinition.opponentVisibility == ZoneOpponentVisibility.Visible)
                        {
                            foreach (var card in zone.Value.cards)
                            {
                                TriggerEffect<OnPlayerStatIncreasedTrigger>(player, card, x => { return x.IsTrue(stat.Value, newValue, oldValue); });
                                TriggerEffect<OnPlayerStatDecreasedTrigger>(player, card, x => { return x.IsTrue(stat.Value, newValue, oldValue); });
                                TriggerEffect<OnPlayerStatReachedValueTrigger>(player, card, x => { return x.IsTrue(stat.Value, newValue, oldValue); });
                            }
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Sets the triggers of the specified card.
        /// </summary>
        /// <param name="card">The card to set.</param>
        public void SetTriggers(RuntimeCard card)
        {
            foreach (var stat in card.stats)
            {
                stat.Value.onValueChanged += (oldValue, newValue) =>
                {
                    TriggerEffect<OnCardStatIncreasedTrigger>(card.ownerPlayer, card, x => { return x.IsTrue(stat.Value, newValue, oldValue); });
                    TriggerEffect<OnCardStatDecreasedTrigger>(card.ownerPlayer, card, x => { return x.IsTrue(stat.Value, newValue, oldValue); });
                    TriggerEffect<OnCardStatReachedValueTrigger>(card.ownerPlayer, card, x => { return x.IsTrue(stat.Value, newValue, oldValue); });
                };
            }
        }

        /// <summary>
        /// Returns the actual player targets of the specified target type.
        /// </summary>
        /// <param name="player">The current player.</param>
        /// <param name="abilityTarget">The target.</param>
        /// <param name="targetInfo">The target information.</param>
        /// <returns>The actual player targets of the specified target type.</returns>
        public List<PlayerInfo> GetPlayerTargets(PlayerInfo player, Target abilityTarget, List<int> targetInfo)
        {
            var playerTargets = new List<PlayerInfo>();
            var target = abilityTarget.GetTarget();
            switch (target)
            {
                case EffectTarget.Player:
                    playerTargets.Add(player);
                    break;

                case EffectTarget.Opponent:
                    playerTargets.Add(gameState.players.Find(x => x != player));
                    break;

                case EffectTarget.TargetPlayer:
                    if (targetInfo != null && targetInfo[0] == 0)
                    {
                        playerTargets.Add(player);
                    }
                    else
                    {
                        playerTargets.Add(gameState.players.Find(x => x != player));
                    }
                    break;

                case EffectTarget.RandomPlayer:
                    {
                        playerTargets.AddRange(gameState.players);
                        playerTargets = playerTargets.OrderBy(x => x.netId.Value).ToList();
                        var randomPlayer = playerTargets[GetRandomNumber(playerTargets.Count)];
                        playerTargets.RemoveAll(x => x != randomPlayer);
                    }
                    break;

                case EffectTarget.AllPlayers:
                    playerTargets.AddRange(gameState.players);
                    break;

                default:
                    break;
            }
            playerTargets.RemoveAll(x =>
            {
                var conditionsFullfilled = true;
                var playerTarget = abilityTarget as PlayerTargetBase;
                foreach (var condition in playerTarget.conditions)
                {
                    if (!condition.IsTrue(x))
                    {
                        conditionsFullfilled = false;
                        break;
                    }
                }
                return !conditionsFullfilled;
            });
            return playerTargets;
        }

        /// <summary>
        /// Returns the actual card targets of the specified target.
        /// </summary>
        /// <param name="player">The current player.</param>
        /// <param name="sourceCard">The current card.</param>
        /// <param name="abilityTarget">The target.</param>
        /// <param name="gameZoneId">The game zone identifier.</param>
        /// <param name="cardTypeId">The card type.</param>
        /// <param name="targetInfo">The target information.</param>
        /// <returns>The actual card targets of the specified target.</returns>
        public List<RuntimeCard> GetCardTargets(PlayerInfo player, RuntimeCard sourceCard, Target abilityTarget, int gameZoneId, int cardTypeId, List<int> targetInfo)
        {
            var cardTargets = new List<RuntimeCard>();
            var opponent = gameState.players.Find(x => x != player);
            var target = abilityTarget.GetTarget();
            var effectZone = gameZoneId;
            var effectCardType = cardTypeId;
            var zoneId = (targetInfo != null && targetInfo.Count > 0) ? targetInfo[0] : effectZone;
            switch (target)
            {
                case EffectTarget.ThisCard:
                    cardTargets.Add(sourceCard);
                    break;

                case EffectTarget.PlayerCard:
                    {
                        var card = player.GetCard(targetInfo[1], zoneId);
                        cardTargets.Add(card);
                    }
                    break;

                case EffectTarget.OpponentCard:
                    {
                        var card = opponent.GetCard(targetInfo[1], zoneId);
                        cardTargets.Add(card);
                    }
                    break;

                case EffectTarget.TargetCard:
                    {
                        var card = player.GetCard(targetInfo[1], zoneId);
                        if (card == null)
                        {
                            card = opponent.GetCard(targetInfo[1], zoneId);
                        }
                        cardTargets.Add(card);
                    }
                    break;

                case EffectTarget.RandomPlayerCard:
                    {
                        cardTargets.AddRange(player.zones[zoneId].cards);
                        cardTargets.RemoveAll(x => x.cardType.id != effectCardType);
                        var card = cardTargets[GetRandomNumber(cardTargets.Count)];
                        cardTargets.RemoveAll(x => x != card);
                    }
                    break;

                case EffectTarget.RandomOpponentCard:
                    {
                        cardTargets.AddRange(opponent.zones[zoneId].cards);
                        cardTargets.RemoveAll(x => x.cardType.id != effectCardType);
                        var card = cardTargets[GetRandomNumber(cardTargets.Count)];
                        cardTargets.RemoveAll(x => x != card);
                    }
                    break;

                case EffectTarget.RandomCard:
                    {
                        cardTargets.AddRange(player.zones[zoneId].cards);
                        cardTargets.AddRange(opponent.zones[zoneId].cards);
                        cardTargets.RemoveAll(x => x.cardType.id != effectCardType);
                        var card = cardTargets[GetRandomNumber(cardTargets.Count)];
                        cardTargets.RemoveAll(x => x != card);
                    }
                    break;

                case EffectTarget.AllPlayerCards:
                    cardTargets.AddRange(player.zones[zoneId].cards);
                    cardTargets.RemoveAll(x => x.cardType.id != effectCardType);
                    break;

                case EffectTarget.AllOpponentCards:
                    cardTargets.AddRange(opponent.zones[zoneId].cards);
                    cardTargets.RemoveAll(x => x.cardType.id != effectCardType);
                    break;

                case EffectTarget.AllCards:
                    cardTargets.AddRange(player.zones[zoneId].cards);
                    cardTargets.AddRange(opponent.zones[zoneId].cards);
                    cardTargets.RemoveAll(x => x.cardType.id != effectCardType);
                    break;

                default:
                    break;
            }
            cardTargets.RemoveAll(x =>
            {
                var conditionsFullfilled = true;
                var cardTarget = abilityTarget as CardTargetBase;
                foreach (var condition in cardTarget.conditions)
                {
                    if (!condition.IsTrue(x))
                    {
                        conditionsFullfilled = false;
                        break;
                    }
                }
                return !conditionsFullfilled;
            });
            return cardTargets;
        }

        /// <summary>
        /// Returns true if there are any targets available for the specified effect and false otherwise.
        /// </summary>
        /// <param name="effect">The effect.</param>
        /// <param name="sourceCard">The card originating the effect.</param>
        /// <param name="target">The target.</param>
        /// <returns>True if there are any targets available for the specified effect; false otherwise.</returns>
        public bool AreTargetsAvailable(Effect effect, RuntimeCard sourceCard, Target target)
        {
            return effect.AreTargetsAvailable(gameState, sourceCard, target);
        }

        /// <summary>
        /// Returns a random number in the [0, max] range.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <returns>A random number in the [0, max] range.</returns>
        public int GetRandomNumber(int max)
        {
            return rng.Next(max);
        }

        /// <summary>
        /// Returns a random number in the [min, max] range.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A random number in the [min, max] range.</returns>
        public int GetRandomNumber(int min, int max)
        {
            return rng.Next(min, max);
        }
    }
}
