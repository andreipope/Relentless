using System.Collections.Generic;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using UnityEngine;

public class ActionData
{
    public List<ActionItem> Actions;

    public void ParseData()
    {
        if (Actions != null)
        {
            foreach (ActionItem action in Actions)
            {
                action.ParseData();
            }
        }
    }

    public List<ActionItem> GetActions(Enumerators.AiActionType[] types)
    {
        List<ActionItem> allActions = new List<ActionItem>();
        ActionItem act = null;
        foreach (Enumerators.AiActionType type in types)
        {
            act = Actions.Find(x => x.Type == type);
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
    public string ActionType;

    public List<ActionState> States;

    [JsonIgnore]
    public Enumerators.AiActionType Type;

    public void ParseData()
    {
        if (ActionType != null)
        {
            Type = Utilites.CastStringTuEnum<Enumerators.AiActionType>(ActionType);
        }

        foreach (ActionState state in States)
        {
            state.ParseData();
        }
    }
}

public class ActionState
{
    public int ActionStateIndex;

    public int CardId;

    public string TargetType;

    [JsonIgnore]
    public List<Enumerators.AbilityTargetType> PriorityTargetTypes;

    public void ParseData()
    {
        if (TargetType != null)
        {
            PriorityTargetTypes = Utilites.CastList<Enumerators.AbilityTargetType>(TargetType);
        }
    }
}
