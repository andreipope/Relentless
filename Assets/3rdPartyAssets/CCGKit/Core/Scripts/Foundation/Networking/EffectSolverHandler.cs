// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace CCGKit
{
    public class EffectSolverHandler : ServerHandler
    {
        public EffectSolverHandler(Server server) : base(server)
        {
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler(NetworkProtocol.ActivateAbility, OnActivateAbility);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler(NetworkProtocol.ActivateAbility);
        }

        public virtual void OnActivateAbility(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ActivateAbilityMessage>();
            var sourcePlayer = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            if (sourcePlayer != null)
            {
                var card = sourcePlayer.zones[msg.zoneId].cards.Find(x => x.instanceId == msg.cardInstanceId);
                if (card != null)
                {
                    var libraryCard = GameManager.Instance.config.GetCard(card.cardId);
                    var cost = libraryCard.costs.Find(x => x is PayResourceCost);
                    if (cost != null)
                    {
                        var payResourceCost = cost as PayResourceCost;
                        var statCost = payResourceCost.value;
                        if (sourcePlayer.stats[payResourceCost.statId].effectiveValue >= statCost)
                        {
                            sourcePlayer.stats[payResourceCost.statId].baseValue -= statCost;
                            server.effectSolver.ActivateAbility(sourcePlayer, card, 0);
                        }
                    }
                }

                var broadcastMsg = new ActivateAbilityMessage();
                broadcastMsg.playerNetId = msg.playerNetId;
                broadcastMsg.zoneId = msg.zoneId;
                broadcastMsg.cardInstanceId = msg.cardInstanceId;
                broadcastMsg.abilityIndex = msg.abilityIndex;
                foreach (var player in server.gameState.players.FindAll(x => x != sourcePlayer))
                {
                    NetworkServer.SendToAll(NetworkProtocol.ActivateAbility, broadcastMsg);
                }
            }
        }
    }
}
