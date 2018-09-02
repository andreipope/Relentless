using System.Collections.Generic;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using UnityEngine;

public class ActionData
{
    public List<ActionItem> actions;

    public void ParseData()
    {
        if (actions != null)
        {
            foreach (ActionItem action in actions)
            {
                action.ParseData();
            }
        }
    }

    public List<ActionItem> GetActions(Enumerators.AIActionType[] types)
    {
        List<ActionItem> allActions = new List<ActionItem>();
        ActionItem act = null;
        foreach (Enumerators.AIActionType type in types)
        {
            act = actions.Find(x => x.type == type);
            if (act != null)
            {
                allActions.Add(act);
            } else
            {
                Debug.LogError("Type not found!");
            }
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
        {
            type = Utilites.CastStringTuEnum<Enumerators.AIActionType>(actionType);
        }

        foreach (ActionState state in states)
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

    public void ParseData()
    {
        if (targetType != null)
        {
            priorityTargetTypes = Utilites.CastList<Enumerators.AbilityTargetType>(targetType);
        }
    }
}
