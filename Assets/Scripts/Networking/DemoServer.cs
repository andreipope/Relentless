// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;

using CCGKit;

/// <summary>
/// The demo server is a subclass of the core Server type which adds demo-specific functionality,
/// like automatically increasing the players' mana pool every turn and determining the game's win
/// condition.
/// </summary>
public class DemoServer : Server
{
    protected override void AddServerHandlers()
    {
        base.AddServerHandlers();
        handlers.Add(new PlayCardHandler(this));
        handlers.Add(new CombatHandler(this));
    }
}
