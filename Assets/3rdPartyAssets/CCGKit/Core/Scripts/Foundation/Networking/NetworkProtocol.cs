// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This utility class defines all the unique identifiers for every network message in a game.
    /// </summary>
    public class NetworkProtocol
    {
        public static short RegisterPlayer = 1000;
        public static short StartGame = 1001;
        public static short EndGame = 1002;
        public static short StartTurn = 1003;
        public static short EndTurn = 1004;
        public static short StopTurn = 1005;

        public static short PlayerState = 1006;
        public static short OpponentState = 1007;

        public static short MoveCard = 1008;
        public static short CardMoved = 1009;

        public static short FightPlayer = 1010;
        public static short FightPlayerBySkill = 10100;
        public static short HealPlayerBySkill = 10104;
        public static short FightCreature = 1011;
        public static short FightCreatureBySkill = 10110;
        public static short HealCreatureBySkill = 10114;
        public static short TryToAttackViaWeapon = 10124;
        public static short PlayEffect = 10134;   
        public static short PlayerAttacked = 1012;
        public static short CreatureAttacked = 1013;

        public static short CreateActiveAbility = 1014;

        public static short SendChatTextMessage = 1015;
        public static short BroadcastChatTextMessage = 1016;
    }

    // Every network message has a corresponding message class that carries the information needed
    // per message.

    public class RegisterPlayerMessage : MessageBase
    {
        public NetworkInstanceId netId;
        public string name;
        public bool isHuman;
        public int[] deck;
    }

    public class StartGameMessage : MessageBase
    {
        public NetworkInstanceId recipientNetId;
        public int playerIndex;
        public int turnDuration;
        public string[] nicknames;
        public NetPlayerInfo player;
        public NetPlayerInfo opponent;
        public int rngSeed;
    }

    public class StartTurnMessage : MessageBase
    {
        public NetworkInstanceId recipientNetId;
        public bool isRecipientTheActivePlayer;
        public int turn;
        public NetPlayerInfo player;
        public NetPlayerInfo opponent;
    }

    public class PlayerGameStateMessage : MessageBase
    {
        public NetPlayerInfo player;
    }

    public class OpponentGameStateMessage : MessageBase
    {
        public NetPlayerInfo[] opponents;
    }

    public class MoveCardMessage : MessageBase
    {
        public NetworkInstanceId playerNetId;
        public int cardInstanceId;
        public int originZoneId;
        public int destinationZoneId;
        public int[] targetInfo;
    }

    public class CardMovedMessage : MessageBase
    {
        public NetworkInstanceId playerNetId;
        public NetCard card;
        public int originZoneId;
        public int destinationZoneId;
        public int[] targetInfo;
    }

    public class SummonCardMessage : MessageBase
    {
        public int cardInstanceId;
    }

    public class PlayedCardMessage : MessageBase
    {
        public NetCard card;
    }

    public class FightPlayerMessage : MessageBase
    {
        public NetworkInstanceId attackingPlayerNetId;
        public int cardInstanceId;
    }

    public class FightPlayerBySkillMessage : MessageBase
    {
        public NetworkInstanceId attackingPlayerNetId;
        public int attack;
        public bool isOpponent;
    }

    public class HealPlayerBySkillMessage : MessageBase
    {
        public NetworkInstanceId callerPlayerNetId;
        public int value;
        public bool isOpponent;
        public bool isLimited;
    }

    public class HealCreatureBySkillMessage : MessageBase
    {
        public NetworkInstanceId callerPlayerNetId;
        public int cardInstanceId;
        public int value;
    }

    
    public class FightCreatureMessage : MessageBase
    {
        public NetworkInstanceId attackingPlayerNetId;
        public int attackingCardInstanceId;
        public int attackedCardInstanceId;
    }
    public class FightCreatureBySkillMessage : MessageBase
    {
        public NetworkInstanceId attackingPlayerNetId;
        public int attack;
        public int attackedCardInstanceId;
    }


    public class PlayerAttackedMessage : MessageBase
    {
        public NetworkInstanceId attackingPlayerNetId;
        public int attackingCardInstanceId;
    }

    public class CreatureAttackedMessage : MessageBase
    {
        public NetworkInstanceId attackingPlayerNetId;
        public int attackingCardInstanceId;
        public int attackedCardInstanceId;
    }

    public class EndGameMessage : MessageBase
    {
        public NetworkInstanceId winnerPlayerIndex;
    }

    public class EndTurnMessage : MessageBase
    {
        public NetworkInstanceId recipientNetId;
        public bool isRecipientTheActivePlayer;
    }

    public class StopTurnMessage : MessageBase
    {
    }

    public class SendChatTextMessage : MessageBase
    {
        public NetworkInstanceId senderNetId;
        public string text;
    }

    public class BroadcastChatTextMessage : MessageBase
    {
        public NetworkInstanceId senderNetId;
        public string text;
    }

    public class MoveCardsMessage : MessageBase
    {
        public NetworkInstanceId recipientNetId;
        public string originZone;
        public string destinationZone;
        public int numCards;
    }

    public class CreateActiveAbilityMessage : MessageBase
    {
        public NetworkInstanceId playerNetId;
        public int zoneId;
        public int cardInstanceId;
        public int abilityIndex;
    }

    public class TryToAttackViaWeaponMessage : MessageBase
    {
        public NetworkInstanceId callerPlayerNetId;
        public int value;
    }

    public class PlayEffectMessage : MessageBase
    {
        public NetworkInstanceId callerPlayerNetId;
        public int effectType;
        public int from;
        public int to;
        public int toType;
    }
}