// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class OnBehaviourHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action<GameObject> OnMouseUpEvent;
        public event Action<GameObject> OnMouseDownEvent;
        public event Action<Collider2D> OnTriggerEnter2DEvent;
        public event Action<Collider2D> OnTriggerExit2DEvent;
        public event Action<Collider> OnTriggerEnterEvent;
        public event Action<Collider> OnTriggerExitEvent;
        public event Action<GameObject> OnDestroyEvent;
        public event Action<PointerEventData> OnPointerEnterEvent;
        public event Action<PointerEventData> OnPointerExitEvent;
        public event Action<GameObject> OnUpdateEvent;
        public event Action<PointerEventData, GameObject> OnDragEvent;
        public event Action<PointerEventData, GameObject> OnBeginDragEvent;
        public event Action<PointerEventData, GameObject> OnEndDragEvent;


        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterEvent?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitEvent?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData, gameObject);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent?.Invoke(eventData, gameObject);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData, gameObject);
        }

        private void Update()
        {
            OnUpdateEvent?.Invoke(gameObject);
        }

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

        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke(gameObject);
        }
    }
}