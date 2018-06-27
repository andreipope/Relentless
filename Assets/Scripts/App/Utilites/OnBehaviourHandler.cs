using System;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class OnBehaviourHandler : MonoBehaviour
    {
        public event Action<GameObject> OnMouseUpEvent;
        public event Action<GameObject> OnMouseDownEvent;
        public event Action<Collider2D> OnTriggerEnter2DEvent;
        public event Action<Collider2D> OnTriggerExit2DEvent;
        public event Action<Collider> OnTriggerEnterEvent;
        public event Action<Collider> OnTriggerExitEvent;

        private void OnMouseUp()
        {
            OnMouseUpEvent?.Invoke(gameObject);
        }

        private void OnMouseDown()
        {
            OnMouseDownEvent?.Invoke(gameObject);
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            OnTriggerExit2DEvent?.Invoke(collider);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            OnTriggerEnter2DEvent?.Invoke(collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            OnTriggerExitEvent?.Invoke(collider);
        }

        private void OnTriggerEnter(Collider collider)
        {
            OnTriggerEnterEvent?.Invoke(collider);
        }
    }
}