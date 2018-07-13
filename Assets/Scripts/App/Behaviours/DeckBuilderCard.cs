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
                    if(!isHordeItem)
                        scene.AddCardToDeck(card);
                    else
                        scene.RemoveCardFromDeck(card);
                }
            }
        }
    }
}