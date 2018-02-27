// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor's 'Game configuration' tab.
    /// </summary>
    public class GameConfigurationEditor : EditorTab
    {
        private ReorderableList gameStartActionList;
        private GameAction currentGameStartAction;

        private ReorderableList turnStartActionList;
        private GameAction currentTurnStartAction;

        private ReorderableList turnEndActionList;
        private GameAction currentTurnEndAction;

        private ReorderableList endGameConditionList;
        private EndGameCondition currentEndGameCondition;

        public GameConfigurationEditor(GameConfiguration config) : base(config)
        {
            gameStartActionList = EditorUtils.SetupReorderableList("Game start actions", gameConfig.properties.gameStartActions, ref currentGameStartAction, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentGameStartAction = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                var actionTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(GameAction));
                foreach (var action in actionTypes)
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(action.Name)), false, CreateGameStartActionCallback, action);
                }
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentGameStartAction = null;
            });

            turnStartActionList = EditorUtils.SetupReorderableList("Turn start actions", gameConfig.properties.turnStartActions, ref currentTurnStartAction, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentTurnStartAction = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                var actionTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(GameAction));
                foreach (var action in actionTypes)
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(action.Name)), false, CreateTurnStartActionCallback, action);
                }
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentTurnStartAction = null;
            });

            turnEndActionList = EditorUtils.SetupReorderableList("Turn end actions", gameConfig.properties.turnEndActions, ref currentTurnEndAction, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentTurnEndAction = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                var actionTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(GameAction));
                foreach (var action in actionTypes)
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(action.Name)), false, CreateTurnEndActionCallback, action);
                }
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentTurnEndAction = null;
            });

            endGameConditionList = EditorUtils.SetupReorderableList("End game conditions", gameConfig.properties.endGameConditions, ref currentEndGameCondition, (rect, x) =>
            {
                var conditionString = x.type.ToString() + ": " + x.GetReadableString(gameConfig);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), conditionString);
            },
            (x) =>
            {
                currentEndGameCondition = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                var conditionTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(EndGameCondition));
                foreach (var condition in conditionTypes)
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(condition.Name)), false, CreateEndGameConditionCallback, condition);
                }
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentEndGameCondition = null;
            });
        }

        private void CreateGameStartActionCallback(object obj)
        {
            var action = Activator.CreateInstance((Type)obj);
            gameConfig.properties.gameStartActions.Add(action as GameAction);
        }

        private void CreateTurnStartActionCallback(object obj)
        {
            var action = Activator.CreateInstance((Type)obj);
            gameConfig.properties.turnStartActions.Add(action as GameAction);
        }

        private void CreateTurnEndActionCallback(object obj)
        {
            var action = Activator.CreateInstance((Type)obj);
            gameConfig.properties.turnEndActions.Add(action as GameAction);
        }

        private void CreateEndGameConditionCallback(object obj)
        {
            var condition = Activator.CreateInstance((Type)obj);
            gameConfig.properties.endGameConditions.Add(condition as EndGameCondition);
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Turn duration");
            gameConfig.properties.turnDuration = EditorGUILayout.IntField(gameConfig.properties.turnDuration, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Minimum deck size");
            gameConfig.properties.minDeckSize = EditorGUILayout.IntField(gameConfig.properties.minDeckSize, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Maximum deck size");
            gameConfig.properties.maxDeckSize = EditorGUILayout.IntField(gameConfig.properties.maxDeckSize, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (gameStartActionList != null)
            {
                gameStartActionList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentGameStartAction != null)
            {
                DrawGameAction(currentGameStartAction);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (turnStartActionList != null)
            {
                turnStartActionList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentTurnStartAction != null)
            {
                DrawGameAction(currentTurnStartAction);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (turnEndActionList != null)
            {
                turnEndActionList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentTurnEndAction != null)
            {
                DrawGameAction(currentTurnEndAction);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(350));
            if (endGameConditionList != null)
            {
                endGameConditionList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentEndGameCondition != null)
            {
                DrawEndGameCondition(currentEndGameCondition);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawGameAction(GameAction action)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorConfig.LargeLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Target");
            action.target = (GameActionTarget)EditorGUILayout.EnumPopup(action.target, GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();

            var fields = action.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < fields.Length; i++)
            {
                var attribute = Attribute.GetCustomAttribute(fields[i], typeof(FieldAttribute)) as FieldAttribute;
                if (attribute != null)
                {
                    GUILayout.BeginHorizontal();
                    attribute.Draw(gameConfig, action, ref fields[i]);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        private void DrawEndGameCondition(EndGameCondition condition)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorConfig.LargeLabelWidth;

            GUILayout.BeginVertical();

            var fields = condition.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(x => ((OrderAttribute)x.GetCustomAttributes(typeof(OrderAttribute), false)[0]).Order).ToArray();
            for (var i = 0; i < fields.Length; i++)
            {
                var attribute = Attribute.GetCustomAttribute(fields[i], typeof(FieldAttribute)) as FieldAttribute;
                if (attribute != null)
                {
                    GUILayout.BeginHorizontal();
                    attribute.Draw(gameConfig, condition, ref fields[i]);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
