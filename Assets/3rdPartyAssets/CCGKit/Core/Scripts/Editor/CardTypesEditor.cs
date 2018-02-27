// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Reflection;
using System.Linq;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor's 'Card types' tab.
    /// </summary>
    public class CardTypesEditor : EditorTab
    {
        private ReorderableList cardTypesList;
        private CardType currentCardType;

        private ReorderableList currentCardTypeStatsList;
        private DefinitionStat currentCardTypeStat;

        private ReorderableList currentCardTypePropertiesList;
        private Property currentCardTypeProperty;

        private ReorderableList currentCardTypeDestroyConditionsList;
        private DestroyCardCondition currentCardTypeDestroyCondition;

        public CardTypesEditor(GameConfiguration config) : base(config)
        {
            cardTypesList = EditorUtils.SetupReorderableList("Card types", gameConfig.cardTypes, ref currentCardType, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentCardType = x;
                currentCardTypeProperty = null;
                currentCardTypeStat = null;
                currentCardTypeDestroyCondition = null;
                CreateCurrentCardTypePropertiesList();
                CreateCurrentCardTypeStatsList();
                CreateCurrentCardTypeDestroyConditionsList();
            },
            () =>
            {
                var cardType = new CardType();
                gameConfig.cardTypes.Add(cardType);
            },
            (x) =>
            {
                currentCardType = null;
                currentCardTypeProperty = null;
                currentCardTypeStat = null;
                currentCardTypeDestroyCondition = null;
            });
        }

        private void CreateCurrentCardTypeStatsList()
        {
            currentCardTypeStatsList = EditorUtils.SetupReorderableList("Card stats", currentCardType.stats, ref currentCardTypeStat, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentCardTypeStat = x;
            },
            () =>
            {
                var stat = new CardStat();
                if (currentCardType.stats.Count > 0)
                {
                    stat.id = currentCardType.stats.Max(x => x.id) + 1;
                }
                else
                {
                    stat.id = 0;
                }
                currentCardType.stats.Add(stat);
            },
            (x) =>
            {
                currentCardTypeStat = null;
            });
        }

        private void CreateCurrentCardTypePropertiesList()
        {
            currentCardTypePropertiesList = EditorUtils.SetupReorderableList("Card properties", currentCardType.properties, ref currentCardTypeProperty, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
            },
            (x) =>
            {
                currentCardTypeProperty = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                var effectTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(Property));
                foreach (var type in effectTypes)
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.Name)), false, CreateCardPropertyCallback, type);
                }
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentCardTypeProperty = null;
            });
        }

        private void CreateCurrentCardTypeDestroyConditionsList()
        {
            currentCardTypeDestroyConditionsList = EditorUtils.SetupReorderableList("Destroy conditions", currentCardType.destroyConditions, ref currentCardTypeDestroyCondition, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.GetReadableString(gameConfig));
            },
            (x) =>
            {
                currentCardTypeDestroyCondition = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                var effectTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(DestroyCardCondition));
                foreach (var type in effectTypes)
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.Name)), false, CreateDestroyConditionCallback, type);
                }
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentCardTypeDestroyCondition = null;
            });
        }

        private void CreateCardPropertyCallback(object obj)
        {
            var property = Activator.CreateInstance((Type)obj) as Property;
            currentCardType.properties.Add(property);
        }

        private void CreateDestroyConditionCallback(object obj)
        {
            var condition = Activator.CreateInstance((Type)obj) as DestroyCardCondition;
            currentCardType.destroyConditions.Add(condition);
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (cardTypesList != null)
            {
                cardTypesList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentCardType != null)
            {
                DrawCardType(currentCardType);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawCardType(CardType cardType)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorConfig.RegularLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            cardType.name = EditorGUILayout.TextField(cardType.name, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(400));
            var statsHelpText = "Card stats are fields that can change throughout the course of a game. " +
                "Examples of common stats are: attack, health, etc. " +
                "They are numeric and always transmitted over the network.";
            EditorGUILayout.HelpBox(statsHelpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (currentCardTypeStatsList != null)
            {
                currentCardTypeStatsList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentCardTypeStat != null)
            {
                DrawDefinitionStat(currentCardTypeStat);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(400));
            var propertiesHelpText = "Card properties are fields that never change throughout the course of a game. " +
                "Examples of common properties are: the body text of the card, its collector number, etc. " +
                "They are never transmitted over the network, as they do not have any gameplay relevance.";
            EditorGUILayout.HelpBox(propertiesHelpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (currentCardTypePropertiesList != null)
            {
                currentCardTypePropertiesList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentCardTypeProperty != null)
            {
                DrawProperty(currentCardTypeProperty);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (currentCardTypeDestroyConditionsList != null)
            {
                currentCardTypeDestroyConditionsList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentCardTypeDestroyCondition != null)
            {
                DrawDestroyCardCondition(currentCardTypeDestroyCondition);
            }

            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 170;
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Move after triggering effect");
            currentCardType.moveAfterTriggeringEffect = EditorGUILayout.Toggle(currentCardType.moveAfterTriggeringEffect, GUILayout.MaxWidth(EditorConfig.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            if (currentCardType.moveAfterTriggeringEffect)
            {
                var fields = currentCardType.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                for (var i = 0; i < fields.Length; i++)
                {
                    var attribute = Attribute.GetCustomAttribute(fields[i], typeof(FieldAttribute)) as FieldAttribute;
                    if (attribute != null)
                    {
                        GUILayout.BeginHorizontal();
                        attribute.Draw(gameConfig, currentCardType, ref fields[i]);
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        private void DrawProperty(Property property)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.BeginVertical();

            property.Draw();

            if (GUILayout.Button("Copy to cards", GUILayout.MaxWidth(100)))
            {
                foreach (var set in gameConfig.cardSets)
                {
                    foreach (var card in set.cards.FindAll(x => x.cardTypeId == currentCardType.id))
                    {
                        if (property is IntProperty)
                        {
                            var propertyCopy = new IntProperty();
                            propertyCopy.name = property.name;
                            propertyCopy.value = (property as IntProperty).value;
                            card.properties.Add(propertyCopy);
                        }
                        else if (property is StringProperty)
                        {
                            var propertyCopy = new StringProperty();
                            propertyCopy.name = property.name;
                            propertyCopy.value = (property as StringProperty).value;
                            card.properties.Add(propertyCopy);
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        private void DrawDestroyCardCondition(DestroyCardCondition condition)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.BeginVertical();

            var fields = condition.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
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
