using System;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class OnMouseHandler : MonoBehaviour
    {
        public event Action<GameObject> OnMouseUpEvent;
        public event Action<GameObject> OnMouseDownEvent;

        private void OnMouseUp()
        {
            if (OnMouseUpEvent != null)
                OnMouseUpEvent(gameObject);
        }

        private void OnMouseDown()
        {
            if (OnMouseDownEvent != null)
                OnMouseDownEvent(gameObject);
        }
    }
}