// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This class provides general networking utilities.
    /// </summary>
    public static class NetworkingUtils
    {
        /// <summary>
        /// Returns the local player for this client.
        /// </summary>
        /// <returns>The local player for this client.</returns>
        public static Player GetLocalPlayer()
        {
            Player localPlayer = null;
            foreach (var pc in NetworkManager.singleton.client.connection.playerControllers)
            {
                var player = pc.gameObject.GetComponent<Player>();
                if (player.isLocalPlayer)
                {
                    localPlayer = player;
                    break;
                }
            }
            return localPlayer;
        }

        /// <summary>
        /// Returns the human local player for this client.
        /// </summary>
        /// <returns>The human local player for this client.</returns>
        public static Player GetHumanLocalPlayer()
        {
            Player localPlayer = null;
            foreach (var pc in NetworkManager.singleton.client.connection.playerControllers)
            {
                var player = pc.gameObject.GetComponent<Player>();
                if (player.isLocalPlayer && player.isHuman)
                {
                    localPlayer = player;
                    break;
                }
            }
            return localPlayer;
        }

        /// <summary>
        /// Returns the active local player for this client.
        /// </summary>
        /// <returns>The active local player for this client.</returns>
        public static Player GetActiveLocalPlayer()
        {
            Player localPlayer = null;
            foreach (var pc in NetworkManager.singleton.client.connection.playerControllers)
            {
                var player = pc.gameObject.GetComponent<Player>();
                if (player.isLocalPlayer && player.isActivePlayer)
                {
                    localPlayer = player;
                    break;
                }
            }
            return localPlayer;
        }

        /// <summary>
        /// Returns the network object with the specified network identifier.
        /// </summary>
        /// <param name="netId">Network identifier of the network object we want to retrieve.</param>
        /// <returns>The network object with the specified network identifier.</returns>
        public static GameObject GetNetworkObject(NetworkInstanceId netId)
        {
            foreach (var pair in NetworkServer.objects)
            {
                var obj = pair.Value.gameObject.GetComponent<NetworkBehaviour>();
                if (obj != null && obj.netId == netId)
                    return obj.gameObject;
            }
            return null;
        }

        public static NetStat GetNetStat(Stat stat)
        {
            var netStat = new NetStat();
            netStat.statId = stat.statId;
            netStat.originalValue = stat.originalValue;
            netStat.baseValue = stat.baseValue;
            netStat.minValue = stat.minValue;
            netStat.maxValue = stat.maxValue;
            var modifiers = new NetModifier[stat.modifiers.Count];
            for (var i = 0; i < stat.modifiers.Count; i++)
            {
                var netModifier = new NetModifier();
                netModifier.value = stat.modifiers[i].value;
                netModifier.duration = stat.modifiers[i].duration;
                modifiers[i] = netModifier;
            }
            netStat.modifiers = modifiers;
            return netStat;
        }

        public static Stat GetRuntimeStat(NetStat netStat)
        {
            var stat = new Stat();
            stat.statId = netStat.statId;
            stat.originalValue = netStat.originalValue;
            stat.baseValue = netStat.baseValue;
            stat.minValue = netStat.minValue;
            stat.maxValue = netStat.maxValue;
            stat.modifiers = new List<Modifier>();
            foreach (var netModifier in netStat.modifiers)
            {
                var modifier = new Modifier(netModifier.value, netModifier.duration);
                stat.modifiers.Add(modifier);
            }
            return stat;
        }

        public static NetKeyword GetNetKeyword(RuntimeKeyword keyword)
        {
            var netKeyword = new NetKeyword();
            netKeyword.keywordId = keyword.keywordId;
            netKeyword.valueId = keyword.valueId;
            return netKeyword;
        }

        public static RuntimeKeyword GetRuntimeKeyword(NetKeyword netKeyword)
        {
            var keyword = new RuntimeKeyword();
            keyword.keywordId = netKeyword.keywordId;
            keyword.valueId = netKeyword.valueId;
            return keyword;
        }

        public static NetCard GetNetCard(RuntimeCard card)
        {
            var netCard = new NetCard();
            netCard.cardId = card.cardId;
            netCard.instanceId = card.instanceId;
            var netStats = new List<NetStat>(card.stats.Count);
            foreach (var stat in card.stats)
            {
                netStats.Add(GetNetStat(stat.Value));
            }
            netCard.stats = netStats.ToArray();
            var netKeywords = new List<NetKeyword>(card.keywords.Count);
            foreach (var keyword in card.keywords)
            {
                netKeywords.Add(GetNetKeyword(keyword));
            }
            netCard.keywords = netKeywords.ToArray();
            return netCard;
        }

        public static RuntimeCard GetRuntimeCard(NetCard netCard)
        {
            var runtimeCard = new RuntimeCard();
            runtimeCard.cardId = netCard.cardId;
            runtimeCard.instanceId = netCard.instanceId;
            foreach (var stat in netCard.stats)
            {
                var runtimeStat = GetRuntimeStat(stat);
                runtimeCard.stats[stat.statId] = runtimeStat;
                var libraryCard = GameManager.Instance.config.GetCard(netCard.cardId);
                var statName = libraryCard.stats.Find(x => x.statId == stat.statId).name;
                runtimeCard.namedStats[statName] = runtimeStat;
            }
            var keywords = new List<RuntimeKeyword>();
            foreach (var keyword in netCard.keywords)
            {
                keywords.Add(GetRuntimeKeyword(keyword));
            }
            runtimeCard.keywords = keywords;
            return runtimeCard;
        }
    }
}
