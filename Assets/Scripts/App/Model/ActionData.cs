// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using Loom.Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionData  
{
    public List<ActionItem> actions;

    public ActionData()
    {
    }

    public void ParseData()
    {
		if (actions != null) {
			foreach (var action in actions) {
				action.ParseData ();
			}
		}
    }

    public List<ActionItem> GetActions(Enumerators.AIActionType[] types)
    {
        List<ActionItem> allActions = new List<ActionItem>();
        ActionItem act = null;
        foreach (var type in types)
        {
            act = actions.Find((x) => x.type == type);
            if (act != null)
                allActions.Add(act);
            else
                Debug.LogError("Type not found!");
        }
        return allActions;
    }
}

public class ActionItem
{
    public string actionType;
    public List<ActionState> states;

    [JsonIgnore]
    public Enumerators.AIActionType type;

    public void ParseData()
    {
        if (actionType != null)
            type = Utilites.CastStringTuEnum<Enumerators.AIActionType>(actionType);
        foreach (var state in states)
        {
            state.ParseData();
        }
    }
}

public class ActionState
{
    public int actionStateIndex;
    public int cardId;
    public string targetType;

    [JsonIgnore]
    public List<Enumerators.AbilityTargetType> priorityTargetTypes;

    public ActionState()
    { }

    public void ParseData()
    {
        if (targetType != null)
            priorityTargetTypes = Utilites.CastList<Enumerators.AbilityTargetType>(targetType);
    }
}
