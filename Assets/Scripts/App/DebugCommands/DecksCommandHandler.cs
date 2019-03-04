using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Data;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class DecksCommandHandler
{
    private static IMatchManager _matchManager;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(DecksCommandHandler));

        _matchManager = GameClient.Get<IMatchManager>();
    }
}
