// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using CCGKit;

public class DemoPlayer : Player
{
    public override void OnStartTurn(StartTurnMessage msg)
    {
        base.OnStartTurn(msg);
        if (msg.isRecipientTheActivePlayer)
        {
            /*var handZone = Array.Find(msg.StaticGameZones, x => x.Name == "Hand");
            hand = new List<int>(handZone.Cards);*/
        }
    }
}