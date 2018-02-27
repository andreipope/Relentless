// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using CCGKit;

public class SpellTargetingArrow : TargetingArrow
{
    [HideInInspector]
    public List<int> targetInfo = new List<int>();

    protected bool validTargetSelected;

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButton(0))
        {
            if (onTargetSelected != null && validTargetSelected)
            {
                onTargetSelected();
                Destroy(gameObject);
            }
        }
    }

    public override void OnCardSelected(BoardCreature creature)
    {
        if (targetType == EffectTarget.AnyPlayerOrCreature ||
            targetType == EffectTarget.TargetCard ||
            (targetType == EffectTarget.PlayerOrPlayerCreature && creature.tag == "PlayerOwned") ||
            (targetType == EffectTarget.OpponentOrOpponentCreature && creature.tag == "OpponentOwned") ||
            (targetType == EffectTarget.PlayerCard && creature.tag == "PlayerOwned") ||
            (targetType == EffectTarget.OpponentCard && creature.tag == "OpponentOwned"))
        {
            var conditionsFullfilled = true;
            var cardTarget = effectTarget as CardTargetBase;
            foreach (var condition in cardTarget.conditions)
            {
                if (!condition.IsTrue(creature.card))
                {
                    conditionsFullfilled = false;
                    break;
                }
            }
            if (conditionsFullfilled)
            {
                validTargetSelected = true;
                selectedCard = creature;
                selectedPlayer = null;
                targetInfo.Clear();
                targetInfo.Add(2);
                targetInfo.Add(creature.card.instanceId);
                CreateTarget(creature.transform.position);
            }
            else
            {
                validTargetSelected = false;
            }
        }
        else
        {
            validTargetSelected = false;
        }
    }

    public override void OnCardUnselected(BoardCreature creature)
    {
        if (selectedCard != null && selectedCard == creature)
        {
            Destroy(target);
            selectedCard = null;
            validTargetSelected = false;
            targetInfo.Clear();
        }
    }

    public override void OnPlayerSelected(PlayerAvatar player)
    {
        if (targetType == EffectTarget.AnyPlayerOrCreature ||
            targetType == EffectTarget.TargetPlayer ||
            (targetType == EffectTarget.PlayerOrPlayerCreature && player.tag == "PlayerOwned") ||
            (targetType == EffectTarget.OpponentOrOpponentCreature && player.tag == "OpponentOwned") ||
            (targetType == EffectTarget.Player && player.tag == "PlayerOwned") ||
            (targetType == EffectTarget.Opponent && player.tag == "OpponentOwned"))
        {
            var conditionsFullfilled = true;
            var playerTarget = effectTarget as PlayerTargetBase;
            foreach (var condition in playerTarget.conditions)
            {
                if (!condition.IsTrue(player.playerInfo))
                {
                    conditionsFullfilled = false;
                    break;
                }
            }
            if (conditionsFullfilled)
            {
                validTargetSelected = true;
                selectedPlayer = player;
                selectedCard = null;
                targetInfo.Clear();
                targetInfo.Add(player.index);
                CreateTarget(player.transform.position);
            }
            else
            {
                validTargetSelected = false;
            }
        }
        else
        {
            validTargetSelected = false;
        }
    }

    public override void OnPlayerUnselected(PlayerAvatar player)
    {
        if (selectedPlayer != null && selectedPlayer == player)
        {
            Destroy(target);
            selectedPlayer = null;
            validTargetSelected = false;
            targetInfo.Clear();
        }
    }
}
