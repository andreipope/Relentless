// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Data;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class DeckBuilderCard : MonoBehaviour, IPointerClickHandler, IScrollHandler
    {
        public DeckEditingPage scene;
        public Card card;
        public bool isActive;
        public bool isHordeItem = false;

        public float doubleClickDelay = 0.3f;

        private float _lastClickTime;
        private int _clickCount;

        private void Awake()
        {
            isActive = true;
        }

        private void Update() {
            if (_clickCount == 1 && Time.unscaledTime > _lastClickTime + doubleClickDelay)
            {
                SingleClickAction();
                _clickCount = 0;
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (_clickCount == 0 || Time.unscaledTime - _lastClickTime < doubleClickDelay)
            {
                _clickCount++;
                _lastClickTime = Time.unscaledTime;
            } else
            {
                _lastClickTime = Time.unscaledTime;
                _clickCount = 1;
            }

            if (_clickCount == 2)
            {
                DoubleClickAction();

                _lastClickTime = Time.unscaledTime;
                _clickCount = 0;
            }
        }

        private void SingleClickAction() {
            scene.SelectCard(this, card);
        }

        private void DoubleClickAction() {
            Debug.Log("double click");
            if (!isHordeItem)
                scene.AddCardToDeck(this, card);
            else
                scene.RemoveCardFromDeck(this, card);
        }

        public void OnScroll(PointerEventData eventData) {
            scene.ScrollCardList(isHordeItem, eventData.scrollDelta);
        }
    }
}