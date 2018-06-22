// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// The base class for effects.
    /// </summary>
    public abstract class Effect
    {
        /// <summary>
        /// Returns true if there are any targets available for this effect and false otherwise.
        /// </summary>
        /// <param name="state">The game's state.</param>
        /// <param name="sourceCard">The card containing the effect.</param>
        /// <param name="target">The target type of the effect.</param>
        /// <returns>True if there are any targets available for this effect; false otherwise.</returns>
        public virtual bool AreTargetsAvailable(GameState state, RuntimeCard sourceCard, Target target)
        {
            return true;
        }
    }

    /// <summary>
    /// The base class for effects that target players.
    /// </summary>
    public abstract class PlayerEffect : Effect
    {
        /// <summary>
        /// Resolves this effect on the specified player.
        /// </summary>
        /// <param name="state">The game's state.</param>
        /// <param name="player">The player on which to resolve this effect.</param>
        public virtual void Resolve(GameState state, PlayerInfo player)
        {
        }

        /// <summary>
        /// Returns true if there are any targets available for this effect and false otherwise.
        /// </summary>
        /// <param name="state">The game's state.</param>
        /// <param name="sourceCard">The card containing the effect.</param>
        /// <param name="target">The target type of the effect.</param>
        /// <returns>True if there are any targets available for this effect; false otherwise.</returns>
        public override bool AreTargetsAvailable(GameState state, RuntimeCard sourceCard, Target target)
        {
            var players = new List<PlayerInfo>();
            switch (target.GetTarget())
            {
                case EffectTarget.Player:
                    players.Add(sourceCard.ownerPlayer);
                    break;

                case EffectTarget.Opponent:
                    var opponent = state.players.Find(x => x != sourceCard.ownerPlayer);
                    players.Add(opponent);
                    break;

                case EffectTarget.TargetPlayer:
                case EffectTarget.RandomPlayer:
                case EffectTarget.AllPlayers:
                    players.AddRange(state.players);
                    break;

                default:
                    break;
            }
            players.RemoveAll(x =>
            {
                var conditionsFullfilled = true;
                var playerTarget = target as PlayerTargetBase;
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
            return players.Count > 0;
        }
    }

    /// <summary>
    /// The base class for effects that target cards.
    /// </summary>
    public abstract class CardBaseEffect : Effect
    {
        /// <summary>
        /// Resolves this effect on the specified card.
        /// </summary>
        /// <param name="state">The game's state.</param>
        /// <param name="player">The card on which to resolve this effect.</param>
        public virtual void Resolve(GameState state, RuntimeCard card)
        {
        }
    }

    /// <summary>
    /// The base class for card effects.
    /// </summary>
    public abstract class CardEffect : CardBaseEffect
    {
        /// <summary>
        /// The unique identifier of the game zone.
        /// </summary>
        [GameZoneField("Zone")]
        [Order(1)]
        public int gameZoneId;

        /// <summary>
        /// The unique identifier of the card type.
        /// </summary>
        [CardTypeField("Card type")]
        [Order(2)]
        public int cardTypeId;

        /// <summary>
        /// Returns true if there are any targets available for this effect and false otherwise.
        /// </summary>
        /// <param name="state">The game's state.</param>
        /// <param name="sourceCard">The card containing the effect.</param>
        /// <param name="target">The target type of the effect.</param>
        /// <returns>True if there are any targets available for this effect; false otherwise.</returns>
        public override bool AreTargetsAvailable(GameState state, RuntimeCard sourceCard, Target target)
        {
            var cards = new List<RuntimeCard>();
            switch (target.GetTarget())
            {
                case EffectTarget.ThisCard:
                    cards.Add(sourceCard);
                    break;

                case EffectTarget.PlayerCard:
                case EffectTarget.AllPlayerCards:
                case EffectTarget.RandomPlayerCard:
                    {
                        foreach (var card in state.currentPlayer.zones[gameZoneId].cards)
                        {
                            cards.Add(card);
                        }
                        break;
                    }
                case EffectTarget.OpponentCard:
                case EffectTarget.AllOpponentCards:
                case EffectTarget.RandomOpponentCard:
                    {
                        foreach (var card in state.currentOpponent.zones[gameZoneId].cards)
                        {
                            cards.Add(card);
                        }
                        break;
                    }
                case EffectTarget.TargetCard:
                case EffectTarget.AllCards:
                case EffectTarget.RandomCard:
                    {
                        foreach (var card in state.currentPlayer.zones[gameZoneId].cards)
                        {
                            cards.Add(card);
                        }
                        foreach (var card in state.currentOpponent.zones[gameZoneId].cards)
                        {
                            cards.Add(card);
                        }
                        break;
                    }

                case EffectTarget.PlayerOrPlayerCreature:
                case EffectTarget.OpponentOrOpponentCreature:
                case EffectTarget.AnyPlayerOrCreature:
                    return true;

                default:
                    return false;
            }

            cards.RemoveAll(x => x.cardType.id != cardTypeId);
            cards.RemoveAll(x =>
            {
                var conditionsFullfilled = true;
                var cardTarget = target as CardTargetBase;
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
            return cards.Count > 0;
        }
    }

    /// <summary>
    /// The base class for card stat effects.
    /// </summary>
    public abstract class CardStatEffect : CardEffect
    {
        /// <summary>
        /// The unique identifier of the stat.
        /// </summary>
        [CardStatField("Card stat")]
        [Order(3)]
        public int statId;
    }
}
