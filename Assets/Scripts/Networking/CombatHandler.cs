// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

using CCGKit;
using GrandDevs.CZB.Common;

/// <summary>
///  This server handler is responsible for managing the network aspects of a combat between
///  two cards.
///
/// Combat follow this sequence:
///     - A player selects a card that is eligible as an attacker during his turn and sends
///       this information to the server.
///     - A player then selects a target player or card as the attack's target and sends this
///       information to the server.
///     - The server then proceeds with resolving the attack authoritatively and updating all
///       the clients afterwards.
///
/// This process is currently very much suited to the traditional way of resolving combats in
/// CCGs (the attacker's attack value is substracted from the attacked's defense value, and
/// vice versa). This is something we would like to expand upon in future updates to allow for
/// more varied/complex mechanics.
/// </summary>
public class CombatHandler : ServerHandler
{
    public CombatHandler(Server server) : base(server)
    {
    }

    public override void RegisterNetworkHandlers()
    {
        base.RegisterNetworkHandlers();
        NetworkServer.RegisterHandler(NetworkProtocol.FightPlayer, OnFightPlayer);
        NetworkServer.RegisterHandler(NetworkProtocol.FightCreature, OnFightCreature);
        NetworkServer.RegisterHandler(NetworkProtocol.FightPlayerBySkill, OnFightPlayerBySkill);
        NetworkServer.RegisterHandler(NetworkProtocol.FightCreatureBySkill, OnFightCreatureBySkill);
        NetworkServer.RegisterHandler(NetworkProtocol.HealPlayerBySkill, OnHealPlayerBySkill);
        NetworkServer.RegisterHandler(NetworkProtocol.HealCreatureBySkill, OnHealCreatureBySkill);
    }

    public override void UnregisterNetworkHandlers()
    {
        base.UnregisterNetworkHandlers();
        NetworkServer.UnregisterHandler(NetworkProtocol.FightCreature);
        NetworkServer.UnregisterHandler(NetworkProtocol.FightPlayer);
        NetworkServer.UnregisterHandler(NetworkProtocol.FightPlayerBySkill);
        NetworkServer.UnregisterHandler(NetworkProtocol.FightCreatureBySkill);
        NetworkServer.UnregisterHandler(NetworkProtocol.HealPlayerBySkill);
        NetworkServer.UnregisterHandler(NetworkProtocol.HealCreatureBySkill);
    }

    public virtual void OnFightPlayer(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<FightPlayerMessage>();
        if (msg != null)
        {
            // Only the current player can fight.
            if (netMsg.conn.connectionId != server.gameState.currentPlayer.connectionId)
            {
                return;
            }

            var playerAttackedMsg = new PlayerAttackedMessage();
            playerAttackedMsg.attackingPlayerNetId = msg.attackingPlayerNetId;
            playerAttackedMsg.attackingCardInstanceId = msg.cardInstanceId;
            server.SafeSendToClient(server.gameState.currentOpponent, NetworkProtocol.PlayerAttacked, playerAttackedMsg);

            server.effectSolver.FightPlayer(msg.attackingPlayerNetId, msg.cardInstanceId);
        }
    }

    public virtual void OnFightPlayerBySkill(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<FightPlayerBySkillMessage>();
        if (msg != null)
        {
            // Only the current player can fight.
            if (netMsg.conn.connectionId != server.gameState.currentPlayer.connectionId)
            {
                return;
            }

            /*var playerAttackedMsg = new PlayerAttackedMessage();
            playerAttackedMsg.attackingPlayerNetId = msg.attackingPlayerNetId;
            playerAttackedMsg.attackingCardInstanceId = msg.cardInstanceId;
            server.SafeSendToClient(server.gameState.currentOpponent, NetworkProtocol.PlayerAttacked, playerAttackedMsg);
              */
            server.effectSolver.FightPlayerBySkill(msg.attackingPlayerNetId, msg.attack, msg.isOpponent);
        }
    }

    public virtual void OnFightCreature(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<FightCreatureMessage>();
        if (msg != null)
        {
            // Only the current player can fight.
            if (netMsg.conn.connectionId != server.gameState.currentPlayer.connectionId)
            {
                return;
            }

            var creatureAttackedMsg = new CreatureAttackedMessage();
            creatureAttackedMsg.attackingPlayerNetId = msg.attackingPlayerNetId;
            creatureAttackedMsg.attackingCardInstanceId = msg.attackingCardInstanceId;
            creatureAttackedMsg.attackedCardInstanceId = msg.attackedCardInstanceId;
            server.SafeSendToClient(server.gameState.currentOpponent, NetworkProtocol.CreatureAttacked, creatureAttackedMsg);

            var attackingCard = server.gameState.currentPlayer.namedZones["Board"].cards.Find(x => x.instanceId == msg.attackingCardInstanceId);
            var attackedCard = server.gameState.currentOpponent.namedZones["Board"].cards.Find(x => x.instanceId == msg.attackedCardInstanceId);
            if (attackingCard != null && attackedCard != null)
            {
                server.effectSolver.FightCreature(msg.attackingPlayerNetId, attackingCard, attackedCard);
            }
        }
    }

    public virtual void OnFightCreatureBySkill(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<FightCreatureBySkillMessage>();
        if (msg != null)
        {

            // Only the current player can fight.
            if (netMsg.conn.connectionId != server.gameState.currentPlayer.connectionId)
            {
                return;
            }
            /*var creatureAttackedMsg = new CreatureAttackedMessage();
            creatureAttackedMsg.attackingPlayerNetId = msg.attackingPlayerNetId;
            creatureAttackedMsg.attackingCardInstanceId = msg.attackingCardInstanceId;
            creatureAttackedMsg.attackedCardInstanceId = msg.attackedCardInstanceId;
            server.SafeSendToClient(server.gameState.currentOpponent, NetworkProtocol.CreatureAttacked, creatureAttackedMsg);
                            */
            var attackedCard = server.gameState.currentOpponent.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == msg.attackedCardInstanceId);
            if (attackedCard != null)
            {
                server.effectSolver.FightCreatureBySkill(msg.attackingPlayerNetId, attackedCard, msg.attack);
            }
        }
    }

    public virtual void OnHealPlayerBySkill(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<HealPlayerBySkillMessage>();
        if (msg != null)
        {
            if (netMsg.conn.connectionId != server.gameState.currentPlayer.connectionId)
                return;

            server.effectSolver.HealPlayerBySkill(msg.callerPlayerNetId, msg.value, msg.isOpponent);
        }
    }

    public virtual void OnHealCreatureBySkill(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<HealCreatureBySkillMessage>();
        if (msg != null)
        {
            if (netMsg.conn.connectionId != server.gameState.currentPlayer.connectionId)
                return;

            var selectedCard = server.gameState.currentOpponent.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == msg.cardInstanceId);

            if(selectedCard == null)
                selectedCard = server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == msg.cardInstanceId);

            if (selectedCard != null)
            {
                server.effectSolver.HealCreatureBySkill(msg.callerPlayerNetId, selectedCard, msg.value);
            }
        }
    }
}