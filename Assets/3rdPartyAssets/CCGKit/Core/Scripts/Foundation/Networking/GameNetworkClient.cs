// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This class is responsible for handling the reception of custom network messages from the
    /// game server and routing them to the appropriate local player. Single-player mode is
    /// implemented via a second local player that uses the same system as normal human players
    /// in multiplayer modes; which is specially convenient implementation-wise (as there is no
    /// special case for it).
    /// </summary>
    public class GameNetworkClient : NetworkBehaviour
    {
        /// <summary>
        /// Cached reference to the local network client.
        /// </summary>
        protected NetworkClient client;

        /// <summary>
        /// List of all the local players connected to this client. Normally this will only contain
        /// the human local player for multiplayer games, but in the case of single-player games it
        /// will also contain the AI-controlled player.
        /// </summary>
        protected List<Player> localPlayers = new List<Player>();

        /// <summary>
        /// Unity's OnStartClient.
        /// </summary>
        public override void OnStartClient()
        {
            client = NetworkManager.singleton.client;
            RegisterNetworkHandlers();
        }

        /// <summary>
        /// Unity's OnDestroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnregisterNetworkHandlers();
        }

        /// <summary>
        /// Addds a new local player to this client.
        /// </summary>
        /// <param name="player">The local player to add to this client.</param>
        public void AddLocalPlayer(Player player)
        {
            localPlayers.Add(player);
        }

        /// <summary>
        /// Registers the network handlers for the network messages we are interested in handling.
        /// </summary>
        protected virtual void RegisterNetworkHandlers()
        {
            client.RegisterHandler(NetworkProtocol.StartGame, OnStartGame);
            client.RegisterHandler(NetworkProtocol.EndGame, OnEndGame);
            client.RegisterHandler(NetworkProtocol.StartTurn, OnStartTurn);
            client.RegisterHandler(NetworkProtocol.EndTurn, OnEndTurn);
            client.RegisterHandler(NetworkProtocol.BroadcastChatTextMessage, OnReceiveChatText);

            client.RegisterHandler(NetworkProtocol.CardMoved, OnCardMoved);
            client.RegisterHandler(NetworkProtocol.PlayerAttacked, OnPlayerAttacked);
            client.RegisterHandler(NetworkProtocol.CreatureAttacked, OnCreatureAttacked);

            client.RegisterHandler(NetworkProtocol.ActivateAbility, OnActivateAbility);
        }

        /// <summary>
        /// Unregisters the network handlers for the network messages we are interested in handling.
        /// </summary>
        protected virtual void UnregisterNetworkHandlers()
        {
            if (client != null)
            {
                client.UnregisterHandler(NetworkProtocol.ActivateAbility);

                client.UnregisterHandler(NetworkProtocol.CreatureAttacked);
                client.UnregisterHandler(NetworkProtocol.PlayerAttacked);
                client.UnregisterHandler(NetworkProtocol.CardMoved);

                client.UnregisterHandler(NetworkProtocol.BroadcastChatTextMessage);
                client.UnregisterHandler(NetworkProtocol.EndTurn);
                client.UnregisterHandler(NetworkProtocol.StartTurn);
                client.UnregisterHandler(NetworkProtocol.EndGame);
                client.UnregisterHandler(NetworkProtocol.StartGame);
            }
        }

        /// <summary>
        /// Called when the game starts.
        /// </summary>
        /// <param name="netMsg">Start game message.</param>
        protected void OnStartGame(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<StartGameMessage>();
            Assert.IsNotNull(msg);
            var player = localPlayers.Find(x => x.netId == msg.recipientNetId);
            if (player != null)
            {
                player.OnStartGame(msg);
            }
        }

        /// <summary>
        /// Called when the game ends.
        /// </summary>
        /// <param name="netMsg">End game message.</param>
        protected void OnEndGame(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<EndGameMessage>();
            Assert.IsTrue(msg != null);
            foreach (var player in localPlayers)
                player.OnEndGame(msg);
        }

        /// <summary>
        /// Called when a new turn starts.
        /// </summary>
        /// <param name="netMsg">Start turn message.</param>
        protected void OnStartTurn(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<StartTurnMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.recipientNetId);
            if (player != null)
                player.OnStartTurn(msg);
        }

        /// <summary>
        /// Called when a new turn ends.
        /// </summary>
        /// <param name="netMsg">End turn message.</param>
        protected void OnEndTurn(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<EndTurnMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.recipientNetId);
            if (player != null)
                player.OnEndTurn(msg);
        }

        /// <summary>
        /// Called when the player receives a chat text message.
        /// </summary>
        /// <param name="netMsg">Chat text message.</param>
        protected void OnReceiveChatText(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<BroadcastChatTextMessage>();
            Assert.IsTrue(msg != null);
            foreach (var player in localPlayers)
                player.OnReceiveChatText(msg.senderNetId, msg.text);
        }

        /// <summary>
        /// Called when a card was moved from one zone to another.
        /// </summary>
        /// <param name="netMsg">Network message.</param>
        protected void OnCardMoved(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<CardMovedMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId != msg.playerNetId);
            if (player != null)
            {
                player.OnCardMoved(msg);
            }
        }

        /// <summary>
        /// Called when a player was attacked.
        /// </summary>
        /// <param name="netMsg">Network message.</param>
        protected void OnPlayerAttacked(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<PlayerAttackedMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId != msg.attackingPlayerNetId);
            if (player != null)
            {
                player.OnPlayerAttacked(msg);
            }
        }

        /// <summary>
        /// Called when a creature was attacked.
        /// </summary>
        /// <param name="netMsg">Network message.</param>
        protected void OnCreatureAttacked(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<CreatureAttackedMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId != msg.attackingPlayerNetId);
            if (player != null)
            {
                player.OnCreatureAttacked(msg);
            }
        }

        /// <summary>
        /// Called when an activated ability was activated.
        /// </summary>
        /// <param name="netMsg">Network message.</param>
        protected void OnActivateAbility(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ActivateAbilityMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId != msg.playerNetId);
            if (player != null)
            {
                player.OnActivateAbility(msg);
            }
        }
    }
}
