// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

using CCGKit;

/// <summary>
/// This class holds information about a player avatar from the game scene, which can be clicked
/// to select a target player for an effect or during combat (this will send the appropriate
/// information to the server).
/// </summary>
public class PlayerAvatar : MonoBehaviour
{
    public PlayerInfo playerInfo;
    public bool IsBottom;
    public int index { get { return IsBottom ? 0 : 1; } }

    private Player GetTargetPlayer()
    {
        var players = FindObjectsOfType<Player>();
        if (IsBottom)
        {
            foreach (var player in players)
            {
                if (player.isLocalPlayer && player.isHuman)
                    return player;
            }
        }
        else
        {
            foreach (var player in players)
            {
                if (!player.isLocalPlayer || (player.isLocalPlayer && !player.isHuman))
                    return player;
            }
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.transform.parent != null)
        {
            var targetingArrow = collider.transform.parent.GetComponent<TargetingArrow>();
            if (targetingArrow != null)
            {
                targetingArrow.OnPlayerSelected(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.transform.parent != null)
        {
            var targetingArrow = collider.transform.parent.GetComponent<TargetingArrow>();
            if (targetingArrow != null)
            {
                targetingArrow.OnPlayerUnselected(this);
            }
        }
    }
}
