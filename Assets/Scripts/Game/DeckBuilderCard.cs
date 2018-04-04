using UnityEngine;
using GrandDevs.CZB;
using GrandDevs.CZB.Data;

public class DeckBuilderCard : MonoBehaviour
{
    public DeckEditingPage scene;
    public Card card;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                scene.AddCardToDeck(card);
            }
        }
    }
}
