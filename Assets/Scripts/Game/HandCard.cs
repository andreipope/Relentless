// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

[RequireComponent(typeof(CardView))]
public class HandCard : MonoBehaviour
{
    public DemoHumanPlayer ownerPlayer;
    public GameObject boardZone;

    protected CardView cardView;

    protected bool startedDrag;
    protected Vector3 initialPos;

    private void Awake()
    {
        cardView = GetComponent<CardView>();
    }

    private void Start()
    {
        if (cardView.CanBePlayed(ownerPlayer))
        {
            cardView.SetHighlightingEnabled(true);
        }
        else
        {
            cardView.SetHighlightingEnabled(false);
        }
    }

    private void Update()
    {
        if (startedDrag)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var newPos = transform.position;
            newPos.z = 0;
            transform.position = newPos;
        }
    }

    public void OnSelected()
    {
        if (ownerPlayer.isActivePlayer &&
            cardView.CanBePlayed(ownerPlayer))
        {
            startedDrag = true;
            initialPos = transform.position;
            ownerPlayer.isCardSelected = true;
        }
    }

    public void OnMouseUp()
    {
        if (!startedDrag)
        {
            return;
        }

        startedDrag = false;
        ownerPlayer.isCardSelected = false;

        if (boardZone.GetComponent<BoxCollider2D>().bounds.Contains(transform.position))
        {
            ownerPlayer.PlayCard(cardView);
            cardView.SetHighlightingEnabled(false);
        }
        else
        {
            transform.position = initialPos;
        }
    }

    public void ResetToInitialPosition()
    {
        transform.position = initialPos;
    }
}
