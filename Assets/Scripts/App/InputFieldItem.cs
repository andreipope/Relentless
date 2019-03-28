using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class InputFieldItem : InputField
    {
        private IMobileKeyboardManager _mobileKeyboardManager;

        public override void OnSelect(BaseEventData eventData)
        {
#if !UNITY_EDITOR
            _mobileKeyboardManager.DrawKeyboard(text, true, keyboardType, multiline: multiLine);
#endif

        }

        public override void OnPointerClick(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            _mobileKeyboardManager.DrawKeyboard(text, true, keyboardType, multiline: multiLine);
#endif
        }

        public override void OnDeselect(BaseEventData eventData)
        {
#if !UNITY_EDITOR
            _mobileKeyboardManager.HideKeyboard(false, false);
#endif
        }

        public override void Select()
        {
            base.Select();
        }

        protected override void Start()
        {
            base.Start();
            _mobileKeyboardManager = GameClient.Get<IMobileKeyboardManager>();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }
}
