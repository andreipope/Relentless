// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// Miscellaneous editor functionality.
    /// </summary>
    public class EditorUtils
    {
        public static ReorderableList SetupReorderableList<T>(string headerText, List<T> elements, ref T currentElement, Action<Rect, T> drawElement, Action<T> selectElement, Action createElement, Action<T> removeElement)
        {
            var list = new ReorderableList(elements, typeof(T), true, true, true, true);

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, headerText);
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = elements[index];
                drawElement(rect, element);
            };

            list.onSelectCallback = (ReorderableList l) =>
            {
                var selectedElement = elements[list.index];
                selectElement(selectedElement);
            };

            if (createElement != null)
            {
                list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
                {
                    createElement();
                };
            }

            list.onRemoveCallback = (ReorderableList l) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this item?", "Yes", "No"))
                {
                    var element = elements[l.index];
                    removeElement(element);
                    ReorderableList.defaultBehaviours.DoRemoveButton(l);
                }
            };

            return list;
        }
    }
}
