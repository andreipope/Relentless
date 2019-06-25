using System;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    public class TextMeshProUGUIWithEvents : TextMeshProUGUI
    {
        public event Action TextMeshGenerated;

        protected override void GenerateTextMesh()
        {
            base.GenerateTextMesh();
            TextMeshGenerated?.Invoke();
        }
    }
}
