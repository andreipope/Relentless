// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DeckBuilderCard : MonoBehaviour
    {
        public DeckEditingPage scene;
        public Card card;
        public bool isActive;
        public bool isHordeItem = false;
        public float doubleClickDelay = 0.5f;

        private float _lastClickTime;
        private int _clickCount;

        private void Awake()
        {
            isActive = true;
        }

        private void Update()
        {
            if (isActive && Input.GetMouseButtonDown(0))
            {
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(mousePos, Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
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
                        DoAction();

                        _lastClickTime = Time.unscaledTime;
                        _clickCount = 0;
                    }
                }
            }
        }

        private void DoAction() {
            if (!isHordeItem)
                scene.AddCardToDeck(this, card);
            else
                scene.RemoveCardFromDeck(this, card);
        }
    }
}