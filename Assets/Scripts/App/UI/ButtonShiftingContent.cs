using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class ButtonShiftingContent : Button
    {
        public Vector3 ShiftValue;

        public Transform ShiftedChild;

        public bool ShiftOnHighlight = true;

        public bool ShiftOnPress = true;

        private Vector3 _originalShiftedChildPosition;

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            UpdateShiftedChildPosition();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            UpdateShiftedChildPosition();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            UpdateShiftedChildPosition();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            UpdateShiftedChildPosition();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            UpdateShiftedChildPosition();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            UpdateShiftedChildPosition();
        }

        public override void Select()
        {
            base.Select();
            UpdateShiftedChildPosition();
        }

        protected override void Start()
        {
            base.Start();

            if (ShiftedChild != null)
			{
                _originalShiftedChildPosition = ShiftedChild.localPosition;
			}
        }

        protected virtual void UpdateShiftedChildPosition()
        {
            if (ShiftedChild == null)
                return;

            bool isShift = false;
            switch (currentSelectionState)
            {
                case SelectionState.Normal:
                    break;
                case SelectionState.Highlighted:
                    if (ShiftOnHighlight)
                    {
                        isShift = true;
                    }

                    break;
                case SelectionState.Pressed:
                    if (ShiftOnPress)
                    {
                        isShift = true;
                    }

                    break;
                case SelectionState.Disabled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ShiftedChild.localPosition =
                isShift ? _originalShiftedChildPosition + ShiftValue : _originalShiftedChildPosition;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (ShiftedChild == null)
                return;

            if (ShiftedChild == transform || !ShiftedChild.IsChildOf(transform))
            {
                ShiftedChild = null;
            }
        }
#endif
    }
}
