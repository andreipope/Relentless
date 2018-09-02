using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    [Serializable]
    public class MultiPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public float DoubleClickDelay = 0.3f;

        private float _lastClickTime;

        private int _clickCount;

        public event Action SingleClickReceived;

        public event Action DoubleClickReceived;

        public void OnPointerClick(PointerEventData eventData)
        {
            if ((_clickCount == 0) || (Time.unscaledTime - _lastClickTime < DoubleClickDelay))
            {
                _clickCount++;
                _lastClickTime = Time.unscaledTime;
            }
            else
            {
                _lastClickTime = Time.unscaledTime;
                _clickCount = 1;
            }

            if (_clickCount == 2)
            {
                DoubleClickReceived?.Invoke();

                _lastClickTime = Time.unscaledTime;
                _clickCount = 0;
            }
        }

        public void Update()
        {
            if ((_clickCount == 1) && (Time.unscaledTime > _lastClickTime + DoubleClickDelay))
            {
                SingleClickReceived?.Invoke();
                _clickCount = 0;
            }
        }
    }
}
